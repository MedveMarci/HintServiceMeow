using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class IntValueParameter(int value) : IParameter
{
    public int Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new IntHintParameter(Value);
    }
}