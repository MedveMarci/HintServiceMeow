using System.Collections.Generic;
using System.ComponentModel;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Tools;
using MEC;
using Mathf = UnityEngine.Mathf;

namespace HintServiceMeow.Core.Models.UniryAdaptors;

internal class ScpslScreenResolution : IScreenResolution
{
    private static readonly object StaticStatusLock = new();
    private static readonly List<ScpslScreenResolution> Instances = [];
    private static CoroutineHandle coroutineHandle;

    private static float yScreenEdge;
    private readonly ReferenceHub? referenceHub;
    private volatile float xScreenEdge;
    private volatile float xyRatio;

    public float XyRatio => xyRatio;

    public ScpslScreenResolution(ReferenceHub referenceHub)
    {
        lock (StaticStatusLock)
        {
            if (yScreenEdge == 0) yScreenEdge = AspectRatioSync.YScreenEdge;

            this.referenceHub = referenceHub;

            _ = TryUpdate();

            if (!coroutineHandle.IsRunning) coroutineHandle = Timing.RunCoroutine(CoroutineMethod());

            Instances.Add(this);

            Logger.Instance.Debug($"[ScpslScreenResolution] ScpslScreenResolution object initialized for player {referenceHub.PlayerId}. Current X/Y ratio: {XyRatio}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static IEnumerator<float> CoroutineMethod()
    {
        Logger.Instance.Debug("[ScpslScreenResolution] Aspect ratio synchronization coroutine started.");

        while (true)
        {
            List<ScpslScreenResolution> updatedInstances = [];

            lock (StaticStatusLock)
            {
                Instances.RemoveAll(x => x.referenceHub == null);

                foreach (ScpslScreenResolution resolution in Instances)
                    if (resolution.TryUpdate())
                        updatedInstances.Add(resolution);
            }

            foreach (ScpslScreenResolution resolution in updatedInstances)
            {
                resolution.PropertyChanged?.Invoke(resolution, new PropertyChangedEventArgs(nameof(XyRatio)));
                Logger.Instance.Debug($"[ScpslScreenResolution] ScpslScreenResolution object for player {resolution.referenceHub!.PlayerId} updated. Current X/Y ratio: {resolution.XyRatio}");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    private bool TryUpdate()
    {
        if (xScreenEdge != referenceHub!.aspectRatioSync.XScreenEdge)
        {
            xScreenEdge = referenceHub.aspectRatioSync.XScreenEdge;
            xyRatio = Mathf.Tan(xScreenEdge * Mathf.Deg2Rad) / Mathf.Tan(yScreenEdge * Mathf.Deg2Rad);

            return true;
        }

        return false;
    }
}