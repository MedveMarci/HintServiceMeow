namespace HintServiceMeow.UI.Models.RichTags;

/// <summary>
///     Represents the subscript rich text tag <c>&lt;sub&gt;</c>.
///     Renders the enclosed text as subscript (smaller, lowered below the baseline).
///     Example: <c>&lt;sub&gt;text&lt;/sub&gt;</c>.
/// </summary>
public sealed class SubscriptTag : RichTag
{
    /// <summary>
    ///     Subscript text formatting. Syntax: <c>&lt;sub&gt;text&lt;/sub&gt;</c>.
    /// </summary>
    public static readonly SubscriptTag Subscript = new();

    /// <inheritdoc />
    public override string OpenTag => "<sub>";

    /// <inheritdoc />
    public override string CloseTag => "</sub>";

    internal override int Priority => 100;

    private SubscriptTag()
    { }
}