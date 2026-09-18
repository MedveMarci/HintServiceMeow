using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Parser.Style;

internal class LineStyle(HintAlignment alignment, float? lineHeight, float indent, float marginLeft, float marginRight, float maxWidth)
{
    public static LineStyle Default => new(HintAlignment.Center, null, 0, 0, 0, 1440);

    public HintAlignment Alignment { get; set; } = alignment;

    public float? LineHeight { get; set; } = lineHeight;

    public float Indent { get; set; } = indent;

    public float MarginLeft { get; set; } = marginLeft;

    public float MarginRight { get; set; } = marginRight;

    public float MaxWidth { get; set; } = maxWidth;

    public override bool Equals(object? obj)
    {
        if (obj is not LineStyle other)
            return false;

        return Alignment == other.Alignment && LineHeight == other.LineHeight && Indent == other.Indent && MarginLeft == other.MarginLeft && MarginRight == other.MarginRight && MaxWidth == other.MaxWidth;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + Alignment.GetHashCode();
        hash = hash * 31 + LineHeight.GetHashCode();
        hash = hash * 31 + Indent.GetHashCode();
        hash = hash * 31 + MarginLeft.GetHashCode();
        hash = hash * 31 + MarginRight.GetHashCode();
        hash = hash * 31 + MaxWidth.GetHashCode();
        return hash;
    }
}