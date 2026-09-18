using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class AmmoParameter(byte id) : IParameter
{
    public byte Id { get; set; } = id;

    public HintParameter GetScpslHintParameter()
    {
        return new AmmoHintParameter(Id);
    }
}