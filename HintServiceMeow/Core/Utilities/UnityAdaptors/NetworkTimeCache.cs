using System.Collections.Generic;
using System.Threading;
using HintServiceMeow.Core.Interface;
using Mirror;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal static class NetworkTimeCache
{
    private static double _cachedTime;

    public static double Time => Volatile.Read(ref _cachedTime);

    public static IEnumerator<float> Update()
    {
        while (true)
        {
            Volatile.Write(ref _cachedTime, NetworkTime.time);
            yield return 0f;
        }
    }

    public static void Initialize(ICoroutineRunner runner)
    {
        runner.StartCoroutine(Update());
    }
}