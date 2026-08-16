using System;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
 
// Example script showing how to invoke the Google Mobile Ads Unity plugin.
public class AdmobIntilization : MonoBehaviour
{


    static InterstitialAd interstitial;
    static RewardedAd rewardedAd;
    public bool isTestIdOn;
    string test_interstitialID = "ca-app-pub-3940256099942544/1033173712";
    string test_rewardID = "ca-app-pub-3940256099942544/5224354917";

    public string interstitialID = "ca-app-pub-3940256099942544/1033173712";
    public string rewardID = "ca-app-pub-3940256099942544/5224354917";


    public AdSize adSize;
    public AdPosition adPosition;


    public static AdmobIntilization _instance;
    public GameObject callBackObject;
    public AppOpen_Code _ap;
    public Rewarded _rewarded;

    private Action<bool> Callback
    {
        get;
        set;
    }
    private void Awake()
    {
        if(_instance==null)
            _instance = this;
    }
    void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus =>
        {
            if (PlayerPrefs.GetInt("RemoveAds") == 0)
            {
                _ap.CallingAppOpen();

                Invoke(nameof(RequestInterstitial), 2f);
                Invoke(nameof(RequesRewardAd), 3f);
            }
        });
    }

    // Event handler for the AdFailedToLoad event

    AdRequest interstitialrequest;
    public void RequestInterstitial()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(isTestIdOn == false ? interstitialID : test_interstitialID, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitial = ad;
                InterstitialRegisterEventHandlers(interstitial);
            });
    }
    private void InterstitialRegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        //interstitialAd.OnAdPaid += (AdValue adValue) =>
        //{
        //    Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
        //        adValue.Value,
        //        adValue.CurrencyCode));
        //    var dic = new System.Collections.Generic.Dictionary<string, string>();
        //    dic["ad_platform"] = "AdMob";
        //    dic["value"] = ((adValue.Value) / 1000000f).ToString();
        //    dic["currency"] = "USD";

        //    AppsFlyerAdRevenue.logAdRevenue("AdMob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, adValue.Value / 1000000f, "USD", dic);
        //};
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            RequestInterstitial();
        };
       
    }
    public void ShowInterstialAd()
    {
        interstitial.Show();
        AdsManager.instance.isPausedDuetoAd = true;

    }
    public bool HasAdmobInterstialAvaible()
    {
        if (!interstitial.CanShowAd())
        {
            RequestInterstitial();
        }
        return interstitial.CanShowAd();
    }

    #region Interstitial callback handlers

    #endregion


    static AdRequest requestReward;
    public void RequesRewardAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(isTestIdOn == false ? rewardID : test_rewardID, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                rewardedAd = ad;
            });
    }
    private void RewardedRegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        //ad.OnAdPaid += (AdValue adValue) =>
        //{
        //    Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
        //        adValue.Value,
        //        adValue.CurrencyCode));
        //    var dic = new System.Collections.Generic.Dictionary<string, string>();
        //    dic["ad_platform"] = "AdMob";
        //    dic["value"] = ((adValue.Value) / 1000000f).ToString();
        //    dic["currency"] = "USD";

        //    AppsFlyerAdRevenue.logAdRevenue("AdMob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, adValue.Value / 1000000f, "USD", dic);
        //};
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            DelayRequest();
        };
       
    }
    public void ShowRewardAd(Rewarded rewarded)
    {
  
        if (rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                _rewarded = rewarded;
                _rewarded();
                Invoke("DelayRequest", 0.1f);
                Debug.Log("This is admob rewards");
            });
            AdsManager.instance.isPausedDuetoAd = true;

        }
        else
        {
            RequesRewardAd();

        }
    }

    public bool HasRewardedAvaiable()
    {
        if (!rewardedAd.CanShowAd())
        {
            RequesRewardAd();
        }
        return rewardedAd.CanShowAd();
    }
    #region RewardedAd callback handlers
 
    void DelayRequest()
    {
        RequesRewardAd();
    }
    #endregion

    //for destroy
  
}
