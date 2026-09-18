using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class TextParameter(string value) : IParameter
{
    public string Value { get; set; } = value;

    public HintParameter GetScpslHintParameter()
    {
        return new StringHintParameter(Value);
    }
}