
using AdjustSdk;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public delegate void Rewarded();

public class AdsManager : MonoBehaviour
{
    public bool isPausedDuetoAd = false;
    public static AdsManager instance = null;

    [SerializeField] private string maxSdkKey = "";
#if UNITY_IPHONE || UNITY_IOS
    //[SerializeField] string BannerAdUnitId_IOS = "";
    [SerializeField] string InterstitialAdUnitId_IOS = "";
    [SerializeField] string RewardedAdUnitId_IOS = "";
    [Space(50)]
#else
    //[SerializeField] string BannerAdUnitId_Android = "";
    [SerializeField] string InterstitialAdUnitId_Android = "";
    [SerializeField] string RewardedAdUnitId_Android = "";
    [SerializeField] string BannerAdUnitId = "";

#endif
    [SerializeField]
    private MaxSdk.AdViewPosition bannerPosition = MaxSdk.AdViewPosition.BottomCenter;    //public static string BannerAdUnitId = "";
    public static string InterstitialAdUnitId = "";
    public static string RewardedAdUnitId = "";
    private int interRequestTime = 0;
    private int rewardRequestTime = 0;
    public Rewarded _rewarded;

    public bool isBannerShowing;
    private Action<bool> Callback
    {
        get;
        set;
    }
    public static bool bannerShowing = false;
    public bool IsVideoLoaded
    {
        get
        {
            if(!isMaxInitialized || !MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
                return false;
            else
                return true;
        }
    }
    private bool isMaxInitialized = false;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        Invoke(nameof(LoadAds), 0.01f);

    }

