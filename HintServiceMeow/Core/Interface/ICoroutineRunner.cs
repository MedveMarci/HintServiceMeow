using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Interface;

internal interface ICoroutineRunner
{
    ICoroutine StartCoroutine(IEnumerator<float> routine);

    ICoroutine CallAfter(TimeSpan time, Action action);
}