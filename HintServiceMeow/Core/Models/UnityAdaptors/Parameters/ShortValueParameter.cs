using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class ShortValueParameter(short value) : IParameter
{
    public short Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new ShortHintParameter(Value);
    }
}