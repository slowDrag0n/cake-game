using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleSequenceInteractable : MonoBehaviour
{

    [Header("Move In Events")]
    public UnityEvent OnBeginMoveIn;
    public UnityEvent OnDoneMoveIn;
    [Space]
    [Header("Pouring Events")]
    public UnityEvent OnBeginPouring;
    public UnityEvent OnDonePouring;
    [Space]
    [Header("Move Out Events")]
    public UnityEvent OnBeginMoveOut;
    public UnityEvent OnDoneMoveOut;

    public void InvokeOnBeginMoveIn()
    {
        OnBeginMoveIn?.Invoke();
    }

    public void InvokeOnDoneMoveIn()
    {
        OnDoneMoveIn?.Invoke();
    }

    public void InvokeOnBeginPouring()
    {
        OnBeginPouring?.Invoke();
    }

    public void InvokeOnDonePouring()
    {
        OnDonePouring?.Invoke();
    }

    public void InvokeOnBeginMoveOut()
    {
        OnBeginMoveOut?.Invoke();
    }

    public void InvokeOnDoneMoveOut()
    {
        OnDoneMoveOut?.Invoke();
    }

}
