using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Models.Transition;

public class Transition
{
    private readonly object @lock = new();
    private float duration;
    private IAnimationCurve curve;
    private EasingType easing;

    /// <summary>
    ///     Gets or sets the duration of the transition in seconds. If value is below or equal to zero, it will be set to
    ///     0.001f to avoid issues.
    /// </summary>
    public float Duration
    {
        get
        {
            lock (@lock)
            {
                return duration;
            }
        }

        set
        {
            lock (@lock)
            {
                if (value <= 0)
                    value = 0.001f;

                duration = value;
            }
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
        get
        {
            lock (@lock)
            {
                return easing;
            }
        }

        set
        {
            lock (@lock)
            {
                curve = CurveFactory.BuildNormalized(value);
                easing = value;
            }
        }
    }

    public IAnimationCurve NormalizedCurve
    {
        get
        {
            lock (@lock)
            {
                return curve;
            }
        }

        set
        {
            lock (@lock)
            {
                curve = value;
                easing = EasingType.Custom;
            }
        }
    }

    internal static IAnimationCurveFactory CurveFactory { get; set; } = new UnityAnimationCurveFactory();

    private Transition(IAnimationCurve curve)
    {
        this.curve = curve;
    }

    public static Transition Get(IAnimationCurve normalizedCurve, float duration = 0.5f)
    {
        Transition t = new(normalizedCurve);
        t.easing = EasingType.Custom;
        t.duration = duration;
        return t;
    }

    public static Transition Get(EasingType type = EasingType.EaseInOut, float duration = 0.5f)
    {
        Transition t = new(CurveFactory.BuildNormalized(type));
        t.easing = type;
        t.duration = duration;
        return t;
    }

    internal IAnimationCurve GetCurve(float from, float to)
    {
        lock (@lock)
        {
            if (curve is null)
                curve = CurveFactory.BuildNormalized(easing);

            float range = to - from;
            HsmKeyFrame[] keys = curve.Keys;

            int frameCount = keys.Length;

            if (curve.PostWrapMode == HsmWrapMode.Once)
                frameCount += 1;

            HsmKeyFrame[] scaled = new HsmKeyFrame[frameCount];

            for (int i = 0; i < keys.Length; i++)
                scaled[i] = new HsmKeyFrame(keys[i].Time * duration, from + keys[i].Value * range, keys[i].InTangent * range / duration, keys[i].OutTangent * range / duration);
            
            if (curve.PostWrapMode == HsmWrapMode.Once)
                scaled[frameCount - 1] = new HsmKeyFrame(99999f, to);

            IAnimationCurve result = CurveFactory.Build(scaled);

            result.PreWrapMode = curve.PreWrapMode;
            result.PostWrapMode = curve.PostWrapMode;

            return result;
        }
    }

    internal float Evaluate(float time, float start, float end)
    {
        lock (@lock)
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
}