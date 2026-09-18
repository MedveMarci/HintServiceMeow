using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Arguments;

/// <summary>
///     Provides contextual data for a content update operation on an <see cref="AbstractHintContent" />.
/// </summary>
public class ContentUpdateArg
{
    /// <summary>
    ///     Gets the hint whose content is being updated.
    /// </summary>
    public AbstractHint Hint { get; }

    /// <summary>
    ///     Gets the player display associated with this content update.
    /// </summary>
    public PlayerDisplay PlayerDisplay { get; }

    internal ContentUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay)
    {
        Hint = hint;
        PlayerDisplay = playerDisplay;
    }
}