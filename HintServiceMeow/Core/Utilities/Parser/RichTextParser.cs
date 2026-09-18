using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Parser;
using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Models.Parser.ValueObject;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities.Parser;

internal class RichTextParser
{
    private const float DefaultReferenceFontSize = 40f;

    private static readonly Cache<(string, RichTextParserSetting), RichTextParserResult> parserCache = new(200);

    private readonly object parserLock = new();

    private TextSegmentStyle? charStyleCache;
    private LineStyle? lineStyleAutoWrappedCache;
    private LineStyle? lineStyleNonAutoWrappedCache;

    private readonly List<IParameter> parameters = [];

    private readonly List<LineInfo> lineInfos = new(16);
    private readonly List<TextSegment> currentLineChars = new(256);

    private readonly Style currentStyle = new();

    private TextMeshStyle defaultStyle = TextMeshStyle.Default;
    private string[] illegalTags = [];
    private HashSet<string> ignoreTags = [];
    private StringBuilder? sb;

    public RichTextParserResult ParseText(string? rawText, RichTextParserSetting setting)
    {
        if (rawText == null)
            return new RichTextParserResult([], [], setting.ParameterIndex);

        if (parserCache.TryGet((rawText, setting), out RichTextParserResult cachedResult)) return cachedResult;

        lock (parserLock)
        {
            Reset();
            defaultStyle = setting.DefaultStyle;
            illegalTags = setting.IllegalTags;
            ignoreTags = setting.IgnoreTags;

            sb = StringBuilderPool.Instance.Rent();

            Tokenizer tokenizer = TokenizerPool.Instance.Rent();
            List<Models.Parser.Token> tokens = tokenizer.Tokenize(rawText, setting.Parameters);
            TokenizerPool.Instance.Return(tokenizer);

            int parameterIndex = setting.ParameterIndex;

            foreach (Models.Parser.Token t in tokens)
                switch (t.Type)
                {
                    case RichTextTokenType.Text:
                        HandleText(t.Text!);
                        break;
                    case RichTextTokenType.OpenTag:
                        HandleOpenTag(t);
                        break;
                    case RichTextTokenType.CloseTag:
                        HandleCloseTag(t);
                        break;
                    case RichTextTokenType.SelfCloseTag:
                        HandleSelfCloseTag(t);
                        break;
                    case RichTextTokenType.Parameter:
                        HandleText($"{{{parameterIndex}}}");
                        parameterIndex++;
                        parameters.Add(t.Parameter!);
                        break;
                    case RichTextTokenType.LineBreak:
                        FinishLine(defaultStyle, false);
                        break;
                }

            if (setting.CloseUnclosedTags)
                CloseUnclosedTag();

            FinishLine(defaultStyle, false);

            LineInfo[] lineInfosArray = [.. lineInfos];
            IParameter[] parametersArray = [.. parameters];

            Reset();

            StringBuilderPool.Instance.Return(sb);
            sb = null;

            RichTextParserResult result = new(lineInfosArray, parametersArray, parameterIndex);
            
            parserCache.Add((rawText, setting.Clone()), result);

            return result;
        }
    }

