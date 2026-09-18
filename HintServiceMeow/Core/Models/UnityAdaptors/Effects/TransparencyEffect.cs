using Hints;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Effects;

public class TransparencyEffect : IEffect
{
    /// <summary>
    ///     Gets or sets the transparency level.
    /// </summary>
    public float Transparency { get; set; }

    /// <summary>
    ///     Gets or sets the initial start point.
    /// </summary>
    public float StartPoint { get; set; }

    /// <summary>
    ///     Gets or sets the duration, in seconds.
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransparencyEffect" /> class.
    ///     parameters.
    /// </summary>
    /// <param name="alpha">The transparency level to apply. Must be between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
    /// <param name="startScalar">The starting point of the effect, as a scalar value.</param>
    /// <param name="durationScalar">The duration of the effect, as a scalar value.</param>
    public TransparencyEffect(float alpha, float startScalar = 0f, float durationScalar = 1f)
    {
        Transparency = alpha;
        StartPoint = startScalar;
        Duration = durationScalar;
    }

    /// <inheritdoc />
    public HintEffect GetHintEffect()
    {
        return new AlphaEffect(Transparency, StartPoint, Duration);
    }
}