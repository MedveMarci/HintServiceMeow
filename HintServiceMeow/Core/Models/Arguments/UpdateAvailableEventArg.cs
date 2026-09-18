using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Arguments;

/// <summary>
///     Provides data for the update-available event raised by a <see cref="PlayerDisplay" />.
/// </summary>
public class UpdateAvailableEventArg
{
    /// <summary>
    ///     Gets or sets the player display that is ready for a hint update.
    /// </summary>
    public PlayerDisplay PlayerDisplay { get; set; }

    internal UpdateAvailableEventArg(PlayerDisplay playerDisplay)
    {
        PlayerDisplay = playerDisplay;
    }
}