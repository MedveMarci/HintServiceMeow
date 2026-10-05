using System.Collections.Generic;
using System.ComponentModel;
using HintServiceMeow.ApiFeatures;
using HintServiceMeow.Core.Interface;
using MEC;
using Mathf = UnityEngine.Mathf;

namespace HintServiceMeow.Core.Models.UnityAdaptors;

internal class ScpslScreenResolution : IScreenResolution
{
    private static readonly List<ScpslScreenResolution> Instances = [];
    private static CoroutineHandle _coroutineHandle;

    private static float _yScreenEdge;
    private readonly ReferenceHub? referenceHub;
    private float xScreenEdge;

    public float XyRatio { get; private set; }

    public ScpslScreenResolution(ReferenceHub referenceHub)
    {
        if (_yScreenEdge == 0) _yScreenEdge = AspectRatioSync.YScreenEdge;

        this.referenceHub = referenceHub;

        _ = TryUpdate();

        if (!_coroutineHandle.IsRunning) _coroutineHandle = Timing.RunCoroutine(CoroutineMethod());

        Instances.Add(this);

        LogManager.Debug($"[ScpslScreenResolution] ScpslScreenResolution object initialized for player {referenceHub.PlayerId}. Current X/Y ratio: {XyRatio}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static IEnumerator<float> CoroutineMethod()
    {
        LogManager.Debug("[ScpslScreenResolution] Aspect ratio synchronization coroutine started.");

        while (true)
        {
            List<ScpslScreenResolution> updatedInstances = [];

            Instances.RemoveAll(x => x.referenceHub == null);

            foreach (ScpslScreenResolution resolution in Instances)
                if (resolution.TryUpdate())
                    updatedInstances.Add(resolution);

            foreach (ScpslScreenResolution resolution in updatedInstances)
            {
                resolution.PropertyChanged?.Invoke(resolution, new PropertyChangedEventArgs(nameof(XyRatio)));
                LogManager.Debug($"[ScpslScreenResolution] ScpslScreenResolution object for player {resolution.referenceHub!.PlayerId} updated. Current X/Y ratio: {resolution.XyRatio}");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    private bool TryUpdate()
    {
        if (xScreenEdge != referenceHub!.aspectRatioSync.XScreenEdge)
        {
            xScreenEdge = referenceHub.aspectRatioSync.XScreenEdge;
            XyRatio = Mathf.Tan(xScreenEdge * Mathf.Deg2Rad) / Mathf.Tan(_yScreenEdge * Mathf.Deg2Rad);

            return true;
        }

        return false;
    }
}