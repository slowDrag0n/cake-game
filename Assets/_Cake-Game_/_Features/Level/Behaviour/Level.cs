using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    [Header("TEST MODE")]
    public bool TestMode;
    public int Test_StartIndex;
    [Space]
    public string LevelName = "Level";
    public LevelSequence[] SequencesInLevel;

    [Header("Level Cake")]
    public Image CakeStartingImage;
    public SpriteRenderer CakeHudImage;
    public Sprite[] LevelCakeSprites;

    [Header("Level Completion")]
    public float WinDelay = 1f;

    [Header("Level Vfx")]
    public GameObject SequenceCompletionVfx;
    public GameObject LevelWinVfx;

    [Header("Level Sfx")]
    [Range(0.000f, 1.000f)]
    public float SfxVolume = 1f;

    [Header("Level UI")]
    public TextMeshProUGUI StartBtnText;

    int _sequenceIndex = -1;
    int _selectedCake;

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

        StartBtnText.text = "Day " + (Profile.Level + 1).ToString();

        //foreach(var seq in SequencesInLevel)
        //    seq.gameObject.SetActive(false);

        AssignLevelCake();

        StartNextSequence();
    }

    private void AssignLevelCake()
    {
        if(Profile.Level < 2)
            return;

        _selectedCake = UnityEngine.Random.Range(0, LevelCakeSprites.Length);
        var selectedCakeSprite = LevelCakeSprites[_selectedCake];
        CakeStartingImage.sprite = selectedCakeSprite;
        CakeHudImage.sprite = selectedCakeSprite;
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
        var vfx = Instantiate(SequenceCompletionVfx, transform);
        var vfxSrc = vfx.GetComponent<AudioSource>();
        vfxSrc.mute = !Profile.SoundEnabled;
        Destroy(vfx, 2f);
    }

    public void SpawnLevelWinVfx()
    {
        var vfx = Instantiate(LevelWinVfx, transform);
        var vfxSrc = vfx.GetComponent<AudioSource>();
        vfxSrc.mute = !Profile.SoundEnabled;
    }

    public void PlaySound(int soundIndex)
    {
        SoundController.Instance?.PlaySound((SoundType)soundIndex);
    }

    public void UpdateAudioMuteStatus()
    {
        foreach(AudioSource src in GetComponentsInChildren<AudioSource>(true))
        {
            src.mute = !Profile.SoundEnabled;
            Debug.Log(src.gameObject.name + " MUTE = " + src.mute);
        }
    }

    public void ShowBlankScreen()
    {
        BlankScreen.FadeIn();
    }

    public void HideBlankScreen()
    {
        BlankScreen.FadeOut();
    }

    public void RunQuickAd()
    {
        // TODO - Interstitial ad here...
        AdCaller.ins.AdBreak();

        //AdsManager.Ins.QuickAd.RunQuickAd();
    }
}

[Serializable]
public class LevelCharacter
{
    public Sprite LevelStartPose;
    public Sprite LevelEndPose;
}