using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;
using System;
using GoogleMobileAds.Ump.Api;
using System.Collections.Generic;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Ins;
    public QuickAdBreak QuickAd { get; set; }

    public float FirstAdTimeThreshold = 45;

    public AdPlacement adUnits;

    private BannerView      _bannerAd;
    private BannerView      _bigBannerAd;
    private InterstitialAd  _interAd;
    private RewardedAd      _rewardAd;
    private AppOpenAd       _appOpenAd;

    [SerializeField] private float minMemory = 1024;

    private UnityAction _intAdCallback;

    private bool _adUnitsLoaded;
    private DateTime _expireTime;

    public UnityEvent OnConsentGathered;
    public UnityEvent OnIntersititialFailed;
    public UnityEvent OnRewardClosed;

    public bool IsInAd { get; private set; }

    bool _firstAdTimeThresholdReached { get { return Time.time >= FirstAdTimeThreshold; } }
    bool _bannerLoadStatus;
    bool _bigBannerLoadStatus;

    private void Awake()
    {
        Singleton();
        QuickAd = GetComponent<QuickAdBreak>();
        //AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void Start()
    {
        InitAds();
        StartCoroutine(ShowBannerAfterDelayCo());
    }

    IEnumerator ShowBannerAfterDelayCo()
    {
        yield return new WaitForSecondsRealtime(FirstAdTimeThreshold);
        ShowBannerAd(BannerType.Banner, AdPosition.Bottom);
    }

    private void Singleton()
    {
        if(Ins == null)
        {
            Ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator GetData(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
        {
            adUnits = JsonUtility.FromJson<AdPlacement>(request.downloadHandler.text);
            _adUnitsLoaded = true;
#if UNITY_EDITOR
            DebugMsg("<color=green>[AdmobServer] Ad units loaded successfully from server.</color>");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("<color=red>[AdmobServer] Error: " + request.error);
#endif
        }
        request.Dispose();

        InitAds();
    }

    private void InitAds()
    {
#if UNITY_EDITOR
        //debugging ad
        string color = "cyan";
        Debug.Log("<color=" + color + "><b>Banner:          </b>" + adUnits.bannerId + "</color>");
        Debug.Log("<color=" + color + "><b>Interstitial:    </b>" + adUnits.intId + "</color>");
        Debug.Log("<color=" + color + "><b>Reward:          </b>" + adUnits.rewardId + "</color>");
#endif

        // When true all events raised by GoogleMobileAds will be raised
        // on the Unity main thread. The default value is false.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        ConsentHandling();
    }

    private void ConsentHandling()
    {

        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
                {
                    "C838E2E602EB92B5C19F101E8E2C2AFB"
                }
        };

        // Create a ConsentRequestParameters object.
        ConsentRequestParameters request = new ConsentRequestParameters()
        {
            ConsentDebugSettings = debugSettings,
        };

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        if(consentError != null)
        {
            // Handle the error.
            Debug.LogError(consentError);
            InitAdsSdk();
            OnConsentGathered?.Invoke();
            return;
        }

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if(formError != null)
            {
                // Consent gathering failed.
                Debug.LogError("Consent gathering err: " + consentError);
                InitAdsSdk();
                OnConsentGathered?.Invoke();
                return;
            }

            // Consent has been gathered.
            InitAdsSdk();

            OnConsentGathered?.Invoke();

        });
    }

    private void InitAdsSdk()
    {
        if(CanShowAds)
        {
            RequestConfiguration requestConfiguration = new RequestConfiguration.Builder()
              .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
              .build();

            MobileAds.SetRequestConfiguration(requestConfiguration);

            MobileAds.Initialize(initStatus =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    LoadAds();
                });
            });
        }
    }

    private void LoadAds()
    {
        DebugMsg("LoadAds");
        if(!string.IsNullOrEmpty(adUnits.appOpenId))
        {
            LoadAppOpenAd();
        }
        if(!string.IsNullOrEmpty(adUnits.intId))
        {
            Invoke(nameof(LoadInterstitialAd), 0.25f);
        }
        if(!string.IsNullOrEmpty(adUnits.rewardId))
        {
            Invoke(nameof(LoadRewardAd), 0.5f);
        }
    }

    private AdRequest CreateAdRequest()
    {
        if(CanShowAds)
        {
            return new AdRequest.Builder()
           .Build();
        }
        else
        {
            return null;
        }
    }

    private void OnDestroy()
    {
        DestroyBannerAd();
        DestroyInterstitialAd();
        DestroyRewardAd();

        // Always unlisten to events when complete.
        //AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }



    #region App Open -------------------------------------------------------------------------------

    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : " + state);

        // if the app is Foregrounded and the ad is available, show it.
        if(state == AppState.Foreground)
        {
            if(IsAdAvailable)
            {
                ShowAppOpenAd();
            }
        }
    }

    public bool IsAdAvailable
    {
        get
        {
            return _appOpenAd != null
                   && _appOpenAd.CanShowAd()
                   && DateTime.Now < _expireTime;
        }
    }

    /// <summary>
    /// Loads the app open ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        if(!CanShowAds) return;

        // Clean up the old ad before loading a new one.
        if(_appOpenAd != null)
        {
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }

        string adUnitId = adUnits.testAds ? "ca-app-pub-3940256099942544/3419835294" : adUnits.appOpenId;
        DebugMsg("Loading the app open ad: " + adUnitId);

        // send the request to load the ad.
        AppOpenAd.Load(adUnitId, CreateAdRequest(),
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if(error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                          + ad.GetResponseInfo());

                // App open ads can be preloaded for up to 4 hours.
                _expireTime = DateTime.Now + TimeSpan.FromHours(4);

                _appOpenAd = ad;
                RegisterEventHandlers(ad);
            });
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            LoadAppOpenAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadAppOpenAd();
        };
    }

    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    public void ShowAppOpenAd()
    {
        if(!CanShowAds) return;

        if(_appOpenAd != null && _appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");
            _appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
        }
    }

    #endregion



    #region Banner Ad ------------------------------------------------------------------------------

    public void ShowBannerAd(BannerType bannerType, AdPosition adPosition = AdPosition.Bottom)
    {
        if(!CanShowAds) return;

        DebugMsg("Banner(Big):" + _bigBannerLoadStatus);
        if(_bigBannerLoadStatus)
            return;

        DestroyBannerAd();

        string adUnitId = adUnits.testAds ? "ca-app-pub-3940256099942544/6300978111" : adUnits.bannerId;
        DebugMsg("ShowBannerAd: " + adUnitId);
        if(string.IsNullOrEmpty(adUnitId)) return;

        AdSize adSize = AdSize.Banner;

        switch(bannerType)
        {
            case BannerType.Banner:
                adSize = AdSize.Banner;
                break;
            case BannerType.Medium:
                adSize = AdSize.MediumRectangle;
                break;
            case BannerType.Adaptive:
                adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                break;
            default:
                break;
        }

        // Create a banner of adSize at adPosition
        _bannerAd = new BannerView(adUnitId, adSize, adPosition);
        RegisterEventHandlers(_bannerAd);
        _bannerAd.LoadAd(CreateAdRequest());
    }

    public void HideBannerAd()
    {
        if(_bannerAd != null)
        {
            _bannerAd.Hide();
        }

        _bannerLoadStatus = false;
    }

    private void DestroyBannerAd()
    {
        if(_bannerAd != null)
        {
            _bannerAd.Destroy();
            _bannerAd = null;
        }

        _bannerLoadStatus = false;
    }

    private void RegisterEventHandlers(BannerView ad)
    {
        ad.OnBannerAdLoaded += () =>
        {
            _bannerLoadStatus = true;
        };

        ad.OnBannerAdLoadFailed += (err) =>
        {
            _bannerLoadStatus = false;
        };
    }

    #endregion



    #region Big Banner Ad ------------------------------------------------------------------------------

    public void ShowBigBannerAd(AdPosition adPosition = AdPosition.Bottom)
    {
        if(!CanShowAds) return;

        DebugMsg("Banner:" + _bannerLoadStatus);
        if(_bannerLoadStatus)
            return;

        DestroyBigBannerAd();

        string adUnitId = adUnits.testAds ? "ca-app-pub-3940256099942544/9214589741" : adUnits.bannerId;
        DebugMsg("ShowBigBannerAd: " + adUnitId);
        if(string.IsNullOrEmpty(adUnitId)) return;

        AdSize adSize = AdSize.MediumRectangle;

        // Create a banner of adSize at adPosition
        _bigBannerAd = new BannerView(adUnitId, adSize, adPosition);
        RegisterBigBannerEventHandlers(_bigBannerAd);
        _bigBannerAd.LoadAd(CreateAdRequest());
    }

    public void HideBigBannerAd()
    {
        if(_bigBannerAd != null)
        {
            _bigBannerAd.Hide();
        }

        _bigBannerLoadStatus = false;
    }

    private void DestroyBigBannerAd()
    {
        if(_bigBannerAd != null)
        {
            _bigBannerAd.Destroy();
            _bigBannerAd = null;
        }

        _bigBannerLoadStatus = false;
    }

    private void RegisterBigBannerEventHandlers(BannerView ad)
    {
        ad.OnBannerAdLoaded += () =>
        {
            _bigBannerLoadStatus = true;
        };

        ad.OnBannerAdLoadFailed += (err) =>
        {
            _bigBannerLoadStatus = false;
        };
    }

    #endregion



    #region Interstitial Ad ------------------------------------------------------------------------

    private void LoadInterstitialAd()
    {
        if(!CanShowAds) return;

        DestroyInterstitialAd();

        string adUnitId = adUnits.testAds ? "ca-app-pub-3940256099942544/1033173712" : adUnits.intId;
        DebugMsg("RequestInterstitialAd: " + adUnitId);

        if(string.IsNullOrEmpty(adUnitId)) return;

        // send the request to load the ad.
        InterstitialAd.Load(adUnitId, CreateAdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if(error != null || ad == null)
                {
                    DebugErrorMsg("interstitial ad failed to load an ad with error : " + error);
                    return;
                }

                DebugMsg("Interstitial ad loaded with response : " + ad.GetResponseInfo());

                _interAd = ad;
                RegisterEventHandlers(_interAd);
            });

    }

    public void ShowInterstitialAd(UnityAction callbackMethod = null)
    {
        if(!CanShowAds) return;
        if(!_firstAdTimeThresholdReached) return;

        DebugMsg("ShowInterstitialAd");

        if(_interAd != null)
        {
            if(_interAd.CanShowAd())
            {
                _intAdCallback = callbackMethod;
                _interAd.Show();
            }
            else
            {
                LoadInterstitialAd();
            }
        }

    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            if(_intAdCallback != null)
            {
                _intAdCallback.Invoke();
                _intAdCallback = null;
            }
            Invoke(nameof(LoadInterstitialAd), adUnits.intLoadAgainTime);
        };

        ad.OnAdFullScreenContentFailed += (err) =>
        {
            OnIntersititialFailed?.Invoke();
        };
    }

    private void DestroyInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if(_interAd != null)
        {
            _interAd.Destroy();
            _interAd = null;
        }
    }

    public bool IsIntAdLoaded => _interAd != null && _interAd.CanShowAd();

    #endregion



    #region Reward Ad ------------------------------------------------------------------------------

    public void LoadRewardAd()
    {
        if(!CanShowAds) return;

        DestroyRewardAd();

        string adUnitId = adUnits.testAds ? "ca-app-pub-3940256099942544/5224354917" : adUnits.rewardId;
        DebugMsg("RequestRewardAd: " + adUnitId);

        if(string.IsNullOrEmpty(adUnitId)) return;

        // send the request to load the ad.
        RewardedAd.Load(adUnitId, CreateAdRequest(),
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if(error != null || ad == null)
                {
                    DebugErrorMsg("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                _rewardAd = ad;

                RegisterEventHandlers(_rewardAd);
            });
    }

    public void ShowRewardAd(UnityAction callBackMethod = null)
    {
        if(!CanShowAds) return;

        DebugMsg("ShowRewardAd");

        if(_rewardAd != null)
        {
            if(_rewardAd.CanShowAd())
            {
                const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

                IsInAd = true;

                _rewardAd.Show((Reward reward) =>
                {
                    if(callBackMethod != null)
                    {
                        callBackMethod.Invoke();
                        callBackMethod = null;
                    }
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
            else
            {
                LoadRewardAd();
            }
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Invoke(nameof(LoadRewardAd), adUnits.rewardLoadAgainTime);

            OnRewardClosed?.Invoke();

            IsInAd = false;
        };
    }

    private void DestroyRewardAd()
    {
        // Clean up the old ad before loading a new one.
        if(_rewardAd != null)
        {
            _rewardAd.Destroy();
            _rewardAd = null;
        }
    }


    public bool IsRewardedLoaded => _rewardAd != null && _rewardAd.CanShowAd();

    #endregion



    #region Debugging ------------------------------------------------------------------------------

    private void DebugMsg(string msg)
    {
        if(adUnits.debugging)
        {
            Debug.Log(msg);
        }
    }

    private void DebugErrorMsg(string msg)
    {
        if(adUnits.debugging)
        {
            Debug.LogError(msg);
        }
    }

    #endregion



    #region DEVICE

    private bool IsNetworkAvailable => Application.internetReachability != NetworkReachability.NotReachable;

    private bool IsDeviceEligible => SystemInfo.systemMemorySize >= minMemory;

    private bool CanShowAds => IsNetworkAvailable && IsDeviceEligible;

    #endregion



    [System.Serializable]
    public struct AdPlacement
    {
        public string bannerId, bigBannerId, intId, rewardId, appOpenId;
        public float intLoadAgainTime, rewardLoadAgainTime;
        public bool debugging, testAds;
    }

}

public enum BannerType
{
    Banner,
    Medium,
    Adaptive
}
