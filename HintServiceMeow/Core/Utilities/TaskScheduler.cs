using System;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities;

internal class TaskScheduler : ITaskScheduler
{
    private readonly ReaderWriterLockSlim schedulerLock = new();

    private readonly PeriodicRunner runner;

    private volatile bool disposed;

    private Func<bool> task;
    private DateTime scheduledActionTime; // Indicate when the timer will begin trying invoking the action
    private TimeSpan interval; // Minimum time between two actions
    private DateTime startTimeStamp; // Used to calculate elapsed time since last action, = DateTime.MinValue if there's no last action.
    private TimeSpan elapsed; // Time elapsed since last action, does not include the time when the scheduler is paused.

    public bool InvokeUntilSuccess { get; set; }

    public bool IsPaused { get; private set; }

    public TimeSpan Elapsed
    {
        get
        {
            if (!TryEnterReadLock())
                return elapsed;

            try
            {
                return GetElapsedUnlocked();
            }
            finally
            {
                schedulerLock.ExitReadLock();
            }
        }

        private set
        {
            if (!TryEnterWriteLock())
                return;

            try
            {
                elapsed = value; // Set elapsed time
                startTimeStamp = DateTime.Now; // Reset time stamp
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }
    }

    public TimeSpan MinInterval
    {
        get
        {
            if (!TryEnterReadLock())
                return interval;

            try
            {
                return interval;
            }
            finally
            {
                schedulerLock.ExitReadLock();
            }
        }

        set
        {
            if (!TryEnterWriteLock())
                return;

            try
            {
                if (value <= TimeSpan.Zero)
                    interval = TimeSpan.Zero;
                else
                    interval = value;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }
    }

    public bool IsReadyForNextAction => Elapsed >= interval;

    private DateTime ScheduledActionTime
    {
        get
        {
            if (!TryEnterReadLock())
                return DateTime.MaxValue;

            try
            {
                return scheduledActionTime;
            }
            finally
            {
                schedulerLock.ExitReadLock();
            }
        }

        set
        {
            if (!TryEnterWriteLock())
                return;

            try
            {
                scheduledActionTime = value;
            }
            finally
            {
                schedulerLock.ExitWriteLock();
            }
        }
    }

    public TaskScheduler(int tickRate = 30)
    {
        interval = TimeSpan.FromSeconds(0);
        task = () => true; // Default empty action
        startTimeStamp = DateTime.Now;

        runner = PeriodicRunner.Start(PeriodicRunnerMethod, TimeSpan.FromSeconds(1.0 / tickRate));
    }

    private bool TryEnterWriteLock()
    {
        if (disposed)
            return false;

        try
        {
            schedulerLock.EnterWriteLock();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private bool TryEnterReadLock()
    {
        if (disposed)
            return false;

        try
        {
            schedulerLock.EnterReadLock();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    void IDisposable.Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        runner.Dispose();

        try
        {
            schedulerLock.Dispose();
        }
        catch (SynchronizationLockException)
        { }
    }

    public void Start(TimeSpan newInterval, Action newAction)
    {
        if (newAction is null)
            throw new ArgumentNullException(nameof(newAction), "Action cannot be null.");

        if (!TryEnterWriteLock())
            return;

        try
        {
            if (newInterval <= TimeSpan.Zero)
                newInterval = TimeSpan.Zero;

            interval = newInterval;

            task = () =>
            {
                newAction();
                return true; // Return true to indicate success
            };

            // Reset Elapsed
            elapsed = TimeSpan.Zero;
            startTimeStamp = DateTime.Now;

            // Reset scheduled action time
            scheduledActionTime = DateTime.MaxValue;
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    public void Start(TimeSpan newInterval, Func<bool> newAction)
    {
        if (newAction is null)
            throw new ArgumentNullException(nameof(newAction), "Action cannot be null.");

        if (!TryEnterWriteLock())
            return;

        try
        {
            if (newInterval <= TimeSpan.Zero)
                newInterval = TimeSpan.Zero;

            // Set new interval and action
            interval = newInterval;
            task = newAction;

            // Reset Elapsed
            elapsed = TimeSpan.Zero;
            startTimeStamp = DateTime.Now;

            // Reset scheduled action time
            scheduledActionTime = DateTime.MaxValue;
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    public void Invoke(float delay = -1f, DelayType delayType = DelayType.Override)
    {
        if (!TryEnterWriteLock())
            return;

        try
        {
            // If there's not scheduled time, then set it to the current time plus delay
            if (scheduledActionTime == DateTime.MaxValue)
            {
                scheduledActionTime = DateTime.Now.AddSeconds(delay);
                return;
            }

            // If there is a scheduled time, set based on the DelayType passed in
            switch (delayType)
            {
                case DelayType.KeepFastest:
                    if (scheduledActionTime > DateTime.Now.AddSeconds(delay))
                        scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    break;
                case DelayType.KeepSlowest:
                    if (scheduledActionTime < DateTime.Now.AddSeconds(delay))
                        scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    break;
                case DelayType.Override:
                    scheduledActionTime = DateTime.Now.AddSeconds(delay);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(delayType), delayType, null);
            }
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    public void Stop()
    {
        if (!TryEnterWriteLock())
            return;

        try
        {
            // Reset the action and interval
            task = () => true; // Default empty action
            interval = TimeSpan.FromSeconds(0);
            scheduledActionTime = DateTime.MaxValue; // Reset scheduled action time

            // Reset Elapsed
            elapsed = TimeSpan.Zero;
            startTimeStamp = DateTime.Now;
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    public void Pause()
    {
        if (!TryEnterWriteLock())
            return;

        try
        {
            if (IsPaused)
                return;

            CalculateElapsedTime(); // Add time to the timer before pausing

            IsPaused = true;
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    public void Resume()
    {
        if (!TryEnterWriteLock())
            return;

        try
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            startTimeStamp = DateTime.Now; // Reset time stamp
        }
        finally
        {
            schedulerLock.ExitWriteLock();
        }
    }

    private void InvokeAction()
    {
        try
        {
            // start action
            if (task.Invoke() || !InvokeUntilSuccess)
            {
                // Reset Timer
                if (!TryEnterWriteLock())
                    return;

                try
                {
                    // Reset Elapsed
                    elapsed = TimeSpan.Zero;
                    startTimeStamp = DateTime.Now;

                    // Reset timer
                    scheduledActionTime = DateTime.MaxValue;
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);

            if (!InvokeUntilSuccess)
            {
                // Reset Timer
                if (!TryEnterWriteLock())
                    return;

                try
                {
                    // Reset Elapsed
                    elapsed = TimeSpan.Zero;
                    startTimeStamp = DateTime.Now;

                    // Reset timer
                    scheduledActionTime = DateTime.MaxValue;
                }
                finally
                {
                    schedulerLock.ExitWriteLock();
                }
            }
        }
    }

    private void CalculateElapsedTime()
    {
        // If the scheduled action time is in the future, skip
        if (startTimeStamp > DateTime.Now)
            return;

        elapsed += DateTime.Now - startTimeStamp; // Calculate elapsed time
        startTimeStamp = DateTime.Now; // Reset time stamp
    }

    private TimeSpan GetElapsedUnlocked()
    {
        if (IsPaused || startTimeStamp > DateTime.Now)
            return elapsed;

        return elapsed + (DateTime.Now - startTimeStamp);
    }

    private bool ShouldInvokeNow()
    {
        if (!TryEnterReadLock())
            return false;

        try
        {
            if (IsPaused || scheduledActionTime == DateTime.MaxValue)
                return false;

            DateTime now = DateTime.Now;
            if (scheduledActionTime > now)
                return false;

            return GetElapsedUnlocked() >= interval;
        }
        finally
        {
            schedulerLock.ExitReadLock();
        }
    }

    private Task PeriodicRunnerMethod()
    {
        try
        {
            if (!ShouldInvokeNow())
                return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
            return Task.CompletedTask;
        }

        InvokeAction();

        return Task.CompletedTask;
    }
}