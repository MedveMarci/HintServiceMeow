using System.Linq;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Parser;

internal struct LineInfo(TextSegment[] characterInfos, LineStyle style, string cleanText, float emptyLineHeight = 0f)
{
    public TextSegment[] CharacterInfos { get; } = characterInfos;

    public LineStyle Style { get; } = style;

    public string CleanText { get; } = cleanText;

    public float Width
    {
        get
        {
            float totalWidth = CharacterInfos.Sum(t => t.Width);

            totalWidth += Style.Indent + Style.MarginLeft + Style.MarginRight;

            return totalWidth;
        }
    }

    public float Height
    {
        get
        {
            if (Style.LineHeight != null)
                return Style.LineHeight.Value;
            
            if (CharacterInfos.Length == 0)
                return emptyLineHeight;

            return CharacterInfos.Select(t => t.Height).Prepend(0f).Max();
        }
    }
}