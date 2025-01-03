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

    public void SpawnCompletionVfx()
    {
        Destroy(Instantiate(SequenceCompletionVfx, transform), 2f);
    }
    public void SpawnLevelWinVfx()
    {
        Instantiate(LevelWinVfx, transform);
    }
}
