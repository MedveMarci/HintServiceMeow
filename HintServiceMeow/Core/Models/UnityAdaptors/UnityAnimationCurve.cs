using System;
using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Interface;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors;

public class UnityAnimationCurve : IAnimationCurve, IEquatable<AnimationCurve>, IEquatable<UnityAnimationCurve>
{
    private readonly AnimationCurve curve;
    private HsmKeyFrame[]? keyFramesCache;

    HsmKeyFrame[] IAnimationCurve.Keys
    {
        get
        {
            if (keyFramesCache == null || keyFramesCache.Length != curve.length)
            {
                keyFramesCache = new HsmKeyFrame[curve.length];
                Keyframe[] unityKeys = curve.keys;
                for (int i = 0; i < unityKeys.Length; i++)
                {
                    Keyframe keyFrame = unityKeys[i];
                    keyFramesCache[i] = new HsmKeyFrame(keyFrame.time, keyFrame.value, keyFrame.inTangent, keyFrame.outTangent);
                }
            }

            return keyFramesCache;
        }
    }

    HsmWrapMode IAnimationCurve.PreWrapMode
    {
        get => curve.preWrapMode.ToHsmWrapMode();
        set => curve.preWrapMode = value.ToUnityWrapMode();
    }

    HsmWrapMode IAnimationCurve.PostWrapMode
    {
        get => curve.postWrapMode.ToHsmWrapMode();
        set => curve.postWrapMode = value.ToUnityWrapMode();
    }

    public Keyframe[] Keys
    {
        get => curve.keys;
        set
        {
            curve.keys = value;
            keyFramesCache = null; // Clear Cache
        }
    }

    public int Length => curve.length;

    public WrapMode PreWrapMode
    {
        get => curve.preWrapMode;
        set => curve.preWrapMode = value;
    }

    public WrapMode PostWrapMode
    {
        get => curve.postWrapMode;
        set => curve.postWrapMode = value;
    }

    public Keyframe this[int index] => curve[index];

    public UnityAnimationCurve(AnimationCurve curve)
    {
        this.curve = curve ?? throw new ArgumentNullException(nameof(curve));
    }

    public UnityAnimationCurve(params Keyframe[] keys)
    {
        curve = new AnimationCurve(keys);
    }

    public UnityAnimationCurve()
    {
        curve = new AnimationCurve();
    }

    public static implicit operator UnityAnimationCurve(AnimationCurve c)
    {
        return new UnityAnimationCurve(c);
    }

    public static explicit operator AnimationCurve(UnityAnimationCurve c)
    {
        return c.curve;
    }

    public static UnityAnimationCurve Constant(float timeStart, float timeEnd, float value)
    {
        return new UnityAnimationCurve(AnimationCurve.Constant(timeStart, timeEnd, value));
    }

    public static UnityAnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
    {
        return new UnityAnimationCurve(AnimationCurve.Linear(timeStart, valueStart, timeEnd, valueEnd));
    }

    public static UnityAnimationCurve EaseInOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
    {
        return new UnityAnimationCurve(AnimationCurve.EaseInOut(timeStart, valueStart, timeEnd, valueEnd));
    }

    public float Evaluate(float time)
    {
        return curve.Evaluate(time);
    }

    public int AddKey(float time, float value)
    {
        keyFramesCache = null; // Clear Cache
        return curve.AddKey(time, value);
    }

    public int AddKey(Keyframe key)
    {
        keyFramesCache = null; // Clear Cache
        return curve.AddKey(key);
    }

    public int MoveKey(int index, Keyframe key)
    {
        keyFramesCache = null; // Clear Cache
        return curve.MoveKey(index, key);
    }

    public void RemoveKey(int index)
    {
        keyFramesCache = null; // Clear Cache
        curve.RemoveKey(index);
    }

    public void ClearKeys()
    {
        keyFramesCache = null; // Clear Cache
        curve.ClearKeys();
    }

    public void SmoothTangents(int index, float weight)
    {
        curve.SmoothTangents(index, weight);
        keyFramesCache = null; // Clear Cache
    }

    public void CopyFrom(AnimationCurve other)
    {
        curve.CopyFrom(other);
        keyFramesCache = null; // Clear Cache
    }

    public override bool Equals(object other)
    {
        if (other is AnimationCurve unityCurve)
            return curve.Equals(unityCurve);
        if (other is UnityAnimationCurve hsmCurve)
            return curve.Equals(hsmCurve.curve);

        return false;
    }

    public bool Equals(AnimationCurve other)
    {
        if (other is null)
            return false;

        return curve.Equals(other);
    }

    public bool Equals(UnityAnimationCurve other)
    {
        if (other is null)
            return false;

        return curve.Equals(other.curve);
    }

    public override int GetHashCode()
    {
        return curve.GetHashCode();
    }
}