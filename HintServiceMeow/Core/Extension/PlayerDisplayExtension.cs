using System;
using System.Runtime.CompilerServices;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Extension;

/// <summary>
///     Provides extension methods for <see cref="PlayerDisplay" /> to schedule timed hint removal.
/// </summary>
public static class PlayerDisplayExtension
{
    private static readonly ConditionalWeakTable<PlayerDisplay, ConditionalWeakTable<AbstractHint, ICoroutine>> RemoveTimers = new();

    internal static ICoroutineRunner CoroutineRunner { get; set; } = new UnityCoroutineRunner();

    /// <summary>
    ///     Remove a hint after a delay. If a removal task is in progress, it will be reset.
    /// </summary>
    /// <param name="playerDisplay">The PlayerDisplay owning the hint.</param>
    /// <param name="hint">The hint to remove.</param>
    /// <param name="delay">How long until the hint is removed.</param>
    public static void RemoveAfter(this PlayerDisplay playerDisplay, AbstractHint hint, float delay)
    {
        if (!RemoveTimers.TryGetValue(playerDisplay, out ConditionalWeakTable<AbstractHint, ICoroutine> hintTimers))
        {
            hintTimers = new ConditionalWeakTable<AbstractHint, ICoroutine>();

            RemoveTimers.Add(playerDisplay, hintTimers);
        }

        if (hintTimers.TryGetValue(hint, out ICoroutine oldTimer))
        {
            oldTimer.Kill();
            hintTimers.Remove(hint);
        }

        hintTimers.Add(hint, CoroutineRunner.CallAfter(TimeSpan.FromSeconds(delay), () =>
        {
            hintTimers.Remove(hint);
            playerDisplay.InternalRemoveHint(null, hint);
        }));
    }
}