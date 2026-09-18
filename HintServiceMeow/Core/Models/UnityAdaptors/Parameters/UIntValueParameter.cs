using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class UIntValueParameter(uint value) : IParameter
{
    public uint Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new UIntHintParameter(Value);
    }
}