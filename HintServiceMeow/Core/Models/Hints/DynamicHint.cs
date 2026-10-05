using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Models.Hints;

/// <summary>
///     Represents a hint that dynamically positions itself within defined boundaries on the player's screen,
///     avoiding overlap with other hints.
/// </summary>
public class DynamicHint : AbstractHint
{
    private float topBoundary;
    private float bottomBoundary = 1000;

    private float leftBoundary = -1200;
    private float rightBoundary = 1200;

    private float targetY = 700;
    private float targetX;

    private float topMargin = 5;
    private float bottomMargin = 5;
    private float leftMargin = 100;
    private float rightMargin = 100;

    private HintPriority priority = HintPriority.Medium;
    private DynamicHintStrategy strategy = DynamicHintStrategy.Hide;

    /// <summary>
    ///     Gets or sets the top boundary of the dynamic hint.
    /// </summary>
    public float TopBoundary
    {
        get => topBoundary;

        set
        {
            if (topBoundary.Equals(value))
                return;

            topBoundary = value;

            OnHintUpdated(nameof(TopBoundary));
        }
    }

    /// <summary>
    ///     Gets or sets the bottom boundary of the dynamic hint.
    /// </summary>
    public float BottomBoundary
    {
        get => bottomBoundary;

        set
        {
            if (bottomBoundary.Equals(value))
                return;

            bottomBoundary = value;

            OnHintUpdated(nameof(BottomBoundary));
        }
    }

    /// <summary>
    ///     Gets or sets the left boundary of the dynamic hint. Should be more than -1200.
    /// </summary>
    public float LeftBoundary
    {
        get => leftBoundary;

        set
        {
            if (leftBoundary.Equals(value))
                return;

            leftBoundary = value;

            OnHintUpdated(nameof(LeftBoundary));
        }
    }

    /// <summary>
    ///     Gets or sets the right boundary of the dynamic hint. Should be less than 1200.
    /// </summary>
    public float RightBoundary
    {
        get => rightBoundary;

        set
        {
            if (rightBoundary.Equals(value))
                return;

            rightBoundary = value;

            OnHintUpdated(nameof(RightBoundary));
        }
    }

    /// <summary>
    ///     Gets or sets the Y coordinate that dynamic hint will try to reach.
    /// </summary>
    public float TargetY
    {
        get => targetY;

        set
        {
            if (targetY.Equals(value))
                return;

            targetY = value;

            OnHintUpdated(nameof(TargetY));
        }
    }

    /// <summary>
    ///     Gets or sets the X coordinate that dynamic hint will try to reach.
    /// </summary>
    public float TargetX
    {
        get => targetX;

        set
        {
            if (targetX.Equals(value))
                return;

            targetX = value;

            OnHintUpdated(nameof(TargetX));
        }
    }

    /// <summary>
    ///     Gets or sets the top margin in pixels between this hint and any hint placed above it.
    /// </summary>
    public float TopMargin
    {
        get => topMargin;

        set
        {
            if (topMargin.Equals(value))
                return;

            topMargin = value;

            OnHintUpdated(nameof(TopMargin));
        }
    }

    /// <summary>
    ///     Gets or sets the bottom margin in pixels between this hint and any hint placed below it.
    /// </summary>
    public float BottomMargin
    {
        get => bottomMargin;

        set
        {
            if (bottomMargin.Equals(value))
                return;

            bottomMargin = value;

            OnHintUpdated(nameof(BottomMargin));
        }
    }

    /// <summary>
    ///     Gets or sets the left margin in pixels applied when this hint is positioned horizontally.
    /// </summary>
    public float LeftMargin
    {
        get => leftMargin;

        set
        {
            if (leftMargin.Equals(value))
                return;

            leftMargin = value;

            OnHintUpdated(nameof(LeftMargin));
        }
    }

    /// <summary>
    ///     Gets or sets the right margin in pixels applied when this hint is positioned horizontally.
    /// </summary>
    public float RightMargin
    {
        get => rightMargin;

        set
        {
            if (rightMargin.Equals(value))
                return;

            rightMargin = value;

            OnHintUpdated(nameof(RightMargin));
        }
    }

    /// <summary>
    ///     Gets or sets the priority of the hint, higher priority means the hint is less likely to be covered by other hint.
    /// </summary>
    public HintPriority Priority
    {
        get => priority;

        set
        {
            if (priority == value)
                return;

            priority = value;

            OnHintUpdated(nameof(Priority));
        }
    }

    /// <summary>
    ///     Gets or sets the fallback strategy used when no valid display position is available.
    /// </summary>
    public DynamicHintStrategy Strategy
    {
        get => strategy;

        set
        {
            if (strategy == value)
                return;

            strategy = value;

            OnHintUpdated(nameof(Strategy));
        }
    }

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicHint" /> class with default values.
    /// </summary>
    public DynamicHint()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicHint" /> class by copying properties from an existing dynamic
    ///     hint.
    /// </summary>
    /// <param name="hint">The dynamic hint whose properties are copied into this instance.</param>
    public DynamicHint(DynamicHint hint) : base(hint)
    {
        topBoundary = hint.topBoundary;
        bottomBoundary = hint.bottomBoundary;

        leftBoundary = hint.leftBoundary;
        rightBoundary = hint.rightBoundary;

        targetY = hint.targetY;
        targetX = hint.targetX;


        topMargin = hint.topMargin;
        bottomMargin = hint.bottomMargin;
        leftMargin = hint.leftMargin;
        rightMargin = hint.rightMargin;

        priority = hint.priority;
        strategy = hint.strategy;
    }

    #endregion
}