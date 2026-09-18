using System;

namespace HintServiceMeow.Core.Enum;

[Flags]
internal enum FontStyle
{
    Normal = 0x0000,
    Bold = 0x0001,
    Italic = 0x0010
}