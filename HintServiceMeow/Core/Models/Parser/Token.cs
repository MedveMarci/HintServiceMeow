using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.Parser;

internal struct Token
{
    public Token(RichTextTokenType type, string? tagName = null, string? tagValue = null, string? text = null, IParameter? parameter = null)
    {
        Type = type;
        TagName = tagName;
        TagValue = tagValue;
        Text = text;
        Parameter = parameter;
    }

    public RichTextTokenType Type { get; set; }

    public string? TagName { get; set; } = null;

    public string? TagValue { get; set; } = null;

    public string? Text { get; set; } = null;

    public IParameter? Parameter { get; set; } = null;

    public static Token GetText(string text)
    {
        return new Token(RichTextTokenType.Text, text: text);
    }

    public static Token GetTag(RichTextTokenType type, string tagName, string? tagValue)
    {
        return new Token(type, tagName, tagValue);
    }

    public static Token GetLineBreak()
    {
        return new Token(RichTextTokenType.LineBreak);
    }

    public static Token GetParameter(IParameter parameter)
    {
        return new Token(RichTextTokenType.Parameter, parameter: parameter);
    }

    public override string ToString()
    {
        return Type switch
        {
            RichTextTokenType.Text => $"Text(\"{Text}\")",
            RichTextTokenType.OpenTag => $"Open(<{TagName}={TagValue}>)",
            RichTextTokenType.CloseTag => $"Close(</{TagName}>)",
            RichTextTokenType.SelfCloseTag => $"SelfClose(<{TagName}>)",
            RichTextTokenType.LineBreak => "Newline",
            _ => "Unknown"
        };
    }
}