    private void CloseUnclosedTag()
    {
        if (currentStyle.NoParse)
        {
            currentStyle.NoParse = false;
            if (!illegalTags.Contains("noparse"))
                sb!.Append("</noparse>");
        }

        for (int i = 0; i < currentStyle.Alignment.Count; i++)
            if (!illegalTags.Contains("align"))
                sb!.Append("</align>");

        currentStyle.Alignment.Clear();

        for (int i = 0; i < currentStyle.Color.Count; i++)
            if (!illegalTags.Contains("color"))
                sb!.Append("</color>");

        currentStyle.Color.Clear();

        for (int i = 0; i < currentStyle.Indent.Count; i++)
            if (!illegalTags.Contains("indent"))
                sb!.Append("</indent>");

        currentStyle.Indent.Clear();

        for (int i = 0; i < currentStyle.Mark.Count; i++)
            if (!illegalTags.Contains("mark"))
                sb!.Append("</mark>");

        currentStyle.Mark.Clear();

        for (int i = 0; i < currentStyle.FontSize.Count; i++)
            if (!illegalTags.Contains("size"))
                sb!.Append("</size>");

        currentStyle.FontSize.Clear();

        for (int i = 0; i < currentStyle.AllCaps; i++)
            if (!illegalTags.Contains("allcaps"))
                sb!.Append("</allcaps>");

        currentStyle.AllCaps = 0;

        for (int i = 0; i < currentStyle.Bold; i++)
            if (!illegalTags.Contains("b"))
                sb!.Append("</b>");

        currentStyle.Bold = 0;

        for (int i = 0; i < currentStyle.Italic; i++)
            if (!illegalTags.Contains("i"))
                sb!.Append("</i>");

        currentStyle.Italic = 0;

        for (int i = 0; i < currentStyle.Lowercase; i++)
            if (!illegalTags.Contains("lowercase"))
                sb!.Append("</lowercase>");

        currentStyle.Lowercase = 0;

        for (int i = 0; i < currentStyle.Strikethrough; i++)
            if (!illegalTags.Contains("s"))
                sb!.Append("</s>");

        currentStyle.Strikethrough = 0;

        for (int i = 0; i < currentStyle.Subscript; i++)
            if (!illegalTags.Contains("sub"))
                sb!.Append("</sub>");

        currentStyle.Subscript = 0;

        for (int i = 0; i < currentStyle.Superscript; i++)
            if (!illegalTags.Contains("sup"))
                sb!.Append("</sup>");

        currentStyle.Superscript = 0;

        for (int i = 0; i < currentStyle.Underline; i++)
            if (!illegalTags.Contains("u"))
                sb!.Append("</u>");

        currentStyle.Underline = 0;

        for (int i = 0; i < currentStyle.Uppercase; i++)
            if (!illegalTags.Contains("uppercase"))
                sb!.Append("</uppercase>");

        currentStyle.Uppercase = 0;

        if (currentStyle.Alpha.HasValue)
        {
            currentStyle.Alpha = null;
            if (!illegalTags.Contains("alpha"))
                sb!.Append("</alpha>");
        }

        if (currentStyle.CharSpace.HasValue)
        {
            currentStyle.CharSpace = null;
            if (!illegalTags.Contains("cspace"))
                sb!.Append("</cspace>");
        }

        if (currentStyle.Font != null)
        {
            currentStyle.Font = null;
            if (!illegalTags.Contains("font"))
                sb!.Append("</font>");
        }

        if (currentStyle.FontWeight.HasValue)
        {
            currentStyle.FontWeight = null;
            if (!illegalTags.Contains("font-weight"))
                sb!.Append("</font-weight>");
        }

        if (currentStyle.LineHeight.HasValue)
        {
            currentStyle.LineHeight = null;
            if (!illegalTags.Contains("line-height"))
                sb!.Append("</line-height>");
        }

        if (currentStyle.LineIndent.HasValue)
        {
            currentStyle.LineIndent = null;
            if (!illegalTags.Contains("line-indent"))
                sb!.Append("</line-indent>");
        }

        if (currentStyle.MarginLeft.HasValue)
        {
            currentStyle.MarginLeft = null;
            if (!illegalTags.Contains("margin-left"))
                sb!.Append("</margin-left>");
        }

        if (currentStyle.MarginRight.HasValue)
        {
            currentStyle.MarginRight = null;
            if (!illegalTags.Contains("margin-right"))
                sb!.Append("</margin-right>");
        }

        if (currentStyle.Monospace.HasValue)
        {
            currentStyle.Monospace = null;
            if (!illegalTags.Contains("mspace"))
                sb!.Append("</mspace>");
        }

        if (currentStyle.Rotate.HasValue)
        {
            currentStyle.Rotate = null;
            if (!illegalTags.Contains("rotate"))
                sb!.Append("</rotate>");
        }

        if (currentStyle.VOffset.HasValue)
        {
            currentStyle.VOffset = null;
            if (!illegalTags.Contains("voffset"))
                sb!.Append("</voffset>");
        }

        if (currentStyle.Width.HasValue)
        {
            currentStyle.Width = null;
            if (!illegalTags.Contains("width"))
                sb!.Append("</width>");
        }

        if (currentStyle.NoBreak)
        {
            currentStyle.NoBreak = false;
            if (!illegalTags.Contains("nobr"))
                sb!.Append("</nobr>");
        }

        if (currentStyle.Smallcap)
        {
            currentStyle.Smallcap = false;
            if (!illegalTags.Contains("smallcaps"))
                sb!.Append("</smallcaps>");
        }

        ClearCharStyleCache();
        ClearLineStyleCache();
    }

