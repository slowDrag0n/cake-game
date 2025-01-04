using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public LevelSequence[] SequencesInLevel;

    [Header("Level Vfx")]
    public GameObject SequenceCompletionVfx;
    public GameObject LevelWinVfx;

    private void Start()
    {
        foreach(Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            if(canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }

    public void SpawnCompletionVfx()
    {
        Destroy(Instantiate(SequenceCompletionVfx, transform), 2f);
    }
    public void SpawnLevelWinVfx()
    {
        Instantiate(LevelWinVfx, transform);
    }
}
