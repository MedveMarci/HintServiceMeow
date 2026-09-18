using Hints;
using HintServiceMeow.Core.Interface;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Effects;

public class TransparencyCurveEffect : IEffect
{
    /// <summary>
    ///     Gets or sets the animation curve used to define transparency changes over time.
    /// </summary>
    public AnimationCurve Curve { get; set; }

    /// <summary>
    ///     Gets or sets the initial time scalar that determines the starting point of the transparency effect on the curve.
    /// </summary>
    public float StartPoint { get; set; }

    /// <summary>
    ///     Gets or sets the scalar value used to adjust the duration of an operation.
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransparencyCurveEffect" /> class using the specified animation curve
    ///     and
    ///     scalar values.
    /// </summary>
    /// <param name="curve">The animation curve that defines the transparency effect over time. Cannot be null.</param>
    /// <param name="startScalar">
    ///     The starting scalar value for the transparency effect. Represents the initial point on the curve. Defaults
    ///     to 0.
    /// </param>
    /// <param name="durationScalar">
    ///     The duration scalar value for the transparency effect. Determines how long the curve is applied. Defaults to
    ///     1.
    /// </param>
    public TransparencyCurveEffect(AnimationCurve curve, float startScalar = 0f, float durationScalar = 1f)
    {
        Curve = curve;
        StartPoint = startScalar;
        Duration = durationScalar;
    }

    public HintEffect GetHintEffect()
    {
        return new AlphaCurveHintEffect(Curve, StartPoint, Duration);
    }
}