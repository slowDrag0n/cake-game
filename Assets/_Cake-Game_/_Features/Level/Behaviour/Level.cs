using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    [Header("TEST MODE")]
    public bool TestMode;
    public int Test_StartIndex;
    [Space]

    public LevelSequence[] SequencesInLevel;

    [Header("Level Completion")]
    public float WinDelay = 1f;

    [Header("Level Vfx")]
    public GameObject SequenceCompletionVfx;
    public GameObject LevelWinVfx;

    [Header("Level Sfx")]
    [Range(0.000f, 1.000f)]
    public float SfxVolume = 1f;

    int _sequenceIndex = -1;

    private void Start()
    {
        foreach(Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            if(canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        _sequenceIndex = -1;

        if(TestMode)
            _sequenceIndex = Test_StartIndex;

        StartNextSequence();
    }

    public void StartNextSequence()
    {
        _sequenceIndex += 1;

        if(_sequenceIndex == SequencesInLevel.Length)
        {
            EventManager.DoFireWinLevel();
            return;
        }

        if(_sequenceIndex > 0)
            SequencesInLevel[_sequenceIndex - 1].gameObject.SetActive(false);

        SequencesInLevel[_sequenceIndex].gameObject.SetActive(true);

    }

    public void SpawnCompletionVfx()
    {
        Destroy(Instantiate(SequenceCompletionVfx, transform), 2f);
    }

    public void SpawnLevelWinVfx()
    {
        Instantiate(LevelWinVfx, transform);
    }

    public void PlaySound(int soundIndex)
    {
        SoundController.Instance.PlaySound((SoundType)soundIndex);
    }
}
