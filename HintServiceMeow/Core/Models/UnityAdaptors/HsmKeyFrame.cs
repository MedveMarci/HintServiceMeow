namespace HintServiceMeow.Core.Models.UnityAdaptors;

public class HsmKeyFrame(float time, float value, float inTangent = 0, float outTangent = 0)
{
    public float Time { get; } = time;

    public float Value { get; } = value;

    public float InTangent { get; } = inTangent;

    public float OutTangent { get; } = outTangent;
}