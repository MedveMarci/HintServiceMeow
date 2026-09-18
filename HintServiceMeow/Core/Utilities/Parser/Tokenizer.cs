using System;
using System.Collections.Generic;
using System.Text;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Pools;

namespace HintServiceMeow.Core.Utilities.Parser;

internal class Tokenizer
{
    private readonly object tokenizerLock = new();
    private List<Models.Parser.Token> tokenList = [];
    private int index;
    private string? rawText;
    private StringBuilder? sb;

    public List<Models.Parser.Token> Tokenize(string raw, Tuple<string, IParameter>[] registeredParameters)
    {
        lock (tokenizerLock)
        {
            try
            {
                sb = StringBuilderPool.Instance.Rent();
                tokenList.Clear();
                index = 0;
                rawText = raw;

                while (index < rawText.Length)
                {
                    char c = raw[index];
                    switch (c)
                    {
                        case '\\':
                            if (TryHandleEscapeCharacter())
                                continue;
                            break;

                        case '<':
                            if (TryHandleRichTag())
                                continue;
                            break;

                        case '{':
                            if (TryHandleParameter(registeredParameters))
                                continue;
                            break;

                        case '\n':
                            PackTextInSbAndAdd(Models.Parser.Token.GetLineBreak());
                            index++;
                            continue;
                    }

                    sb.Append(c);
                    index++;
                }

                PackTextInSb();

                List<Models.Parser.Token> result = tokenList;
                tokenList = new List<Models.Parser.Token>(result.Count);
                return result;
            }
            finally
            {
                StringBuilderPool.Instance.Return(sb);
                sb = null;
            }
        }
    }

    private bool TryHandleEscapeCharacter()
    {
        if (rawText![index] != '\\') return false;

        if (rawText.Length <= index + 1) return false;

        switch (rawText[index + 1])
        {
            case 'n':
                PackTextInSbAndAdd(Models.Parser.Token.GetLineBreak());
                break;
            case '\\':
                sb!.Append('\\');
                break;
            default:
                return false;
        }

        index += 2;

        return true;
    }

    private bool TryHandleRichTag()
    {
        if (rawText![index] != '<') return false;

        if (rawText.Length <= index + 1) return false;

        int tagStart = index + 1;
        int tagEnd = rawText.IndexOf('>', tagStart) - 1;

        if (tagEnd < 0) return false;

        bool isCloseTag = rawText[tagStart] == '/';
        string? tagName, tagParameter = null;

        if (isCloseTag)
        {
            tagName = TagChecker.TryMatchValidTag(rawText, tagStart + 1, tagEnd - tagStart);
        }
        else
        {
            int equalSignIndex = rawText.IndexOf('=', tagStart);

            if (equalSignIndex == -1 || equalSignIndex > tagEnd)
            {
                tagName = TagChecker.TryMatchValidTag(rawText, tagStart, tagEnd - tagStart + 1);
            }
            else
            {
                tagName = TagChecker.TryMatchValidTag(rawText, tagStart, equalSignIndex - tagStart);
                tagParameter = rawText.Substring(equalSignIndex + 1, tagEnd - equalSignIndex);
            }
        }

        if (tagName == null) return false;

        if (tagName.Equals("br", StringComparison.OrdinalIgnoreCase))
        {
            PackTextInSbAndAdd(Models.Parser.Token.GetLineBreak());
        }
        else
        {
            if (isCloseTag)
                PackTextInSbAndAdd(Models.Parser.Token.GetTag(RichTextTokenType.CloseTag, tagName, tagParameter));
            else if (TagChecker.IsSelfClosingTag(tagName))
                PackTextInSbAndAdd(Models.Parser.Token.GetTag(RichTextTokenType.SelfCloseTag, tagName, tagParameter));
            else
                PackTextInSbAndAdd(Models.Parser.Token.GetTag(RichTextTokenType.OpenTag, tagName, tagParameter));
        }

        index = tagEnd + 2;
        return true;
    }

    private bool TryHandleParameter(Tuple<string, IParameter>[] registeredParameters)
    {
        if (rawText![index] != '{') return false;

        int start = index + 1;

        int endParamIndex = rawText.IndexOf('}', start);
        if (endParamIndex == -1) return false;

        string paramContent = rawText.Substring(start, endParamIndex - start);

        foreach (Tuple<string, IParameter> t in registeredParameters)
            if (string.Equals(paramContent, t.Item1, StringComparison.OrdinalIgnoreCase))
            {
                PackTextInSbAndAdd(Models.Parser.Token.GetParameter(t.Item2));
                index = endParamIndex + 1;
                return true;
            }

        return false;
    }

    private void PackTextInSbAndAdd(Models.Parser.Token token)
    {
        PackTextInSb();
        tokenList.Add(token);
    }

    private void PackTextInSb()
    {
        string text = sb!.ToString();

        if (text == string.Empty) return;

        tokenList.Add(Models.Parser.Token.GetText(text));
        sb.Clear();
    }
}