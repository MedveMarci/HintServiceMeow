using HintServiceMeow.Core.Models.HintContent;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models.Transition;
using YamlDotNet.Serialization;

namespace HintServiceMeow.UI.Models.Template;

public class DynamicHintTemplate : DynamicHintConfig
{
    /// <summary>Gets or sets the logical identifier. Maps to <see cref="AbstractHint.Id" />.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets whether the hint is hidden. Maps to <see cref="AbstractHint.Hide" />.</summary>
    public bool? Hide { get; set; }
    
    /// <summary>Gets or sets the auto-text callback. Maps to <see cref="AbstractHint.AutoText" />.</summary>
    [YamlIgnore]
    public AutoContent.TextUpdateHandler? AutoText { get; set; }

    /// <summary>Gets or sets the hint content. Maps to <see cref="AbstractHint.Content" />.</summary>
    [YamlIgnore]
    public AbstractHintContent? Content { get; set; }

    /// <summary>Gets or sets the font-size transition. Maps to <see cref="AbstractHint.FontSizeTransition" />.</summary>
    [YamlIgnore]
    public Transition? FontSizeTransition { get; set; }

    /// <summary>Gets or sets the X-coordinate transition. Maps to <see cref="DynamicHint.XCoordinateTransition" />.</summary>
    [YamlIgnore]
    public Transition? XCoordinateTransition { get; set; }

    /// <summary>Gets or sets the Y-coordinate transition. Maps to <see cref="DynamicHint.YCoordinateTransition" />.</summary>
    [YamlIgnore]
    public Transition? YCoordinateTransition { get; set; }

    /// <inheritdoc />
    public override void Apply(DynamicHint hint)
    {
        base.Apply(hint);

        if (Id != null)
            hint.Id = Id;

        if (Hide.HasValue)
            hint.Hide = Hide.Value;

        if (AutoText != null)
            hint.AutoText = AutoText;

        if (FontSizeTransition != null)
            hint.FontSizeTransition = FontSizeTransition;
    }
}