using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleSequenceInteractable : MonoBehaviour
{

    [Space]
    public UnityEvent OnDoneMoveIn;
    [Space]
    public UnityEvent OnDonePouring;
    [Space]
    public UnityEvent OnDoneMoveOut;

    public void InvokeOnDoneMoveIn()
    {
        OnDoneMoveIn?.Invoke();
    }

    public void InvokeOnDonePouring()
    {
        OnDonePouring?.Invoke();
    }

    public void InvokeOnDoneMoveOut()
    {
        OnDoneMoveOut?.Invoke();
    }

}
