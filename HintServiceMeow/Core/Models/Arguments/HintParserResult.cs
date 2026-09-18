using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.Arguments;

public class HintParserResult(string content, IParameter[] parameters)
{
    /// <summary>
    ///     Gets the formatted hint content string to be rendered.
    /// </summary>
    public string Content { get; } = content;

    public IParameter[] Parameters { get; } = parameters;
}