    private void HandleText(string text)
    {
        sb!.Append(text);

        if (charStyleCache == null) charStyleCache = currentStyle.GetCharStyle(defaultStyle);

        IFontTool fontTool = FontTool.Instance;
        float fontSize = charStyleCache.FontSize;
        float totalWidthWithFontSize = 0f;
        for (int i = 0; i < text.Length; i++) totalWidthWithFontSize += fontTool.GetCharWidth(text[i], fontSize);

        float actualWidth = charStyleCache.GetWidth(totalWidthWithFontSize, text.Length);

        currentLineChars.Add(new TextSegment(text, actualWidth, charStyleCache));
    }

    private void AddPlaceholder(float width)
    {
        if (charStyleCache == null) charStyleCache = currentStyle.GetCharStyle(defaultStyle);

        TextSegment charInfo = new(" ", width, charStyleCache);
        currentLineChars.Add(charInfo);
    }

    private void ClearLineStyleCache()
    {
        lineStyleAutoWrappedCache = null;
        lineStyleNonAutoWrappedCache = null;
    }

    private void ClearCharStyleCache()
    {
        charStyleCache = null;
    }

    private void Reset()
    {
        parameters.Clear();
        lineInfos.Clear();
        currentLineChars.Clear();

        currentStyle.Clear();

        ClearLineStyleCache();
        ClearCharStyleCache();
    }

    private void FinishLine(TextMeshStyle defaultStyle, bool isAutoWrapped)
    {
        LineStyle lineStyle;
        if (isAutoWrapped)
        {
            if (lineStyleAutoWrappedCache == null) lineStyleAutoWrappedCache = currentStyle.GetLineStyle(defaultStyle, true);

            lineStyle = lineStyleAutoWrappedCache;
        }
        else
        {
            lineStyleNonAutoWrappedCache ??= currentStyle.GetLineStyle(defaultStyle, false);

            lineStyle = lineStyleNonAutoWrappedCache;
        }
        
        float emptyLineHeight = currentLineChars.Count == 0 ? currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize) : 0f;

        lineInfos.Add(new LineInfo([.. currentLineChars], lineStyle, sb!.ToString(), emptyLineHeight));

