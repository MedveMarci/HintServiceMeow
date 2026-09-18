using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class ULongValueParameter(ulong value) : IParameter
{
    public ulong Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new ULongHintParameter(Value);
    }
}