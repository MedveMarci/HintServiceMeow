using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Core.Utilities.Pools;

internal class TokenizerPool : PoolBase<Tokenizer>
{
    public static TokenizerPool Instance { get; } = new();

    protected override void Reset(Tokenizer item)
    { }

    protected override Tokenizer Create()
    {
        return new Tokenizer();
    }
}