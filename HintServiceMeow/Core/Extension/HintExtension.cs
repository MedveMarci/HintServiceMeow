using System;
using System.Runtime.CompilerServices;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Extension;

/// <summary>
///     Provides extension methods for <see cref="AbstractHint" /> to schedule timed visibility changes.
/// </summary>
public static class HintExtension
{
    private static readonly ConditionalWeakTable<AbstractHint, ICoroutine> HideTimers = new();

    internal static ICoroutineRunner CoroutineRunner { get; set; } = new UnityCoroutineRunner();

    /// <summary>
    ///     Set Hint.Hide to true after a delay. If a hiding task is in progress, it will be reset.
    /// </summary>
    /// <param name="hint">The hint to hide.</param>
    /// <param name="delay">How much time in seconds to wait until hiding the hint.</param>
    public static void HideAfter(this AbstractHint hint, float delay)
    {
        if (HideTimers.TryGetValue(hint, out ICoroutine oldTimer))
        {
            oldTimer.Kill();
            HideTimers.Remove(hint);
        }

        HideTimers.Add(hint, CoroutineRunner.CallAfter(TimeSpan.FromSeconds(delay), () =>
        {
            HideTimers.Remove(hint);
            hint.Hide = true;
        }));
    }
}