using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScreen : UiPanel
{
    public Button CloseBtn;

    private void OnEnable()
    {
        BigBanner.instance.bannerBigBannerShow();
        //AdsManager.Ins.ShowBigBannerAd(GoogleMobileAds.Api.AdPosition.Top);

        CloseBtn.onClick.AddListener(delegate
        {
            EventManager.DoFireHideUiEvent(UiType.Setting);
            SoundController.Instance.PlaySound(SoundType.Click);
        });
    }

    private void OnDisable()
    {
        BigBanner.instance.bannerBigBannerHide();

        //AdsManager.Ins.HideBigBannerAd();
        CloseBtn.onClick.RemoveAllListeners();
    }
}
