using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Transition;

namespace HintServiceMeow.Core.Models.Hints;

/// <summary>
///     Represents a hint displayed at a fixed position on the player's screen.
/// </summary>
public class Hint : AbstractHint
{
    private HintAlignment alignment = HintAlignment.Center;
    private HintVerticalAlign yCoordinateAlign = HintVerticalAlign.Middle;

    private float xCoordinate;
    private float yCoordinate = 700;

    /// <summary>
    ///     Gets or sets the Y coordinate of the hint. Higher Y coordinate means lower position
    ///     Select from 0 to 1080 on any screen.
    /// </summary>
    public float YCoordinate
    {
        get => yCoordinate;

        set
        {
            if (yCoordinate.Equals(value))
                return;

            yCoordinate = value;

            OnHintUpdated(nameof(YCoordinate));
        }
    }

    /// <summary>
    ///     Gets or sets the horizontal offset of the hint. Higher X coordinate means more to the right
    ///     This value should be between -1200 to 1200 including text length.
    /// </summary>
    public float XCoordinate
    {
        get => xCoordinate;

        set
        {
            if (xCoordinate.Equals(value))
                return;

            xCoordinate = value;

            OnHintUpdated(nameof(XCoordinate));
        }
    }

    /// <summary>
    ///     Gets or sets alignment of the hint.
    /// </summary>
    public HintAlignment Alignment
    {
        get => alignment;

        set
        {
            if (alignment == value)
                return;

            alignment = value;

            OnHintUpdated(nameof(Alignment));
        }
    }

    /// <summary>
    ///     Gets or sets the vertical alignment reference point used when interpreting <see cref="YCoordinate" />.
    /// </summary>
    public HintVerticalAlign YCoordinateAlign
    {
        get => yCoordinateAlign;

        set
        {
            if (yCoordinateAlign == value)
                return;

            yCoordinateAlign = value;

            OnHintUpdated(nameof(YCoordinateAlign));
        }
    }

    public Transition.Transition? XCoordinateTransition
    {
        get;

        set
        {
            if (field == value)
                return;

            field = value;

            OnHintUpdated(nameof(XCoordinateTransition));
        }
    }

    public Transition.Transition? YCoordinateTransition
    {
        get;

        set
        {
            if (field == value)
                return;

            field = value;

            OnHintUpdated(nameof(YCoordinateTransition));
        }
    }

    internal TransitionState? XCoordinateTransitionState { get; set; }

    internal TransitionState? VOffsetTransitionState { get; set; }

    internal float CurrentXCoordinate
    {
        get
        {
            TransitionState? transitionState = XCoordinateTransitionState;

            if (transitionState is null)
                return xCoordinate;

            return transitionState.CurrentValue;
        }
    }

    internal void GetFromDynamicHint(DynamicHint dynamicHint, float x, float y)
    {
        CopyFieldsFrom(dynamicHint);

        xCoordinate = x;
        yCoordinate = y;

        alignment = HintAlignment.Center;
        yCoordinateAlign = HintVerticalAlign.Bottom;
        ResolutionOption = ResolutionOption.None;
    }

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="Hint" /> class with default values.
    /// </summary>
    public Hint()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Hint" /> class by copying properties from an existing hint.
    /// </summary>
    /// <param name="hint">The hint whose properties are copied into this instance.</param>
    public Hint(Hint hint) : base(hint)
    {
        yCoordinate = hint.yCoordinate;
        xCoordinate = hint.xCoordinate;
        alignment = hint.alignment;
        yCoordinateAlign = hint.yCoordinateAlign;
    }

    #endregion
}