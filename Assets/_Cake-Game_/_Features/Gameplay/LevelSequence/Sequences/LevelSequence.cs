using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelSequence : MonoBehaviour
{
    public string SequenceId = "";
    [Space(15)]
    public UnityEvent OnSequenceStart;
    public UnityEvent OnSequenceDone;

    protected virtual void Start()
    {
        OnSequenceStart?.Invoke();
    }

    protected virtual void OnDisable()
    {
        OnSequenceDone?.Invoke();
        Debug.Log("Sequence Done: " + SequenceId);
    }

    public void ResetLocalTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;
    }
}
