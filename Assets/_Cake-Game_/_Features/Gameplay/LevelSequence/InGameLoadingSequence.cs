using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameLoadingSequence : LevelSequence
{
    protected override void Start()
    {
        var canvas = GetComponentInChildren<Canvas>(true);
        canvas.worldCamera = Camera.main;
    }

    private void OnEnable()
    {
        BigBanner.instance.bannerBigBannerShow();
        //AdsManager.Ins.ShowBigBannerAd(GoogleMobileAds.Api.AdPosition.Top);
    }

    private void OnDisable()
    {
        BigBanner.instance.bannerBigBannerHide();
        //AdsManager.Ins.HideBigBannerAd();
    }
}
