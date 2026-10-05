using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Models.Transition;

public class Transition
{
    private float duration;
    private IAnimationCurve curve;
    private EasingType easing;

    /// <summary>
    ///     Gets or sets the duration of the transition in seconds. If value is below or equal to zero, it will be set to
    ///     0.001f to avoid issues.
    /// </summary>
    public float Duration
    {
        get => duration;

        set
        {
            if (value <= 0)
                value = 0.001f;

            duration = value;
        }
    }

    /// <summary>
    ///     Gets or sets the easing type used. The curve use by transition will be generated based on this easing type.
    ///     If set to <see cref="EasingType.Custom" />, the curve will be defaultly set to an EaseInOut curve.
    /// </summary>
    /// <remarks>
    ///     The easing function determines the rate of change of a value over time, allowing for
    ///     smooth transitions.
    /// </remarks>
    public EasingType Easing
    {
        get => easing;

        set
        {
            curve = CurveFactory.BuildNormalized(value);
            easing = value;
        }
    }

    public IAnimationCurve NormalizedCurve
    {
        get => curve;

        set
        {
            curve = value;
            easing = EasingType.Custom;
        }
    }

    internal static IAnimationCurveFactory CurveFactory { get; set; } = new UnityAnimationCurveFactory();

    private Transition(IAnimationCurve curve, EasingType easing, float duration)
    {
        this.curve = curve;
        this.easing = easing;
        this.duration = duration;
    }

    public static Transition Get(IAnimationCurve normalizedCurve, float duration = 0.5f)
    {
        return new Transition(normalizedCurve, EasingType.Custom, duration);
    }

    public static Transition Get(EasingType type = EasingType.EaseInOut, float duration = 0.5f)
    {
        return new Transition(CurveFactory.BuildNormalized(type), type, duration);
    }

    internal IAnimationCurve GetCurve(float from, float to)
    {
        if (curve is null)
            curve = CurveFactory.BuildNormalized(easing);

        float range = to - from;
        HsmKeyFrame[] keys = curve.Keys;
        HsmWrapMode postWrapMode = curve.PostWrapMode;

        int frameCount = keys.Length;

        if (postWrapMode == HsmWrapMode.Once)
            frameCount += 1;

        HsmKeyFrame[] scaled = new HsmKeyFrame[frameCount];

        for (int i = 0; i < keys.Length; i++)
            scaled[i] = new HsmKeyFrame(keys[i].Time * duration, from + keys[i].Value * range, keys[i].InTangent * range / duration, keys[i].OutTangent * range / duration);

        if (postWrapMode == HsmWrapMode.Once)
            scaled[frameCount - 1] = new HsmKeyFrame(99999f, to);

        IAnimationCurve result = CurveFactory.Build(scaled);

        result.PreWrapMode = curve.PreWrapMode;
        result.PostWrapMode = postWrapMode;

        return result;
    }

    internal float Evaluate(float time, float start, float end)
    {
        if (curve is null)
            return end;

        if (time > duration)
            return end;
        if (time < 0)
            return start;

        float dif = end - start;

        return start + dif * curve.Evaluate(time / duration);
    }
}