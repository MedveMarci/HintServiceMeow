using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Core.Utilities.Pools;

internal class RichTextParserPool : PoolBase<RichTextParser>
{
    public static RichTextParserPool Instance { get; } = new();

    protected override void Reset(RichTextParser parser)
    { }

    protected override RichTextParser Create()
    {
        return new RichTextParser();
    }
}