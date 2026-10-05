using System;
using System.Collections.Generic;
using System.Diagnostics;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Utilities;

internal class UpdateQueue
{
    private static UpdateQueue? _instance;

    private readonly Queue<PlayerDisplay> queue = new();
    private readonly ICoroutineRunner coroutineRunner;
    private readonly TimeSpan maxUpdateTimePerFrame;
    private readonly Stopwatch stopwatch = new();

    private ICoroutine coroutine;

    public static UpdateQueue Instance => _instance ??= new UpdateQueue(new UnityCoroutineRunner(), TimeSpan.FromMilliseconds(2));

    public int Count => queue.Count;

    internal UpdateQueue(ICoroutineRunner coroutineRunner, TimeSpan maxUpdateTimePerFrame)
    {
        this.coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
        this.maxUpdateTimePerFrame = maxUpdateTimePerFrame;
        coroutine = coroutineRunner.StartCoroutine(CoroutineMethod());
    }

    public void Enqueue(PlayerDisplay display)
    {
        if (!coroutine.IsRunning)
            coroutine = coroutineRunner.StartCoroutine(CoroutineMethod());

        queue.Enqueue(display);
    }

    internal void ProcessFrame()
    {
        stopwatch.Restart();

        bool updatedAny = false;

        while (queue.Count > 0 && (!updatedAny || stopwatch.Elapsed < maxUpdateTimePerFrame))
        {
            queue.Dequeue().UpdateDisplay();
            updatedAny = true;
        }
    }

    private IEnumerator<float> CoroutineMethod()
    {
        while (true)
        {
            yield return -1f;

            try
            {
                ProcessFrame();
            }
            catch (Exception ex)
            {
                LogManager.Error(ex.ToString());
            }
        }
    }
}