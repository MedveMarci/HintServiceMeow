using System;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.Arguments;

/// <summary>
///     Provides data passed to an <see cref="ICompatibilityAdaptor" /> when sending a hint to a player.
/// </summary>
public class CompatibilityAdaptorArg
{
    /// <summary>
    ///     Gets the name of the assembly that registered the hint.
    /// </summary>
    public string AssemblyName { get; }

    /// <summary>
    ///     Gets the formatted hint content string to display, or <see langword="null" /> if there is no content.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    ///     Gets the duration in seconds for which the hint should be displayed.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    ///     Gets the parameters attached to the hint. These are resolved exactly like the
    ///     parameters of a native HintServiceMeow hint. Empty when the hint has no parameters.
    /// </summary>
    public IParameter[] Parameters { get; }

    internal CompatibilityAdaptorArg(string assemblyName, string? content, float duration, IParameter[]? parameters = null)
    {
        AssemblyName = assemblyName;
        Content = content;
        Duration = duration;
        Parameters = parameters ?? [];
    }
}