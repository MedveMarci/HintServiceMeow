using System;

namespace HintServiceMeow.Core.Models.Parser.Style;

internal class TextMeshStyle(TextSegmentStyle charStyle, LineStyle lineStyle, float width, float height) : IEquatable<TextMeshStyle>
{
    public static TextMeshStyle Default => new(TextSegmentStyle.Default, LineStyle.Default, 1440, 1080);

    public TextSegmentStyle CharStyle { get; set; } = charStyle;

    public LineStyle LineStyle { get; set; } = lineStyle;

    public float Width { get; set; } = width;

    public float Height { get; set; } = height;

    public bool Equals(TextMeshStyle? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Equals(CharStyle, other.CharStyle) && Equals(LineStyle, other.LineStyle) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TextMeshStyle);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (CharStyle != null ? CharStyle.GetHashCode() : 0);
            hash = hash * 31 + (LineStyle != null ? LineStyle.GetHashCode() : 0);
            hash = hash * 31 + Width.GetHashCode();
            hash = hash * 31 + Height.GetHashCode();
            return hash;
        }
    }
}