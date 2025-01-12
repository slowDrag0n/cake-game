using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameLoadingSequence : LevelSequence
{
    private void Start()
    {
        AdsManager.Ins.HideBannerAd();
        AdsManager.Ins.ShowBigBannerAd(GoogleMobileAds.Api.AdPosition.Center);
    }

    private void OnDisable()
    {
        AdsManager.Ins.HideBigBannerAd();
        AdsManager.Ins.ShowBannerAd(BannerType.Banner);
    }
}
