using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class SByteValueParameter(sbyte value) : IParameter
{
    public sbyte Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new SByteHintParameter(Value);
    }
}