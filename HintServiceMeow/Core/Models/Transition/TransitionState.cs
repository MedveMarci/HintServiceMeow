using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Models.Transition;

internal class TransitionState(Transition transition, float fromValue, float toValue, double? startTime = null)
{
    public Transition Transition { get; } = transition;

    public double StartTime { get; } = startTime ?? NetworkTimeCache.Time;

    public float FromValue { get; } = fromValue;

    public float ToValue { get; } = toValue;

    public float Duration => Transition.Duration;

    public bool IsExpired => NetworkTimeCache.Time - StartTime >= Duration;

    public float CurrentValue
    {
        get
        {
            double time = NetworkTimeCache.Time;

            if (time - StartTime >= Duration) // If is expired
                return ToValue;

            float elapsed = (float)(time - StartTime);
            return Transition.Evaluate(elapsed, FromValue, ToValue);
        }
    }
}