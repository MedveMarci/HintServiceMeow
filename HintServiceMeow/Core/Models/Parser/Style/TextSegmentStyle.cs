using System;
using HintServiceMeow.Core.Models.Parser.ValueObject;

namespace HintServiceMeow.Core.Models.Parser.Style;

internal class TextSegmentStyle(float fontSize, Color color, float? alpha, bool bold, bool italic, bool underline, bool strikethrough, int superscript, int subscript, float? vOffset, float? rotate, float? charSpace, float? monospace, Color? mark, string? font, int? fontWeight)
{
    public static TextSegmentStyle Default => new(40, new Color(255, 255, 255), null, false, false, false, false, 0, 0, null, null, null, null, null, null, null);

    public float GetWidth(float totalWidthWithFontSize, int count)
    {
        if (Monospace.HasValue)
            return (Monospace.Value + (CharSpace ?? 0)) * count;

        totalWidthWithFontSize *= (float)Math.Pow(0.5, Superscript + Subscript);
        totalWidthWithFontSize += CharSpace ?? 0;

        return totalWidthWithFontSize;
    }

    public float GetHeight() 
    {
        float height = FontSize;
        height *= (float)Math.Pow(0.5, Superscript + Subscript);
        return height;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TextSegmentStyle other)
            return false;

        return FontSize == other.FontSize && Color.Equals(other.Color) && Alpha == other.Alpha && Bold == other.Bold && Italic == other.Italic && Underline == other.Underline && Strikethrough == other.Strikethrough && Superscript == other.Superscript && Subscript == other.Subscript && VOffset == other.VOffset && Rotate == other.Rotate && CharSpace == other.CharSpace && Monospace == other.Monospace && Nullable.Equals(Mark, other.Mark) && Font == other.Font && FontWeight == other.FontWeight;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + FontSize.GetHashCode();
        hash = hash * 31 + Color.GetHashCode();
        hash = hash * 31 + Alpha.GetHashCode();
        hash = hash * 31 + Bold.GetHashCode();
        hash = hash * 31 + Italic.GetHashCode();
        hash = hash * 31 + Underline.GetHashCode();
        hash = hash * 31 + Strikethrough.GetHashCode();
        hash = hash * 31 + Superscript.GetHashCode();
        return hash;
    }

    #region Data

    public float FontSize { get; set; } = fontSize;

    public Color Color { get; set; } = color;

    public float? Alpha { get; set; } = alpha;

    public bool Bold { get; set; } = bold;

    public bool Italic { get; set; } = italic;

    public bool Underline { get; set; } = underline;

    public bool Strikethrough { get; set; } = strikethrough;

    public int Superscript { get; set; } = superscript;

    public int Subscript { get; set; } = subscript;

    public float? VOffset { get; set; } = vOffset;

    public float? Rotate { get; set; } = rotate;

    public float? CharSpace { get; set; } = charSpace;

    public float? Monospace { get; set; } = monospace;

    public Color? Mark { get; set; } = mark;

    public string? Font { get; set; } = font;

    public int? FontWeight { get; set; } = fontWeight;

    #endregion
}