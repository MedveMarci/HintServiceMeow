using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class LongValueParameter(long value) : IParameter
{
    public long Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new LongHintParameter(Value);
    }
}