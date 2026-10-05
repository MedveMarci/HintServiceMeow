using System;
using System.ComponentModel;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.HintContent;
using HintServiceMeow.Core.Models.Transition;
using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Hints;

/// <summary>
///     Represents the base class for all hints displayed on a player's screen.
///     Provides common properties such as text content, font size, sync speed, and visibility.
/// </summary>
public abstract class AbstractHint : INotifyPropertyChanged
{
    private HintSyncSpeed syncSpeed = HintSyncSpeed.Normal;

    private float fontSize = 20f;
    private Transition.Transition? fontSizeTransition;

    private float lineHeight;

    private AbstractHintContent content = new StringContent(string.Empty);

    private bool hide;

    private ResolutionOption resolutionOption = ResolutionOption.Offset;

    private float edgeMargin;

    #region Events

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="AbstractHint" /> class with default values.
    /// </summary>
    protected AbstractHint()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AbstractHint" /> class by copying properties from an existing hint.
    /// </summary>
    /// <param name="hint">The hint whose properties are copied into this instance.</param>
    protected AbstractHint(AbstractHint hint)
    {
        Id = hint.Id;
        syncSpeed = hint.syncSpeed;
        fontSize = hint.fontSize;
        lineHeight = hint.lineHeight;
        content = hint.content;
        hide = hint.hide;
        edgeMargin = hint.edgeMargin;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the update analyser used to track and estimate hint update timing.
    /// </summary>
    public IUpdateAnalyser UpdateAnalyser { get; set; } = new UpdateAnalyzer();

    /// <summary>
    ///     Gets the unique identifier for this hint instance.
    /// </summary>
    public Guid Guid { get; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets the logical identifier used to group or retrieve this hint.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the synchronization speed that controls how quickly this hint's updates are sent to the display.
    /// </summary>
    public HintSyncSpeed SyncSpeed
    {
        get => syncSpeed;

        set
        {
            if (syncSpeed == value)
                return;

            syncSpeed = value;

            OnHintUpdated(nameof(SyncSpeed));
        }
    }

    /// <summary>
    ///     Gets or sets the font size of the hint text.
    /// </summary>
    public float FontSize
    {
        get => fontSize;

        set
        {
            if (fontSize == value)
                return;

            fontSize = value;

            OnHintUpdated(nameof(FontSize));
        }
    }

    /// <summary>
    ///     Gets or sets the transition effect applied when the font size changes.
    /// </summary>
    public Transition.Transition? FontSizeTransition
    {
        get => fontSizeTransition;

        set
        {
            if (fontSizeTransition == value)
                return;

            fontSizeTransition = value;

            OnHintUpdated(nameof(FontSizeTransition));
        }
    }

    /// <summary>
    ///     Gets or sets the line height offset for the hint text.
    /// </summary>
    public float LineHeight
    {
        get => lineHeight;

        set
        {
            if (lineHeight.Equals(value))
                return;

            lineHeight = value;

            OnHintUpdated(nameof(LineHeight));
        }
    }

    /// <summary>
    ///     Gets or sets the content displayed by this hint.
    /// </summary>
    public AbstractHintContent Content
    {
        get => content;

        set
        {
            if (content == value)
                return;

            content.ContentUpdated -= OnContentUpdate;

            content = value;
            content.ContentUpdated += OnContentUpdate;

            OnHintUpdated(nameof(Content));
        }
    }

    /// <summary>
    ///     Gets or sets the plain text of this hint when the content is a <see cref="StringContent" />.
    ///     Returns <see langword="null" /> if the current content is not a <see cref="StringContent" />.
    /// </summary>
    public string? Text
    {
        get
        {
            AbstractHintContent currentContent = content;

            return currentContent is StringContent ? currentContent.GetText() : null;
        }

        set
        {
            try
            {
                if (content is StringContent textContent)
                    textContent.Text = value;
                else
                    content.ContentUpdated -= OnContentUpdate;
                content = new StringContent(value);
                content.ContentUpdated += OnContentUpdate;
            }
            catch (Exception ex)
            {
                LogManager.Error(ex.ToString());
            }

            OnHintUpdated(nameof(Text));
        }
    }

    /// <summary>
    ///     Gets or sets the auto-text handler used to dynamically generate hint content.
    ///     Setting this property replaces the current content with an <see cref="AutoContent" /> instance.
    ///     Returns <see langword="null" /> if the current content is not an <see cref="AutoContent" />.
    /// </summary>
    public AutoContent.TextUpdateHandler? AutoText
    {
        get
        {
            if (content is AutoContent autoContent) return autoContent.AutoText;

            return null;
        }

        set
        {
            content.ContentUpdated -= OnContentUpdate;
            content = new AutoContent(value);
            content.ContentUpdated += OnContentUpdate;

            OnHintUpdated(nameof(AutoText));
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this hint is hidden from the player's display.
    /// </summary>
    public bool Hide
    {
        get => hide;

        set
        {
            if (hide == value)
                return;

            hide = value;

            OnHintUpdated(nameof(Hide));
        }
    }

    /// <summary>
    ///     Gets or sets the way the HintParser handle hint's position when the screen resolution changes.
    /// </summary>
    public ResolutionOption ResolutionOption
    {
        get => resolutionOption;

        set
        {
            if (resolutionOption == value)
                return;
            resolutionOption = value;

            OnHintUpdated(nameof(ResolutionOption));
        }
    }

    /// <summary>
    ///     Gets or sets how far, in units, the hint is kept away from the screen edge when
    ///     <see cref="ResolutionOption" /> is <see cref="ResolutionOption.Offset" /> and the hint
    ///     is left- or right-aligned. <c>0</c> (default) sends the hint all the way to the edge;
    ///     a positive value insets it inward by that amount.
    /// </summary>
    public float EdgeMargin
    {
        get => edgeMargin;

        set
        {
            if (edgeMargin == value)
                return;
            edgeMargin = value;

            OnHintUpdated(nameof(EdgeMargin));
        }
    }

    public ParameterCollection Parameters { get; private set; } = new();

    internal TransitionState? FontSizeTransitionState { get; set; }

    internal float CurrentFontSize
    {
        get
        {
            TransitionState? transitionState = FontSizeTransitionState;

            if (transitionState is null)
                return fontSize;

            return transitionState.CurrentValue;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Attempts to update the hint content in response to an update-available event.
    /// </summary>
    /// <param name="ev">The event arguments containing the player display context.</param>
    public virtual void TryUpdateHint(UpdateAvailableEventArg ev)
    {
        Content.TryUpdate(new ContentUpdateArg(this, ev.PlayerDisplay));
    }

    internal void CopyFieldsFrom(AbstractHint copyFrom)
    {
        Id = copyFrom.Id;
        syncSpeed = copyFrom.SyncSpeed;
        fontSize = copyFrom.FontSize;
        lineHeight = copyFrom.LineHeight;
        content = copyFrom.Content;
        hide = copyFrom.Hide;
        fontSizeTransition = copyFrom.FontSizeTransition;
        FontSizeTransitionState = copyFrom.FontSizeTransitionState;
        resolutionOption = copyFrom.ResolutionOption;
        edgeMargin = copyFrom.EdgeMargin;
        Parameters = copyFrom.Parameters;
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event and notifies the update analyser of a change.
    /// </summary>
    /// <param name="argumentName">The name of the property that changed.</param>
    protected virtual void OnHintUpdated(string argumentName)
    {
        UpdateAnalyser.OnUpdate();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argumentName));
    }

    private void OnContentUpdate()
    {
        OnHintUpdated(nameof(Content));
    }

    #endregion
}