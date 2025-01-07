using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndingSequence : LevelSequence
{
    public Transform EndingCakeHolder;
    public Transform[] EndingCakeChildren;
    public Transform EndingSequenceCakeHolder;

    private void Start()
    {
        foreach(Transform item in EndingCakeChildren)
        {
            item.parent = EndingCakeHolder;
        }

        EndingCakeHolder.parent = EndingSequenceCakeHolder;

        EndingCakeHolder.localScale = Vector3.one;
        EndingCakeHolder.localPosition = Vector3.zero;
    }
}
