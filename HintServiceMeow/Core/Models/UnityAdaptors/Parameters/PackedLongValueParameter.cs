using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class PackedLongValueParameter(long value) : IParameter
{
    public long Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new PackedLongHintParameter(Value);
    }
}