using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEndingSequence : LevelSequence
{
    public Transform EndingCakeHolder;
    public Transform[] EndingCakeChildren;
    public Transform EndingSequenceCakeHolder;

    public Image CharImage;

    private void Start()
    {
        LevelCharacter levelChar = EventManager.DoFireGetLevelCharacter();
        //CharImage.sprite = levelChar.LevelEndPose;

        foreach(Transform item in EndingCakeChildren)
        {
            item.parent = EndingCakeHolder;
        }

        EndingCakeHolder.parent = EndingSequenceCakeHolder;

        EndingCakeHolder.localScale = Vector3.one;
        EndingCakeHolder.localPosition = Vector3.zero;

        var canvas = GetComponentInChildren<Canvas>(true);
        canvas.worldCamera = Camera.main;

        foreach(AudioSource src in EndingCakeHolder.GetComponentsInChildren<AudioSource>(true))
        {
            src.Stop();
        }
    }
}