        currentLineChars.Clear();
        sb.Clear();
    }

    private class Style
    {
        public void Clear()
        {
            Alignment.Clear();
            Color.Clear();
            Indent.Clear();
            Mark.Clear();
            FontSize.Clear();

            AllCaps = 0;
            Bold = 0;
            Italic = 0;
            Lowercase = 0;
            Strikethrough = 0;
            Subscript = 0;
            Superscript = 0;
            Underline = 0;
            Uppercase = 0;

            NoBreak = false;
            NoParse = false;
            Smallcap = false;

            Alpha = null;
            CharSpace = null;
            Font = null;
            LineHeight = null;
            LineIndent = null;
            MarginLeft = null;
            MarginRight = null;
            Monospace = null;
            Rotate = null;
            VOffset = null;
            Width = null;
            FontWeight = null;
        }

        public float GetActualSize(float defaultSize)
        {
            if (FontSize.Count > 0)
                return FontSize.Peek();
            return defaultSize;
        }

        public TextSegmentStyle GetCharStyle(TextMeshStyle defaultStyle)
        {
            float currentFontSize = FontSize.Count > 0 ? FontSize.Peek() : defaultStyle.CharStyle.FontSize;
            Color currentColor = Color.Count > 0 ? Color.Peek() : defaultStyle.CharStyle.Color;
            float? charSpace = CharSpace ?? defaultStyle.CharStyle.CharSpace;
            float? monoSpace = Monospace ?? defaultStyle.CharStyle.Monospace;

            return new TextSegmentStyle(currentFontSize, currentColor, Alpha.HasValue ? Alpha.Value / 255f : 1f, Bold > 0 ? true : defaultStyle.CharStyle.Bold, Italic > 0 ? true : defaultStyle.CharStyle.Italic, Underline > 0 ? true : defaultStyle.CharStyle.Underline, Strikethrough > 0 ? true : defaultStyle.CharStyle.Strikethrough, Superscript > 0 ? Superscript : defaultStyle.CharStyle.Superscript, Subscript > 0 ? Subscript : defaultStyle.CharStyle.Subscript, VOffset ?? defaultStyle.CharStyle.VOffset, Rotate ?? defaultStyle.CharStyle.Rotate, charSpace, monoSpace, Mark.Count > 0 ? Mark.Peek() : defaultStyle.CharStyle.Mark, Font ?? defaultStyle.CharStyle.Font, FontWeight ?? defaultStyle.CharStyle.FontWeight);
        }

        public LineStyle GetLineStyle(TextMeshStyle defaultStyle, bool isAutoWrapped)
        {
            float marginLeftValue = MarginLeft ?? defaultStyle.LineStyle.MarginLeft;
            float marginRightValue = MarginRight ?? defaultStyle.LineStyle.MarginRight;
            float? lineHeightValue = LineHeight ?? defaultStyle.LineStyle.LineHeight;
            float lineIndentValue = LineIndent ?? defaultStyle.LineStyle.Indent;

            HintAlignment alignment = Alignment.Count > 0 ? Alignment.Peek() : defaultStyle.LineStyle.Alignment;
            float actualIndent;

            if (Indent.Count == 0 && LineIndent == null)
            {
                actualIndent = defaultStyle.LineStyle.Indent;
            }
            else
            {
                actualIndent = Indent.Count > 0 ? Indent.Peek() : defaultStyle.LineStyle.Indent;
                if (!isAutoWrapped) actualIndent += LineIndent == null ? 0f : LineIndent.Value;
            }

            return new LineStyle(alignment, lineHeightValue, actualIndent, marginLeftValue, marginRightValue, Width ?? defaultStyle.LineStyle.MaxWidth);
        }

        #region Tag handling data
        
        public Stack<HintAlignment> Alignment { get; } = new(8);

        public int AllCaps { get; set; }

        public byte? Alpha { get; set; }

        public int Bold { get; set; }
        
        public Stack<Color> Color { get; } = new(8);

        public float? CharSpace { get; set; }

        public string? Font { get; set; }

        public int? FontWeight { get; set; }
        
        public int Italic { get; set; }

        public Stack<float> Indent { get; } = new(8);

        public float? LineHeight { get; set; }

        public float? LineIndent { get; set; }

        public int Lowercase { get; set; }

        public float? MarginLeft { get; set; }

        public float? MarginRight { get; set; }

        public Stack<Color> Mark { get; } = new(8);

        public float? Monospace { get; set; }

        public bool NoBreak { get; set; }

        public bool NoParse { get; set; }

        public float? Rotate { get; set; }

        public int Strikethrough { get; set; }

        public Stack<float> FontSize { get; } = new(8);

        public bool Smallcap { get; set; }

        public int Subscript { get; set; }

        public int Superscript { get; set; }

        public int Underline { get; set; }

        public int Uppercase { get; set; }

        public float? VOffset { get; set; }

        public float? Width { get; set; }

        #endregion
    }

    #region Parsing Helpers

    private static bool TryParseAlignment(string? value, out HintAlignment alignment)
    {
        alignment = default;

        if (string.IsNullOrEmpty(value))
            return false;

        switch (value)
        {
            case "left":
                alignment = HintAlignment.Left;
                return true;
            case "center":
                alignment = HintAlignment.Center;
                return true;
            case "right":
                alignment = HintAlignment.Right;
                return true;
            case "justified":
                alignment = HintAlignment.Justified;
                return true;
            case "flush":
                alignment = HintAlignment.Flush;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseAlpha(string? value, out byte alpha)
    {
        alpha = 255;

        if (string.IsNullOrEmpty(value))
            return false;

        string hex = value!.StartsWith("#") ? value.Substring(1) : value;

        return hex.Length == 2 && byte.TryParse(hex, NumberStyles.HexNumber, null, out alpha);
    }

    private static bool TryParseColor(string? value, out Color color)
    {
        color = default;

        if (string.IsNullOrEmpty(value))
            return false;

        switch (value)
        {
            case "red":
                color = new Color(255, 0, 0);
                return true;
            case "green":
                color = new Color(0, 128, 0);
                return true;
            case "blue":
                color = new Color(0, 0, 255);
                return true;
            case "white":
                color = new Color(255, 255, 255);
                return true;
            case "black":
                color = new Color(0, 0, 0);
                return true;
            case "yellow":
                color = new Color(255, 255, 0);
                return true;
            case "cyan":
                color = new Color(0, 255, 255);
                return true;
            case "magenta":
                color = new Color(255, 0, 255);
                return true;
            case "orange":
                color = new Color(255, 165, 0);
                return true;
            case "purple":
                color = new Color(128, 0, 128);
                return true;
            case "grey":
            case "gray":
                color = new Color(128, 128, 128);
                return true;
        }

        string hex = value!.StartsWith("#") ? value.Substring(1) : value;

        try
        {
            byte r, g, b, a = 255;

            if (hex.Length == 6)
            {
                r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                color = new Color(r, g, b, a);
                return true;
            }
            
            if (hex.Length == 8)
            {
                r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                color = new Color(r, g, b, a);
                return true;
            }

            if (hex.Length == 3)
            {
                r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                color = new Color(r, g, b, a);
                return true;
            }
        }
        catch
        {
            // Parse failed
        }

        return false;
    }

    private static bool TryParseFloat(string? value, out float result)
    {
        result = 0f;

        if (string.IsNullOrEmpty(value))
            return false;

        string trimmed = value!.Trim();

        return float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseToMeasuredValue(string? value, out MeasuredValue result)
    {
        result = default;

        if (string.IsNullOrEmpty(value))
            return false;

        MeasureUnit unit;

        if (value!.EndsWith("px"))
        {
            unit = MeasureUnit.Pixel;
            value = value.Substring(0, value.Length - 2);
        }
        else if (value.EndsWith("em"))
        {
            unit = MeasureUnit.FontUnit;
            value = value.Substring(0, value.Length - 2);
        }
        else if (value.EndsWith("%"))
        {
            unit = MeasureUnit.Percentage;
            value = value.Substring(0, value.Length - 1);
        }
        else
        {
            unit = MeasureUnit.Pixel;
        }

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float numericValue))
            return false;

        result = new MeasuredValue
        {
            Value = numericValue,
            Unit = unit
        };
        return true;
    }

    private static bool TryParseToPixels(string? value, float? fontSize, float? targetValue, out float? result)
    {
        result = 0f;

        return TryParseToMeasuredValue(value, out MeasuredValue measured) && measured.TryGetPixels(fontSize, targetValue, out result);
    }

    #endregion

    #region Tag Handlers

    private void HandleOpenTag(Models.Parser.Token token)
    {
        if (currentStyle.NoParse || ignoreTags.Contains(token.TagName!))
        {
            sb!.Append('<').Append(token.TagName);
            if (!string.IsNullOrEmpty(token.TagValue)) sb.Append('=').Append(token.TagValue);

            sb.Append('>');
            return;
        }

        string tagName = token.TagName ?? string.Empty;
        string? value = token.TagValue;

        switch (tagName)
        {
            case "align":
                if (TryParseAlignment(value, out HintAlignment alignment))
                    currentStyle.Alignment.Push(alignment);
                ClearLineStyleCache();
                break;

            case "color":
                if (TryParseColor(value, out Color color))
                    currentStyle.Color.Push(color);
                ClearCharStyleCache();
                break;

            case "indent":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? indentVal))
                    currentStyle.Indent.Push(indentVal!.Value);
                ClearLineStyleCache();
                break;

            case "mark":
                if (TryParseColor(value, out Color markColor))
                    currentStyle.Mark.Push(markColor);
                ClearCharStyleCache();
                break;

            case "size":
                if (TryParseToPixels(value, DefaultReferenceFontSize, DefaultReferenceFontSize, out float? sizeVal))
                    currentStyle.FontSize.Push(sizeVal!.Value);
                ClearCharStyleCache();
                break;

            case "allcaps":
                currentStyle.AllCaps++;
                ClearCharStyleCache();
                break;

            case "b":
                currentStyle.Bold++;
                ClearCharStyleCache();
                break;

            case "i":
                currentStyle.Italic++;
                ClearCharStyleCache();
                break;

            case "lowercase":
                currentStyle.Lowercase++;
                ClearCharStyleCache();
                break;

            case "s":
                currentStyle.Strikethrough++;
                ClearCharStyleCache();
                break;

            case "sub":
                currentStyle.Subscript++;
                ClearCharStyleCache();
                break;

            case "sup":
                currentStyle.Superscript++;
                ClearCharStyleCache();
                break;

            case "u":
                currentStyle.Underline++;
                ClearCharStyleCache();
                break;

            case "uppercase":
                currentStyle.Uppercase++;
                ClearCharStyleCache();
                break;

            case "cspace":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? cspaceVal))
                    currentStyle.CharSpace = cspaceVal;
                ClearCharStyleCache();
                break;

            case "font":
                currentStyle.Font = value;
                ClearCharStyleCache();
                break;

            case "font-weight":
                if (int.TryParse(value, out int fontWeightVal))
                    currentStyle.FontWeight = fontWeightVal;
                ClearCharStyleCache();
                break;

            case "line-height":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.CharStyle.GetHeight(), out float? lineHeightVal))
                    currentStyle.LineHeight = lineHeightVal;
                ClearLineStyleCache();
                break;

            case "line-indent":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? lineIndentVal))
                    currentStyle.LineIndent = lineIndentVal;
                ClearLineStyleCache();
                break;

            case "margin":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginVal))
                {
                    currentStyle.MarginLeft = marginVal;
                    currentStyle.MarginRight = marginVal;
                }

                ClearLineStyleCache();
                break;

            case "margin-left":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginLeftVal))
                    currentStyle.MarginLeft = marginLeftVal;
                ClearLineStyleCache();
                break;

            case "margin-right":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginRightVal))
                    currentStyle.MarginRight = marginRightVal;
                ClearLineStyleCache();
                break;

            case "mspace":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? mspaceVal))
                    currentStyle.Monospace = mspaceVal;
                ClearCharStyleCache();
                break;

            case "rotate":
                if (TryParseFloat(value, out float rotateVal))
                    currentStyle.Rotate = rotateVal;
                ClearCharStyleCache();
                break;

            case "voffset":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? voffsetVal))
                    currentStyle.VOffset = voffsetVal;
                ClearCharStyleCache();
                break;

            case "width":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? widthVal))
                    currentStyle.Width = widthVal;
                ClearLineStyleCache();
                break;

            case "nobr":
                currentStyle.NoBreak = true;
                break;

            case "noparse":
                currentStyle.NoParse = true;
                break;

            case "smallcaps":
                currentStyle.Smallcap = true;
                ClearCharStyleCache();
                break;
        }

        if (!illegalTags.Contains(tagName))
        {
            sb!.Append('<').Append(tagName);

            if (!string.IsNullOrEmpty(value)) sb.Append('=').Append(value);

            sb.Append('>');
        }
    }

    private void HandleCloseTag(Models.Parser.Token token)
    {
        if ((currentStyle.NoParse && token.TagName != "noparse") || ignoreTags.Contains(token.TagName!))
        {
            sb!.Append("</").Append(token.TagName).Append('>');
            return;
        }

        string tagName = token.TagName ?? string.Empty;

        switch (tagName)
        {
            case "align":
                if (currentStyle.Alignment.Count > 0)
                    currentStyle.Alignment.Pop();
                ClearLineStyleCache();
                break;

            case "color":
                if (currentStyle.Color.Count > 0)
                    currentStyle.Color.Pop();
                ClearCharStyleCache();
                break;

            case "indent":
                if (currentStyle.Indent.Count > 0)
                    currentStyle.Indent.Pop();
                ClearLineStyleCache();
                break;

            case "mark":
                if (currentStyle.Mark.Count > 0)
                    currentStyle.Mark.Pop();
                ClearCharStyleCache();
                break;

            case "size":
                if (currentStyle.FontSize.Count > 0)
                    currentStyle.FontSize.Pop();
                ClearCharStyleCache();
                break;

            case "allcaps":
                if (currentStyle.AllCaps > 0)
                    currentStyle.AllCaps--;
                ClearCharStyleCache();
                break;

            case "b":
                if (currentStyle.Bold > 0)
                    currentStyle.Bold--;
                ClearCharStyleCache();
                break;

            case "i":
                if (currentStyle.Italic > 0)
                    currentStyle.Italic--;
                ClearCharStyleCache();
                break;

            case "lowercase":
                if (currentStyle.Lowercase > 0)
                    currentStyle.Lowercase--;
                ClearCharStyleCache();
                break;

            case "s":
                if (currentStyle.Strikethrough > 0)
                    currentStyle.Strikethrough--;
                ClearCharStyleCache();
                break;

            case "sub":
                if (currentStyle.Subscript > 0)
                    currentStyle.Subscript--;
                ClearCharStyleCache();
                break;

            case "sup":
                if (currentStyle.Superscript > 0)
                    currentStyle.Superscript--;
                ClearCharStyleCache();
                break;

            case "u":
                if (currentStyle.Underline > 0)
                    currentStyle.Underline--;
                ClearCharStyleCache();
                break;

            case "uppercase":
                if (currentStyle.Uppercase > 0)
                    currentStyle.Uppercase--;
                ClearCharStyleCache();
                break;

            case "alpha":
                currentStyle.Alpha = null;
                ClearCharStyleCache();
                break;

            case "cspace":
                currentStyle.CharSpace = null;
                ClearCharStyleCache();
                break;

            case "font":
                currentStyle.Font = null;
                ClearCharStyleCache();
                break;

            case "font-weight":
                currentStyle.FontWeight = null;
                ClearCharStyleCache();
                break;

            case "line-height":
                currentStyle.LineHeight = null;
                ClearLineStyleCache();
                break;

            case "line-indent":
                currentStyle.LineIndent = null;
                ClearLineStyleCache();
                break;

            case "margin":
                currentStyle.MarginLeft = null;
                currentStyle.MarginRight = null;
                ClearLineStyleCache();
                break;

            case "margin-left":
                currentStyle.MarginLeft = null;
                ClearLineStyleCache();
                break;

            case "margin-right":
                currentStyle.MarginRight = null;
                ClearLineStyleCache();
                break;

            case "mspace":
                currentStyle.Monospace = null;
                ClearCharStyleCache();
                break;

            case "rotate":
                currentStyle.Rotate = null;
                ClearCharStyleCache();
                break;

            case "voffset":
                currentStyle.VOffset = null;
                ClearCharStyleCache();
                break;

            case "width":
                currentStyle.Width = null;
                ClearLineStyleCache();
                break;

            case "nobr":
                currentStyle.NoBreak = false;
                break;

            case "noparse":
                currentStyle.NoParse = false;
                break;

            case "smallcaps":
                currentStyle.Smallcap = false;
                ClearCharStyleCache();
                break;

        }

        if (!illegalTags.Contains(tagName))
            sb!.Append("</").Append(tagName).Append('>');
    }

    private void HandleSelfCloseTag(Models.Parser.Token token)
    {
        if (currentStyle.NoParse || ignoreTags.Contains(token.TagName!))
        {
            sb!.Append('<').Append(token.TagName);
            if (!string.IsNullOrEmpty(token.TagValue)) sb.Append('=').Append(token.TagValue);

            sb.Append("/>");
            return;
        }

        string tagName = token.TagName ?? string.Empty;
        string value = token.TagValue ?? string.Empty;

        switch (tagName)
        {
            case "br":
                FinishLine(defaultStyle, false);
                break;

            case "alpha":
                if (TryParseAlpha(value, out byte alphaVal))
                    currentStyle.Alpha = alphaVal;
                ClearCharStyleCache();
                break;

            case "pos":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? posVal))
                    if (posVal.HasValue)
                        AddPlaceholder(posVal.Value);

                break;

            case "space":
                if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? spaceVal))
                    if (spaceVal.HasValue)
                        AddPlaceholder(spaceVal.Value);

                break;
        }

        if (!illegalTags.Contains(tagName))
        {
            sb!.Append('<').Append(tagName);
            if (!string.IsNullOrEmpty(value)) sb.Append('=').Append(value);

            sb.Append('>');
        }
    }

    #endregion
}