namespace HintServiceMeow.Core.Models.Parser.ValueObject;

internal struct Color(byte red, byte green, byte blue, byte alpha = 255)
{
    public byte Red = red;
    public byte Green = green;
    public byte Blue = blue;
    public byte Alpha = alpha;
}