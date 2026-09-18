using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class DoubleValueParameter(double value, string format = "F2") : IParameter
{
    public double Value { get; set; } = value;

    public string Format { get; set; } = format;

    public HintParameter GetScpslHintParameter()
    {
        return new DoubleHintParameter(Value, Format);
    }
}