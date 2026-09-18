using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class ByteValueParameter(byte value) : IParameter
{
    public byte Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new ByteHintParameter(Value);
    }
}