using Hints;

namespace HintServiceMeow.Core.Interface;

/// <summary>
///     Defines a contract for creating hint effects.
/// </summary>
public interface IEffect
{
    /// <summary>
    ///     Get <see cref="HintEffect" />.
    /// </summary>
    /// <returns>An instance of <see cref="HintEffect" />.</returns>
    HintEffect GetHintEffect();
}