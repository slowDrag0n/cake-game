using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent[] Events;

    public void RunAnimationEvent(int index)
    {
        try
        {
            Events[index]?.Invoke();
        }
        catch(ArgumentOutOfRangeException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
