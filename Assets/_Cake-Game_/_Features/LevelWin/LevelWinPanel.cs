using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelWinPanel : UiPanel
{
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI RewardCoinText;
    public TextMeshProUGUI RewardGemText;
    public RewardMultiplier MultiplierPanel;
    public Image LevelWinImage;
    public Button ContinueBtn;
    public Button RetryBtn;
    public Button BackBtn;

    Sprite _winSprite;

    private void OnEnable()
    {
        ContinueBtn.onClick.AddListener(OnClicked_ContinueBtn);
        RetryBtn.onClick.AddListener(OnClicked_RetryBtn);
        BackBtn.onClick.AddListener(OnClicked_BackBtn);
        MultiplierPanel.ClaimWithAdButton.onClick.AddListener(OnClicked_RewardAdMultiplier);
    }

    private void OnDisable()
    {
        ContinueBtn.onClick.RemoveListener(OnClicked_ContinueBtn);
        RetryBtn.onClick.RemoveListener(OnClicked_RetryBtn);
        BackBtn.onClick.RemoveListener(OnClicked_BackBtn);
        MultiplierPanel.ClaimWithAdButton.onClick.RemoveListener(OnClicked_RewardAdMultiplier);
    }

    public override void Show(object uiData = null)
    {
        base.Show(uiData);

        //SoundController.Instance.PlaySound(SoundType.Win);

        LevelText.text = "Level " + (Profile.Level + 1).ToString();

        _winSprite = (Sprite)uiData;
        LevelWinImage.sprite = _winSprite;

        //AdsManager.Ins.HideBannerAd();
        //AdsManager.Ins.ShowBigBannerAd(GoogleMobileAds.Api.AdPosition.Center);
    }

    void OnClicked_ContinueBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        MultiplierPanel.ClaimWithAdButton.onClick.RemoveAllListeners();
        MultiplierPanel.Stop();

        // update level and close ui
        int lv = Profile.Level + 1;
        Profile.Level = lv;
        EventManager.DoFireStartGame(lv);

        EventManager.DoFireHideUiEvent(UiType.LevelWin);

        //AdsManager.Ins.HideBigBannerAd();
        //AdsManager.Ins.ShowBannerAd(BannerType.Banner);

        //if(Profile.LevelsFinishedCounter % 3 == 0)
        //    AdsManager.Ins.ShowInterstitialAd();
    }

    void OnClicked_RetryBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        MultiplierPanel.ClaimWithAdButton.onClick.RemoveAllListeners();
        MultiplierPanel.Stop();

        // update level and close ui
        int lv = Profile.Level;
        Profile.Level = lv;
        EventManager.DoFireStartGame(lv);

        EventManager.DoFireHideUiEvent(UiType.LevelWin);

        //AdsManager.Ins.HideBigBannerAd();
        //AdsManager.Ins.ShowBannerAd(BannerType.Banner);

        //if(Profile.LevelsFinishedCounter % 3 == 0)
        //    AdsManager.Ins.ShowInterstitialAd();
    }

    void OnClicked_BackBtn()
    {
        SoundController.Instance.PlaySound(SoundType.Click);

        EventManager.DoFireShowUiEvent(UiType.LevelSelection);

        //AdsManager.Ins.HideBigBannerAd();
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
        //    int lv = Profile.Level + 1;
        //    Profile.Level = lv;
        //    EventManager.DoFireStartLevel(lv);

        //    EventManager.DoFireHideUiEvent(UiType.LevelWin);
        //});
    }
}
