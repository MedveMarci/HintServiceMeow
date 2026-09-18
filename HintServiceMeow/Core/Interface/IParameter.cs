using Hints;

namespace HintServiceMeow.Core.Interface;

public interface IParameter
{
    /// <summary>
    ///     Get an instance of <see cref="HintParameter" />.
    /// </summary>
    /// <returns>A <see cref="HintParameter" /> instance.</returns>
    HintParameter GetScpslHintParameter();
}