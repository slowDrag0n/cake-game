using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelSequence : MonoBehaviour
{
    public UnityEvent OnSequenceStart;
    public UnityEvent OnSequenceDone;

    protected virtual void Start()
    {
        OnSequenceStart?.Invoke();
    }

    public void ResetLocalTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;
    }
}
