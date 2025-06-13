using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScreen : UiPanel
{
    public Button CloseBtn;

    private void OnEnable()
    {
        //AdsManager.Ins.HideBannerAd();
        //AdsManager.Ins.ShowBigBannerAd();

        CloseBtn.onClick.AddListener(delegate
        {
            EventManager.DoFireHideUiEvent(UiType.Setting);
            SoundController.Instance.PlaySound(SoundType.Click);

            //AdsManager.Ins.HideBigBannerAd();
            //AdsManager.Ins.ShowBannerAd(BannerType.Banner);
        });
    }

    private void OnDisable()
    {
        CloseBtn.onClick.RemoveAllListeners();
    }
}
