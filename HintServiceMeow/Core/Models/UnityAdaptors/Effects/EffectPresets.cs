using Hints;
using HintServiceMeow.Core.Interface;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Effects;

internal static class EffectPresets
{
    public static Keyframe[] CreateBumpKeyframes(float floorValue, float bumpValue, int count, float duration = 1f)
    {
        return HintEffectPresets.CreateBumpKeyframes(floorValue, bumpValue, count, duration);
    }

    public static AnimationCurve CreateBumpCurve(float floorValue, float bumpValue, int count, float duration = 1f)
    {
        return HintEffectPresets.CreateBumpCurve(floorValue, bumpValue, count, duration);
    }

    public static AnimationCurve CreateTrailingBumpCurve(float floorValue, float bumpValue, int count, float startTrailPercent, float duration = 1f)
    {
        return HintEffectPresets.CreateTrailingBumpCurve(floorValue, bumpValue, count, startTrailPercent, duration);
    }

    public static TransparencyCurveEffect FadeIn(float durationScalar = 1f, float startScalar = 0f, float iterations = 1f)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, iterations, 1f);
        curve.postWrapMode = WrapMode.Loop;
        return new TransparencyCurveEffect(curve, startScalar, durationScalar);
    }

    public static TransparencyCurveEffect FadeOut(float durationScalar = 1f, float startScalar = 0f, float iterations = 1f)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 1f, iterations, 0f);
        curve.postWrapMode = WrapMode.Loop;
        return new TransparencyCurveEffect(curve, startScalar, durationScalar);
    }

    public static IEffect[] FadeInAndOut(float window, float durationScalar = 1f, float startScalar = 0f)
    {
        float num = (durationScalar - window) / 2f;
        return
        [
            FadeIn(num, startScalar),
            FadeOut(num, startScalar + durationScalar - num)
        ];
    }

    public static TransparencyCurveEffect PulseAlpha(float floorValue, float peakValue, float iterationScalar = 1f, float startOffset = 0f)
    {
        return new TransparencyCurveEffect(CreateBumpCurve(floorValue, peakValue, 1, iterationScalar), startOffset);
    }

    public static TransparencyCurveEffect TrailingPulseAlpha(float floorValue, float peakValue, float startTrailScalar, float iterationScalar = 1f, float startScalar = 0f, int count = 1)
    {
        return new TransparencyCurveEffect(CreateTrailingBumpCurve(floorValue, peakValue, count, startTrailScalar, iterationScalar), startScalar);
    }
}