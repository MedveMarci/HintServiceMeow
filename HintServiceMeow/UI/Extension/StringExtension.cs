using HintServiceMeow.UI.Models;

namespace HintServiceMeow.UI.Extension;

public static class StringExtension
{
    public static string UseTag(this string text, RichTag tag)
    {
        return tag.Apply(text);
    }
}