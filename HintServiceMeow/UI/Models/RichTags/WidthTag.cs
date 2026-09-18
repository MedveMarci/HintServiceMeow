namespace HintServiceMeow.UI.Models.RichTags;

public sealed class WidthTag : RichTag
{
    private readonly string value;

    /// <inheritdoc />
    public override string OpenTag => $"<width={value}>";

    /// <inheritdoc />
    public override string CloseTag => "</width>";

    internal override int Priority => 400;

    private WidthTag(string value)
    {
        this.value = value;
    }
    
    /// <summary>
    ///     Creates a <see cref="WidthTag" /> with a custom width constraint.
    /// </summary>
    /// <param name="value">
    ///     The width in pixels (e.g., <c>300</c>) or as a percentage of the text area width (e.g., <c>60%</c>).
    ///     Syntax result: <c>&lt;width=value&gt;</c>.
    /// </param>
    /// <returns>A new <see cref="WidthTag" /> with the specified width constraint.</returns>
    public static WidthTag Get(string value)
    {
        return new WidthTag(value);
    }

    public static WidthTag Get(int pixels)
    {
        return new WidthTag(pixels.ToString());
    }
}