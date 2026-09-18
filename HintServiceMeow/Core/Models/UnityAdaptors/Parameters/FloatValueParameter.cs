using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class FloatValueParameter(float value, string format = "F2") : IParameter
{
    public float Value { get; set; } = value;

    public string Format { get; set; } = format;

    public HintParameter GetScpslHintParameter()
    {
        return new FloatHintParameter(Value, Format);
    }
}