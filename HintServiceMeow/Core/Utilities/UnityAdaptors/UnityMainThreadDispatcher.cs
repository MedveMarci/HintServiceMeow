using System;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal class UnityMainThreadDispatcher : IMainThreadDispatcher
{
    public void Dispatch(Action action)
    {
        MainThreadDispatcher.Dispatch(action);
    }
}