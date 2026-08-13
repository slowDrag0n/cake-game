using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartSequence : LevelSequence
{
    public Image CharImage;

    protected override void Start()
    {
        base.Start();
        LevelCharacter levelChar = EventManager.DoFireGetLevelCharacter();
        CharImage.sprite = levelChar.LevelStartPose;
    }
}
