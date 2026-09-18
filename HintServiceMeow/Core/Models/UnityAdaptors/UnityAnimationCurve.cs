using System;
using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Interface;
using UnityEngine;

namespace HintServiceMeow.Core.Models.UnityAdaptors;

/// <summary>
///     Represents an animation curve that encapsulates Unity's AnimationCurve, providing methods to evaluate, create,
///     and manipulate keyframes and curve behavior.
/// </summary>
/// <remarks>
///     Use this class to define and modify animation curves for interpolating values over time, such
///     as in animation systems or procedural motion. The curve supports different wrap modes and allows for adding,
///     moving, and removing keyframes. Changes to the curve automatically update the internal cache of keyframes. This
///     class is intended for scenarios where Unity's AnimationCurve functionality needs to be accessed or extended in a
///     type-safe and convenient manner.
/// </remarks>
public class UnityAnimationCurve : IAnimationCurve, IEquatable<AnimationCurve>, IEquatable<UnityAnimationCurve>
{
    private readonly object cacheLock = new(); // Since curve is thread safe, only used for cache
    private readonly AnimationCurve curve;
    private HsmKeyFrame[]? keyFramesCache;

    HsmKeyFrame[] IAnimationCurve.Keys
    {
        get
        {
            lock (cacheLock)
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
            lock (cacheLock)
            {
                curve.keys = value;
                keyFramesCache = null; // Clear Cache
            }
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
        lock (cacheLock)
        {
            keyFramesCache = null; // Clear Cache
            return curve.AddKey(time, value);
        }
    }

    public int AddKey(Keyframe key)
    {
        lock (cacheLock)
        {
            keyFramesCache = null; // Clear Cache
            return curve.AddKey(key);
        }
    }

    public int MoveKey(int index, Keyframe key)
    {
        lock (cacheLock)
        {
            keyFramesCache = null; // Clear Cache
            return curve.MoveKey(index, key);
        }
    }

    public void RemoveKey(int index)
    {
        lock (cacheLock)
        {
            keyFramesCache = null; // Clear Cache
            curve.RemoveKey(index);
        }
    }

    public void ClearKeys()
    {
        lock (cacheLock)
        {
            keyFramesCache = null; // Clear Cache
            curve.ClearKeys();
        }
    }

    public void SmoothTangents(int index, float weight)
    {
        lock (cacheLock)
        {
            curve.SmoothTangents(index, weight);
            keyFramesCache = null; // Clear Cache
        }
    }

    public void CopyFrom(AnimationCurve other)
    {
        lock (cacheLock)
        {
            curve.CopyFrom(other);
            keyFramesCache = null; // Clear Cache
        }
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