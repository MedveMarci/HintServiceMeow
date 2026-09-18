using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.UnityAdaptors;

namespace HintServiceMeow.Core.Interface;

internal interface IAnimationCurveFactory
{
    IAnimationCurve BuildNormalized(EasingType type);

    IAnimationCurve Build(HsmKeyFrame[] keyframes);
}