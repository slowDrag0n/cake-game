using System;
using System.Collections;
using SexyDevs.Utils;
using UnityEngine;

public class CoroutineRunner : Singleton<CoroutineRunner>
{
    public void WaitForFixedUpdateAndExecute(Action action)
    {
        StartCoroutine(WaitForFixedUpdateAndExecuteRoutine(action));
    }

    public void WaitTillFrameEndAndExecute(Action action)
    {
        StartCoroutine(WaitTillFrameEndAndExecuteRoutine(action));
    }

    public void WaitForUpdateAndExecute(Action action)
    {
        StartCoroutine(WaitForUpdateAndExecuteRoutine(action));
    }

    public void WaitForRealTimeDelayAndExecute(Action action, float delay)
    {
        StartCoroutine(WaitForRealTimeDelayAndExecuteRoutine(action, delay));
    }
    public void WaitForTimeDelayAndExecute(Action action, float delay)
    {
        StartCoroutine(WaitForTimeDelayAndExecuteRoutine(action, delay));
    }
    private IEnumerator WaitForFixedUpdateAndExecuteRoutine(Action action)
    {
        yield return new WaitForFixedUpdate();
        action?.Invoke();
    }

    private IEnumerator WaitTillFrameEndAndExecuteRoutine(Action action)
    {
        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }

    private IEnumerator WaitForUpdateAndExecuteRoutine(Action action)
    {
        yield return null;
        action?.Invoke();
    }

    private IEnumerator WaitForRealTimeDelayAndExecuteRoutine(Action action, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        action?.Invoke();
    }
    private IEnumerator WaitForTimeDelayAndExecuteRoutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }


}