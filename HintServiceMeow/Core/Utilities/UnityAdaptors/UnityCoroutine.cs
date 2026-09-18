using HintServiceMeow.Core.Interface;
using MEC;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors;

internal class UnityCoroutine : ICoroutine
{
    private readonly CoroutineHandle handle;

    public bool IsRunning => handle.IsRunning;

    public bool IsPaused => handle.IsAliveAndPaused;

    internal UnityCoroutine(CoroutineHandle handle)
    {
        this.handle = handle;
    }

    public void Kill()
    {
        Timing.KillCoroutines(handle);
    }

    public void Pause()
    {
        Timing.PauseCoroutines(handle);
    }

    public void Resume()
    {
        Timing.ResumeCoroutines(handle);
    }
}