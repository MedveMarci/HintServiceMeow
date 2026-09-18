using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class PackedULongValueParameter(ulong value) : IParameter
{
    public ulong Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new PackedULongHintParameter(Value);
    }
}