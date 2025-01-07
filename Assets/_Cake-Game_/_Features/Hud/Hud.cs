using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Hud : UiPanel
{
    public Button BackBtn;
    public Button SettingBtn;
    public Button SkipBtn;
    public Button HintBtn;
    public TextMeshProUGUI LevelText;

    int _lv;

    public override void Show(object uiData = null)
    {
        base.Show(uiData);

        _lv = (int)uiData;
        LevelText.text = "Level " + (_lv + 1).ToString();
    }

    private void OnEnable()
    {
        BackBtn.onClick.AddListener(OnClick_BackFromGameplayBtn);
        SettingBtn.onClick.AddListener(OnClick_SettingBtn);
        SkipBtn.onClick.AddListener(OnClick_SkipBtn);
        //RetryBtn.onClick.AddListener(OnClick_RetryBtn);
        HintBtn.onClick.AddListener(OnClick_HintBtn);
    }

    private void OnDisable()
    {
        BackBtn.onClick.RemoveListener(OnClick_BackFromGameplayBtn);
        SettingBtn.onClick.RemoveListener(OnClick_SettingBtn);
        SkipBtn.onClick.RemoveListener(OnClick_SkipBtn);
        //RetryBtn.onClick.RemoveListener(OnClick_RetryBtn);
        HintBtn.onClick.RemoveListener(OnClick_HintBtn);
    }

    private void OnClick_BackFromGameplayBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        EventManager.DoFireStopGameLevel();
        EventManager.DoFireShowUiEvent(UiType.LevelSelection, Profile.Level);
    }

    private void OnClick_SettingBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        EventManager.DoFireShowUiEvent(UiType.Setting);
    }

    private void OnClick_SkipBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        //AdsManager.Ins.ShowRewardAd(() =>
        //{
        //update level and close ui
        int lv = Profile.Level + 1;
        Profile.Level = lv;
        EventManager.DoFireStartGame(lv);

        EventManager.DoFireHideUiEvent(UiType.LevelFail);
        //});
    }

    private void OnClick_RetryBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        EventManager.DoFireStartGame(Profile.Level);

        //AnalyticsManager.Instance.LogEvent("level_retry", "hud_retry", Profile.Level.ToString());
        //AdsManager.Ins.ShowInterstitialAd();
    }

    private void OnClick_HintBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        //AdsManager.Ins.ShowRewardAd(() =>
        //{
        SoundController.Instance.PlaySound(SoundType.Hint);
        EventManager.DoFireShowLevelTutorial();
        //});
    }
}