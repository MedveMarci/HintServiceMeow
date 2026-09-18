using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class ItemParameter(ItemType item) : IParameter
{
    public ItemType Item { get; set; } = item;

    public HintParameter GetScpslHintParameter()
    {
        return new ItemHintParameter(Item);
    }
}