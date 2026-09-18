using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Parser;

namespace HintServiceMeow.Core.Models.Arguments;

internal readonly struct RichTextParserResult(LineInfo[] lineInfos, IParameter[] parameters, int parameterIndex)
{
    public LineInfo[] LineInfos { get; } = lineInfos;

    public IParameter[] Parameters { get; } = parameters;

    public int ParameterIndex { get; } = parameterIndex;
}