    private void LoadAds()
    {
        if(PlayerPrefs.GetInt("NoAds") == 1) return;
        if(PlayerPrefs.GetInt("MaxAdStop") == 1) return;

        //AdSettings.SetDataProcessingOptions(new string[] { "LDU" }, 1, 1000);
#if UNITY_IPHONE || UNITY_IOS
        BannerAdUnitId = BannerAdUnitId_IOS;
        InterstitialAdUnitId = InterstitialAdUnitId_IOS;
        RewardedAdUnitId = RewardedAdUnitId_IOS;
        mrecAdUnitId = MRecAdUnitId_IOS;
#else
        //BannerAdUnitId = BannerAdUnitId_Android;
        InterstitialAdUnitId = InterstitialAdUnitId_Android;
        RewardedAdUnitId = RewardedAdUnitId_Android;
#endif
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            // AppLovin SDK is initialized, start loading ads
            Debug.Log("MAX SDK Initialized");
            isMaxInitialized = true;
            if(PlayerPrefs.GetInt("RemoveAds") == 0)
            {
                InitializeInterstitialAds();
                InitializeBannerAds();
                Invoke(nameof(ShowBanner), 1);


            }
            InitializeRewardedAds();
        };
        MaxSdk.InitializeSdk();

    }




    public void SelfDestroy()
    {
        instance = null;
        Destroy(this.gameObject);
    }
    public void RequestInter()
    {
        if(!isMaxInitialized)
            return;

        if(PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            if(!MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
                LoadMaxInterstitial();
        }
    }
    public void RequestVideo()
    {
        if(!isMaxInitialized)
            return;
        if(!MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
            LoadRewardedAd();
    }
    public void MuteAudio()
    {
        MaxSdk.SetMuted(PlayerPrefs.GetInt("Audio", 1) == 0);
    }



    #region Interstitial Ad Methods
    public void InitializeInterstitialAds()
    {
        if(!isMaxInitialized)
            return;
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == InterstitialAdUnitId)
            {
                TrackRevenue(info, "interstitial");
            }
        };

        // Load the first interstitial
        LoadMaxInterstitial();
    }
    private void LoadMaxInterstitial()
    {
        if(!isMaxInitialized)
            return;
        Debug.Log("Interstitial loading start=>");
        MuteAudio();
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }
    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(adUnitId) will now return 'true'
        Debug.Log("Max Interstitial ad loaded");
        interRequestTime = 0;
    }
    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
        print("Interstitial FailedToReceiveAd=>" + errorInfo);
        if(interRequestTime >= 3)
            return;
        interRequestTime += 1;
        Invoke(nameof(LoadMaxInterstitial), 5f);
        Time.timeScale = 1;
    }
    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        if(interRequestTime >= 3)
            return;
        interRequestTime += 1;
        Invoke(nameof(LoadMaxInterstitial), 5f);
        Time.timeScale = 1;
    }
    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        Debug.Log("Interstitial ad is dismissed=>");
        Invoke(nameof(LoadMaxInterstitial), 0.5f);
        Time.timeScale = 1;
        AdCaller.ins.resetTime();

    }

    private void AdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        double revenue = impressionData.Revenue;
        var impressionParameters = new[] {
        new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
        new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
        new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
        new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
        new Firebase.Analytics.Parameter("value", revenue),
        new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);

        //var impressionAppsflyerParameters = new Dictionary<string, string>();
        //impressionAppsflyerParameters["ad_platform"] = "AppLovin";
        //impressionAppsflyerParameters["ad_source"] = impressionData.NetworkName;
        //impressionAppsflyerParameters["ad_unit_name"] = impressionData.AdUnitIdentifier;
        //impressionAppsflyerParameters["ad_format"] = impressionData.AdFormat;
        //impressionAppsflyerParameters["value"] = revenue.ToString();
        //impressionAppsflyerParameters["currency"] = "USD";

        //AppsFlyer.sendEvent("Ads_Impression", impressionAppsflyerParameters);
        //AppsFlyerAdRevenue.logAdRevenue(
        //    "AppLovin",
        //    AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
        //    revenue,
        //    "USD",
        //    impressionAppsflyerParameters
        //    );
    }

    private static void TrackRevenue(MaxSdkBase.AdInfo info, string placement)
    {
        double revenue = info.Revenue;
        if(revenue <= 0) return;
        var adj = new AdjustAdRevenue("applovin_max_sdk");
        adj.SetRevenue(revenue, "USD");
        adj.AdRevenueNetwork = info.NetworkName;
        adj.AdRevenuePlacement = placement;
        Adjust.TrackAdRevenue(adj);
    }

    public void ShowInterstitial()
    {
        if(PlayerPrefs.GetInt("MaxAdStop") == 1) return;

        if(PlayerPrefs.GetInt("RemoveAds") == 1 || !isMaxInitialized)
        {
            return;
        }
        else
        {

            if(MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
            {
                Debug.Log("Showing interstitial");
                isPausedDuetoAd = true;
                MaxSdk.ShowInterstitial(InterstitialAdUnitId);

            }
            else
            {
                Debug.Log("Interstitial is not loaded");
                LoadMaxInterstitial();
                if(AdmobIntilization.Instance.HasAdmobInterstialAvaible())
                {
                    isPausedDuetoAd = true;
                    AdmobIntilization.Instance.ShowInterstialAd();
                    Debug.Log("AdmobAd");
                }
            }
        }
    }
    public bool isMaxReady()
    {
        if(MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            return true;
        }
        return false;
    }
    public void ShowMaxInterstitial()
    {
        if(PlayerPrefs.GetInt("MaxAdStop") == 1) return;

        if(MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            Debug.Log("Showing interstitial");
            isPausedDuetoAd = true;
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
        }

    }


    #endregion

    #region Rewarded Ad Methods
    private void InitializeRewardedAds()
    {
        if(!isMaxInitialized)
            return;
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == RewardedAdUnitId)
            {
                TrackRevenue(info, "rewarded");
            }
        };

        // Load the first rewarded ad
        LoadRewardedAd();
    }
    private void LoadRewardedAd()
    {
        Debug.Log("Rewarded loading start=>");
        MuteAudio();
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }
    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        print("RewardedAd Loaded event");
        rewardRequestTime = 0;
    }
    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load 
        // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
        print("Rewarded Failed ToReceive with error code" + errorInfo);
        if(rewardRequestTime >= 3)
            return;
        rewardRequestTime += 1;
        Invoke(nameof(LoadRewardedAd), 5f);
    }
    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        print("RewardedAd FailedToShow with error code" + errorInfo);
    }



    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        print("RewardedAd hidden event received");
        Callback?.Invoke(false);
        Callback = null;
        LoadRewardedAd();
        AdCaller.ins.resetTime();

    }
    private void OnRewardedAdDismissedEvent(string adUnitId)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        print("RewardedAd Dismissed event received");
        Callback?.Invoke(false);
        Callback = null;
        LoadRewardedAd();
        AdCaller.ins.resetTime();

    }
    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        print("Video event for EarnedReward=>");
        _rewarded();
        rewardRequestTime = 0;
        LoadRewardedAd();
        AdCaller.ins.resetTime();

    }
    public void ShowRewardedAd(Rewarded rewarded)
    {
        if(!isMaxInitialized)
            return;

        _rewarded = rewarded;
        if(MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            isPausedDuetoAd = true;
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else
        {
            LoadRewardedAd();
            AdmobIntilization.Instance.ShowRewardAd(rewarded);
            isPausedDuetoAd = true;
        }


    }
    public void ShowRewardedAdMax(Rewarded rewarded)
    {
        if(!isMaxInitialized)
            return;

        _rewarded = rewarded;
        if(MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            isPausedDuetoAd = true;
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }



    }
    #endregion

    #region Banner Ad Methods

    // Retrieve the ID from your account
    #region Banner Ad Methods

    public void InitializeBannerAds()
    {
        if(!isMaxInitialized)
            return;

        Debug.Log("Initializing Banner...");
        Debug.Log("Banner Ad Unit : " + BannerAdUnitId);

        // Register callbacks BEFORE creating banner
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == BannerAdUnitId)
            {
                TrackRevenue(info, "banner");
            }
        };


        var bannerConfig = new MaxSdk.AdViewConfiguration(bannerPosition);

        MaxSdk.CreateBanner(BannerAdUnitId, bannerConfig);
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.clear);

        // Load & Show
        MaxSdk.ShowBanner(BannerAdUnitId);

        Debug.Log("Banner Created");
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("===== MAX Banner Loaded =====");

        isBannerShowing = true;

        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isBannerShowing = false;

        Debug.LogError($"Banner Failed\nCode : {errorInfo.Code}\nMessage : {errorInfo.Message}");

        Invoke(nameof(RetryBanner), 5f);
    }

    private void RetryBanner()
    {
        Debug.Log("Retry Banner");

        MaxSdk.DestroyBanner(BannerAdUnitId);

        var bannerConfig = new MaxSdk.AdViewConfiguration(bannerPosition);

        MaxSdk.CreateBanner(BannerAdUnitId, bannerConfig);

        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner Clicked");
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner Expanded");
    }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner Collapsed");
    }

    public void ShowBanner()
    {
        if(!isMaxInitialized)
            return;

        if(PlayerPrefs.GetInt("RemoveAds") == 1)
            return;

        if(!FirebaseHandler.isBannerOn)
            return;

        Debug.Log("Show Banner");
        if(FirebaseHandler.isBannerOn == false)
        {
            return;
        }
        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    public void HideBanner()
    {
        if(!isMaxInitialized)
            return;

        Debug.Log("Hide Banner");

        MaxSdk.HideBanner(BannerAdUnitId);
    }

    #endregion

    #endregion



}