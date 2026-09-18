using System;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities;

internal class PeriodicRunner : IDisposable
{
    private readonly CancellationTokenSource cts = new();
    private readonly TimeSpan interval;
    private readonly Func<Task> actionAsync;
    private readonly object pauseLock = new();

    private bool paused;

    public Task CurrentTask { get; }

    private PeriodicRunner(Func<Task> actionAsync, TimeSpan interval, bool runImmediately = false)
    {
        this.actionAsync = actionAsync ?? throw new ArgumentNullException(nameof(actionAsync));
        this.interval = interval >= TimeSpan.Zero ? interval : throw new ArgumentOutOfRangeException(nameof(interval));
        CurrentTask = RunLoopAsync(runImmediately, cts.Token);
    }

    public static PeriodicRunner Start(Func<Task> actionAsync, TimeSpan interval, bool runImmediately = false)
    {
        return new PeriodicRunner(actionAsync, interval, runImmediately);
    }

    public void Pause()
    {
        lock (pauseLock)
        {
            paused = true;
        }
    }

    public void Resume()
    {
        lock (pauseLock)
        {
            paused = false;
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private async Task RunLoopAsync(bool runImmediately, CancellationToken token)
    {
        try
        {
            if (runImmediately)
                await InvokeActionSafeAsync(token).ConfigureAwait(false);

            DateTime nextDue = DateTime.UtcNow + interval;

            while (!token.IsCancellationRequested)
            {
                TimeSpan delay = nextDue - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, token).ConfigureAwait(false);

                if (!IsPaused())
                {
                    await InvokeActionSafeAsync(token).ConfigureAwait(false);
                    nextDue = DateTime.UtcNow + interval;
                }
                else
                {
                    // Paused
                    await Task.Delay(interval, token).ConfigureAwait(false);
                }
            }
        }
        catch (TaskCanceledException)
        { } // Action cancelled
    }

    private bool IsPaused()
    {
        lock (pauseLock)
        {
            return paused;
        }
    }

    private async Task InvokeActionSafeAsync(CancellationToken token)
    {
        try
        {
            await actionAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Cancellation requested, do nothing
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error in periodic action: {ex.Message}");
        }
    }
}