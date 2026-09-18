using System;
using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

/// <summary>
///     Wraps a raw SCP:SL <see cref="HintParameter" /> so that it can flow through
///     HintServiceMeow's parameter pipeline. This is used by the compatibility adaptor
///     so that parameters attached to intercepted hints keep working exactly like the
///     parameters of a native HintServiceMeow hint.
/// </summary>
public class ScpslHintParameterWrapper : IParameter
{
    private readonly HintParameter parameter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScpslHintParameterWrapper" /> class.
    /// </summary>
    /// <param name="parameter">The raw SCP:SL hint parameter to wrap.</param>
    public ScpslHintParameterWrapper(HintParameter parameter)
    {
        this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    /// <inheritdoc />
    public HintParameter GetScpslHintParameter()
    {
        return parameter;
    }
}