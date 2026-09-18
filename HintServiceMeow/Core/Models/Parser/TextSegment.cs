using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Parser;

internal struct TextSegment(string segment, float width, TextSegmentStyle style)
{
    public string Text { get; } = segment;

    public TextSegmentStyle Style { get; set; } = style;

    public float Width { get; } = width;

    public float Height => Style.GetHeight();
}