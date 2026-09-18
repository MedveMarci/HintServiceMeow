using HintServiceMeow.Core.Models.Arguments;

namespace HintServiceMeow.Core.Interface;

/// <summary>
///     Defines a display output target that can handle processed hint text.
/// </summary>
public interface IDisplayOutput
{
    /// <summary>
    ///     Gets the current screen resolution settings for the display.
    /// </summary>
    IScreenResolution ScreenResolution { get; }

    /// <summary>
    ///     Handle the processed hint text.
    /// </summary>
    /// <param name="ev">The arguments containing the hint content and related arguments.</param>
    void ShowHint(DisplayOutputArg ev);
}