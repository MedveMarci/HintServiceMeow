using Hints;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.UnityAdaptors;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class AnimationParameter(double offset, IAnimationCurve curve, string format, bool integral) : IParameter
{
    public double Offset { get; set; } = offset;

    public IAnimationCurve Curve { get; set; } = curve;

    public string Format { get; set; } = format;

    public bool Integral { get; set; } = integral;

    public AnimationParameter(IAnimationCurve curve, string format = "F1") : this(NetworkTimeCache.Time, curve, format, false)
    { }

    public HintParameter GetScpslHintParameter()
    {
        Keyframe[] keyframes = new Keyframe[Curve.Keys.Length];
        for (int i = 0; i < Curve.Keys.Length; i++)
        {
            HsmKeyFrame kf = Curve.Keys[i];
            keyframes[i] = new Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent);
        }

        AnimationCurve curve = new(keyframes);
        curve.preWrapMode = Curve.PreWrapMode.ToUnityWrapMode();
        curve.postWrapMode = Curve.PostWrapMode.ToUnityWrapMode();

        return new AnimationCurveHintParameter(Offset, curve, Format, Integral);
    }
}