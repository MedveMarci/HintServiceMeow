using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Models.UnityAdaptors;

namespace HintServiceMeow.Core.Interface;

public interface IAnimationCurve
{
    HsmKeyFrame[] Keys { get; }

    HsmWrapMode PreWrapMode { get; set; }

    HsmWrapMode PostWrapMode { get; set; }

    float Evaluate(float time);
}