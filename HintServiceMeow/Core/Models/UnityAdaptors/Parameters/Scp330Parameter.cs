using Hints;
using HintServiceMeow.Core.Interface;
using InventorySystem.Items.Usables.Scp330;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class Scp330Parameter(Scp330Translations.Entry index) : IParameter
{
    public Scp330Translations.Entry Index { get; set; } = index;

    public HintParameter GetScpslHintParameter()
    {
        return new Scp330HintParameter(Index);
    }
}