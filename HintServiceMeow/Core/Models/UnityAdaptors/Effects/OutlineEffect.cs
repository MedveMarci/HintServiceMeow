using Hints;
using HintServiceMeow.Core.Interface;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Effects;

public class OutlineEffect(Color32 outlineColor, float outlineWidth, float startScalar = 0f, float durationScalar = 1f) : IEffect
{
    public Color32 OutlineColor { get; set; } = outlineColor;

    public float OutlineWidth { get; set; } = outlineWidth;

    public float StartScalar { get; set; } = startScalar;

    public float DurationScalar { get; set; } = durationScalar;

    public HintEffect GetHintEffect()
    {
        return new global::Hints.OutlineEffect(OutlineColor, OutlineWidth, StartScalar, DurationScalar);
    }
}