using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Parser.Style;

namespace HintServiceMeow.Core.Models.Arguments;

internal class RichTextParserSetting(TextMeshStyle defaultStyle, Tuple<string, IParameter>[] parameters, string[] illegalTags, HashSet<string> ignoreTags, bool closeUnclosedTags) : IEquatable<RichTextParserSetting>
{
    public TextMeshStyle DefaultStyle { get; } = defaultStyle;

    public Tuple<string, IParameter>[] Parameters { get; set; } = parameters;

    public string[] IllegalTags { get; } = illegalTags;

    public HashSet<string> IgnoreTags { get; } = ignoreTags;

    public int ParameterIndex { get; set; }

    public bool CloseUnclosedTags { get; set; } = closeUnclosedTags;

    public RichTextParserSetting Clone()
    {
        TextSegmentStyle charStyle = DefaultStyle.CharStyle;
        LineStyle lineStyle = DefaultStyle.LineStyle;

        TextSegmentStyle charStyleCopy = new(charStyle.FontSize, charStyle.Color, charStyle.Alpha, charStyle.Bold, charStyle.Italic, charStyle.Underline, charStyle.Strikethrough, charStyle.Superscript, charStyle.Subscript, charStyle.VOffset, charStyle.Rotate, charStyle.CharSpace, charStyle.Monospace, charStyle.Mark, charStyle.Font, charStyle.FontWeight);

        LineStyle lineStyleCopy = new(lineStyle.Alignment, lineStyle.LineHeight, lineStyle.Indent, lineStyle.MarginLeft, lineStyle.MarginRight, lineStyle.MaxWidth);

        TextMeshStyle styleCopy = new(charStyleCopy, lineStyleCopy, DefaultStyle.Width, DefaultStyle.Height);

        return new RichTextParserSetting(styleCopy, Parameters, IllegalTags, IgnoreTags, CloseUnclosedTags)
        {
            ParameterIndex = ParameterIndex
        };
    }

    public bool Equals(RichTextParserSetting? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Equals(DefaultStyle, other.DefaultStyle) && ParameterIndex == other.ParameterIndex && CloseUnclosedTags == other.CloseUnclosedTags && SequenceEqual(Parameters, other.Parameters) && SequenceEqual(IllegalTags, other.IllegalTags) && IgnoreTags.SetEquals(other.IgnoreTags);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as RichTextParserSetting);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 31 + (DefaultStyle != null ? DefaultStyle.GetHashCode() : 0);
            hash = hash * 31 + ParameterIndex;
            hash = hash * 31 + CloseUnclosedTags.GetHashCode();

            if (Parameters != null)
                foreach (var p in Parameters)
                    hash = hash * 31 + (p != null ? p.GetHashCode() : 0);

            if (IllegalTags != null)
                foreach (var tag in IllegalTags)
                    hash = hash * 31 + (tag != null ? tag.GetHashCode() : 0);

            if (IgnoreTags != null)
            {
                int ignoreTagsHash = 0;
                foreach (string? tag in IgnoreTags) ignoreTagsHash ^= tag != null ? tag.GetHashCode() : 0;

                hash = hash * 31 + ignoreTagsHash;
            }

            return hash;
        }
    }

    private static bool SequenceEqual<T>(T[] a, T[] b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
            if (!Equals(a[i], b[i]))
                return false;

        return true;
    }
}