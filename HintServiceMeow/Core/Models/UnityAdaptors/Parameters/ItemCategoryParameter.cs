using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class ItemCategoryParameter(ItemCategory category) : IParameter
{
    public ItemCategory Category { get; set; } = category;

    public HintParameter GetScpslHintParameter()
    {
        return new ItemCategoryHintParameter(Category);
    }
}