using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.UI.Models.Template;

public abstract class AbstractHintTemplate
{
    /// <summary>Gets or sets the sync speed. Maps to <see cref="AbstractHint.SyncSpeed" />.</summary>
    public HintSyncSpeed? SyncSpeed { get; set; }

    /// <summary>Gets or sets the font size. Maps to <see cref="AbstractHint.FontSize" />.</summary>
    public int? FontSize { get; set; }

    /// <summary>Gets or sets the line-height offset. Maps to <see cref="AbstractHint.LineHeight" />.</summary>
    public float? LineHeight { get; set; }

    /// <summary>
    ///     Gets or sets the plain-text content. Use a static text as hint's content.
    ///     Maps to <see cref="AbstractHint.Text" />.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    ///     Applies all non-null base properties to <paramref name="hint" />.
    ///     Intended to be called by concrete subclass <c>ApplyTemplate</c> implementations.
    /// </summary>
    protected void ApplyBaseTemplate(AbstractHint hint)
    {
        if (SyncSpeed.HasValue)
            hint.SyncSpeed = SyncSpeed.Value;

        if (FontSize.HasValue)
            hint.FontSize = FontSize.Value;

        if (LineHeight.HasValue)
            hint.LineHeight = LineHeight.Value;

        if (Text != null)
            hint.Text = Text;
    }
}