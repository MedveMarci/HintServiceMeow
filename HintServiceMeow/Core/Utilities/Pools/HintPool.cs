using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Core.Utilities.Pools;

internal class HintPool : PoolBase<Hint>
{
    public static HintPool Instance { get; } = new();

    protected override void Reset(Hint hint)
    {
        hint.ResetFields();
    }

    protected override Hint Create()
    {
        return new Hint();
    }
}