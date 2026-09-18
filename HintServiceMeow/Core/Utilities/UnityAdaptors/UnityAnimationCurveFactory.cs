using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors;
using UnityEngine;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal class UnityAnimationCurveFactory : IAnimationCurveFactory
{
    public IAnimationCurve Build(HsmKeyFrame[] keyframes)
    {
        Keyframe[] unityFrames = new Keyframe[keyframes.Length];

        for (int i = 0; i < keyframes.Length; i++)
        {
            HsmKeyFrame kf = keyframes[i];
            unityFrames[i] = new Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent);
        }

        return new UnityAnimationCurve(new AnimationCurve(unityFrames));
    }

    public IAnimationCurve BuildNormalized(EasingType type)
    {
        switch (type)
        {
            case EasingType.Linear:
                return new UnityAnimationCurve(AnimationCurve.Linear(0f, 0f, 1f, 1f));

            case EasingType.EaseIn:
                return new UnityAnimationCurve(new AnimationCurve(new Keyframe(0f, 0f) { outTangent = 0f }, new Keyframe(1f, 1f) { inTangent = 2f }));

            case EasingType.EaseOut:
                return new UnityAnimationCurve(new AnimationCurve(new Keyframe(0f, 0f) { outTangent = 2f }, new Keyframe(1f, 1f) { inTangent = 0f }));

            case EasingType.EaseInOut:
            default:
                return new UnityAnimationCurve(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
        }
    }
}