using System.Collections.Generic;
using HintServiceMeow.Core.Interface;
using Mirror;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal static class NetworkTimeCache
{
    public static double Time { get; private set; }

    public static IEnumerator<float> Update()
    {
        while (true)
        {
            Time = NetworkTime.time;
            yield return 0f;
        }
    }

    public static void Initialize(ICoroutineRunner runner)
    {
        runner.StartCoroutine(Update());
    }
}