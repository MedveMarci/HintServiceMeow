using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class SSKeybindParameter(int value, string format) : IParameter
{
    public const string SettingNotFound = "SERVER SETTING NOT FOUND";

    public const string KeyNotAssigned = "KEY NOT ASSIGNED";

    public const string DefaultKeybindFormat = "[{0}]";

    public int Value { get; set; } = value;

    public string Format { get; set; } = format;

    public SSKeybindParameter(int value) : this(value, DefaultKeybindFormat)
    { }

    public HintParameter GetScpslHintParameter()
    {
        return new SSKeybindHintParameter(Value, Format);
    }
}