using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds;
using GoogleMobileAds.Common;
using System;
using System.Diagnostics;

public class BigBanner : MonoBehaviour
{
    // Start is called before the first frame update
    public AdPosition adsPosition;
    
    public static BigBanner instance;
    private void Awake()
    {
        instance = this;

    }
    void Start()
    {
       
        RequestBanner();
    }
    static BannerView bannerView;
    string test_bannerID = "ca-app-pub-3940256099942544/6300978111";
    public string bannerID = "ca-app-pub-3940256099942544/6300978111";
    public void RequestBanner()
    {

        if (PlayerPrefs.GetInt("AdsStop") == 1) return;
        if (PlayerPrefs.GetInt("RemoveAds") == 1) return;
        // Create a 320x50 banner at the top of the screen.
        if (bannerView != null)
        {
            bannerView.Destroy();
        }
        //AdSize adSize = new AdSize(250, 250);
        bannerView = new BannerView(AdmobIntilization._instance.isTestIdOn==false?bannerID:test_bannerID, AdSize.MediumRectangle, adsPosition);
        ListenToAdEvents(bannerView);
        var adRequest = new AdRequest();

        // send the request to load the ad.
      
        bannerView.LoadAd(adRequest);
        bannerView.Hide();
    }
    private void ListenToAdEvents(BannerView _bannerView)
    {
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            AdsManager.instance.isPausedDuetoAd = true;
        };
     
    }
    public void bannerBigBannerShow()
    {
        if (FirebaseHandler.isMRecOn == false)
            return;
        if (PlayerPrefs.GetInt("AdsStop") == 1) return;
        if (PlayerPrefs.GetInt("RemoveAds") == 1) return;

        if (bannerView != null)
        {
            bannerView.Show();
            AdsManager.instance.HideBanner();
        }
        else
        {
            RequestBanner();
        }

    }
    public void bannerBigBannerHide()
    {
        if (FirebaseHandler.isMRecOn == false)
            return;
        if (PlayerPrefs.GetInt("AdsStop") == 1) return;
        if (PlayerPrefs.GetInt("RemoveAds") == 1) return;

        bannerView.Hide();
        AdsManager.instance.ShowBanner();

    }
    
}
