using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailPanel : UiPanel
{
    public TextMeshProUGUI RewardCoinText;
    public TextMeshProUGUI RewardGemText;
    public RewardMultiplier MultiplierPanel;
    public Image LevelFailImage;
    public Button SkipBtn;
    public Button RetryBtn;
    public Button BackBtn;

    Sprite _failSrpite;

    private void OnEnable()
    {
        RetryBtn.onClick.AddListener(OnClicked_RetryBtn);
        BackBtn.onClick.AddListener(OnClicked_BackBtn);
        SkipBtn.onClick.AddListener(OnClicked_SkipBtn);
        MultiplierPanel.ClaimWithAdButton.onClick.AddListener(OnClicked_RewardAdMultiplier);
    }

    private void OnDisable()
    {
        RetryBtn.onClick.RemoveListener(OnClicked_RetryBtn);
        BackBtn.onClick.RemoveListener(OnClicked_BackBtn);
        SkipBtn.onClick.RemoveListener(OnClicked_SkipBtn);
        MultiplierPanel.ClaimWithAdButton.onClick.RemoveListener(OnClicked_RewardAdMultiplier);
    }

    public override void Show(object uiData = null)
    {
        base.Show(uiData);

        SoundController.Instance.PlaySound(SoundType.Fail);

        _failSrpite = (Sprite)uiData;
        LevelFailImage.sprite = _failSrpite;


        //AdsManager.Ins.HideBannerAd();
        //AdsManager.Ins.ShowBigBannerAd(GoogleMobileAds.Api.AdPosition.Center);
    }

    void OnClicked_RetryBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        MultiplierPanel.ClaimWithAdButton.onClick.RemoveAllListeners();
        MultiplierPanel.Stop();

        // update level and close ui
        int lv = Profile.Level;
        EventManager.DoFireStartGame(lv);

        EventManager.DoFireHideUiEvent(UiType.LevelFail);

        //AdsManager.Ins.HideBigBannerAd();
        //AdsManager.Ins.ShowBannerAd(BannerType.Banner);

        //AnalyticsManager.Instance.LogEvent("level_retry", "fail_retry", Profile.Level.ToString());

        //if(Profile.LevelsFinishedCounter % 3 == 0)
        //    AdsManager.Ins.ShowInterstitialAd();
    }

    void OnClicked_BackBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        EventManager.DoFireShowUiEvent(UiType.LevelSelection);

        //AdsManager.Ins.HideBigBannerAd();
    }

    void OnClicked_SkipBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        //AdsManager.Ins.HideBigBannerAd();
        //AdsManager.Ins.ShowBannerAd(BannerType.Banner);

        //AdsManager.Ins.ShowRewardAd(() =>
        //{
        //update level and close ui
        int lv = Profile.Level + 1;
        Profile.Level = lv;
        EventManager.DoFireStartGame(lv);

        EventManager.DoFireHideUiEvent(UiType.LevelFail);
        //});
    }

    void OnClicked_RewardAdMultiplier()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        //AdsManager.Ins.ShowRewardAd(() =>
        //{
        //    MultiplierPanel.ClaimWithAdButton.onClick.RemoveAllListeners();
        //    MultiplierPanel.Stop();

        //    float rewardToGive = _earnedCoinsGems.Item1 * MultiplierPanel.CurrentMultiplier;
        //    uint r = (uint)rewardToGive;

        //    Profile.Cash -= _earnedCoinsGems.Item1;
        //    Profile.Cash += r;

        //    Debug.Log("Reward To Give=" + rewardToGive);

        //    // update level and close ui
        //    int lv = Profile.Level;
        //    EventManager.DoFireStartLevel(lv);

        //    EventManager.DoFireHideUiEvent(UiType.LevelFail);
        //});
    }
}
