using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class UShortValueParameter(ushort value) : IParameter
{
    public ushort Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new UShortHintParameter(Value);
    }
}