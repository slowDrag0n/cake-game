using UnityEngine;
using UnityEngine.Events;
//using AdjustSdk;
using SexyDevs.Ads;

/// <summary>
/// IAdProvider implementation backed by AppLovin MAX SDK.
/// Handles Banner, MREC, Interstitial, and Rewarded via MAX mediation.
///
/// HOW TO WIRE UP:
///   1. Import the AppLovin MAX Unity plugin.
///   2. In AppLovin dashboard, create ad units and copy their IDs.
///   3. On the MediationAdsManager prefab, fill in the AppLovin ID fields.
///   4. Set AdProvider = AppLovinOnly, AppLovinWithAdMobAOA, or AppLovinWithAdMobAOAAndMREC.
///   That's it — no code changes needed.
///
/// NOTE: This class wraps MaxSdk calls behind #if MAX_SDK guards so the
/// project compiles cleanly even when the MAX plugin is not yet imported.
/// Remove the guards once MAX is in the project.
/// </summary>
public class AppLovinProvider : IAdProvider
{
    private readonly string _interstitialId;
    private readonly string _staticInterstitialId;
    private readonly string _rewardedId;
    private readonly string _bannerId;
    private readonly string _mrecId;
    private readonly string _maxSdkKey;

    private bool _initialized;
    private System.Action _onReady;

    private bool _interLoaded;
    private bool _staticLoaded;
    private bool _rewardedLoaded;
    private bool _bannerShowing;
    private bool _mrecShowing;

    private UnityAction<AdState> _interCallback;
    private UnityAction<AdState> _staticCallback;
    private UnityAction<AdState> _rewardedCallback;
    private bool _rewardGranted;

    public AppLovinProvider(
        string sdkKey,
        string interstitialId,
        string staticInterstitialId,
        string rewardedId,
        string bannerId,
        string mrecId)
    {
        _maxSdkKey = sdkKey;
        _interstitialId = interstitialId;
        _staticInterstitialId = staticInterstitialId;
        _rewardedId = rewardedId;
        _bannerId = bannerId;
        _mrecId = mrecId;
    }

    // ── IAdProvider ───────────────────────────────────────────────────────

    public void Initialize(System.Action onReady)
    {
        if(_initialized) return;
        _onReady = onReady;

#if MAX_SDK
        MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;
        MaxSdk.SetSdkKey(_maxSdkKey);
        MaxSdk.InitializeSdk();
#else
        Debug.LogWarning("[AppLovinProvider] MAX SDK not imported. Define MAX_SDK scripting symbol once plugin is in project.");
        _initialized = true;
        onReady?.Invoke();
#endif
    }

#if MAX_SDK
    private bool _callbacksRegistered;

    private void OnMaxInitialized(MaxSdkBase.SdkConfiguration config)
    {
        _initialized = true;
        Debug.Log("[AppLovinProvider] MAX SDK initialized");

        // Guard against duplicate subscriptions if OnSdkInitializedEvent ever fires more than once
        if(!_callbacksRegistered)
        {
            _callbacksRegistered = true;
            RegisterInterstitialCallbacks();
            RegisterStaticInterstitialCallbacks();
            RegisterRewardedCallbacks();
        }

        _onReady?.Invoke();
    }
#endif

    // ── Interstitial ──────────────────────────────────────────────────────

    public void LoadInterstitial()
    {
#if MAX_SDK
        MaxSdk.LoadInterstitial(_interstitialId);
#endif
    }

    public void ShowInterstitial(UnityAction<AdState> callback)
    {
#if MAX_SDK
        if(!MaxSdk.IsInterstitialReady(_interstitialId))
        {
            callback?.Invoke(AdState.Failed);
            LoadInterstitial();
            return;
        }
        _interCallback = callback;
        MaxSdk.ShowInterstitial(_interstitialId);
#else
        callback?.Invoke(AdState.Failed);
#endif
    }

    // ── Static Interstitial ───────────────────────────────────────────────

    public void LoadStaticInterstitial()
    {
#if MAX_SDK
        MaxSdk.LoadInterstitial(_staticInterstitialId);
#endif
    }

    public void ShowStaticInterstitial(UnityAction<AdState> callback)
    {
#if MAX_SDK
        if(!MaxSdk.IsInterstitialReady(_staticInterstitialId))
        {
            callback?.Invoke(AdState.Failed);
            LoadStaticInterstitial();
            return;
        }
        _staticCallback = callback;
        MaxSdk.ShowInterstitial(_staticInterstitialId);
#else
        callback?.Invoke(AdState.Failed);
#endif
    }

    // ── Rewarded ──────────────────────────────────────────────────────────

    public void LoadRewarded()
    {
#if MAX_SDK
        MaxSdk.LoadRewardedAd(_rewardedId);
#endif
    }

    public void ShowRewarded(UnityAction<AdState> callback)
    {
#if MAX_SDK
        if(!MaxSdk.IsRewardedAdReady(_rewardedId))
        {
            callback?.Invoke(AdState.Failed);
            LoadRewarded();
            return;
        }
        _rewardedCallback = callback;
        _rewardGranted = false;
        MaxSdk.ShowRewardedAd(_rewardedId);
#else
        callback?.Invoke(AdState.Failed);
#endif
    }

    // ── Banner ────────────────────────────────────────────────────────────

    public void LoadBanner()
    {
        if(MonetizationData.RemoveAds) return;
#if MAX_SDK
        MaxSdk.CreateBanner(_bannerId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += (id, info) => ShowBanner();
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (id, info) => TrackRevenue(info, "banner");
#endif
    }

    public void ShowBanner()
    {
        if(MonetizationData.RemoveAds || _bannerShowing) return;
#if MAX_SDK
        MaxSdk.ShowBanner(_bannerId);
#endif
        _bannerShowing = true;
    }

    public void HideBanner()
    {
#if MAX_SDK
        MaxSdk.HideBanner(_bannerId);
#endif
        _bannerShowing = false;
    }

    // ── MREC ─────────────────────────────────────────────────────────────

    public void LoadMrec()
    {
        if(MonetizationData.RemoveAds) return;
#if MAX_SDK
        MaxSdk.CreateMRec(_mrecId, MaxSdkBase.AdViewPosition.TopCenter);
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += (id, info) => TrackRevenue(info, "mrec");
#endif
    }

    public void ShowMrec()
    {
        if(MonetizationData.RemoveAds || _mrecShowing) return;
#if MAX_SDK
        MaxSdk.ShowMRec(_mrecId);
#endif
        _mrecShowing = true;
    }

    public void HideMrec()
    {
#if MAX_SDK
        MaxSdk.HideMRec(_mrecId);
#endif
        _mrecShowing = false;
    }

    // ── MAX Callbacks ─────────────────────────────────────────────────────

#if MAX_SDK
    private void RegisterInterstitialCallbacks()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (id, info) =>
        {
            if(id == _interstitialId) _interLoaded = true;
        };
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (id, info) =>
        {
            if(id == _interstitialId) { _interLoaded = false; LoadInterstitial(); }
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (id, info) =>
        {
            if(id == _interstitialId) _interCallback?.Invoke(AdState.Opening);
        };
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (id, info) =>
        {
            if(id == _interstitialId)
            {
                _interCallback?.Invoke(AdState.Closed);
                _interCallback = null;
                _interLoaded = false;
                LoadInterstitial();
            }
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (id, err, info) =>
        {
            if(id == _interstitialId)
            {
                _interCallback?.Invoke(AdState.Failed);
                _interCallback = null;
                LoadInterstitial();
            }
        };
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == _interstitialId) TrackRevenue(info, "interstitial");
        };
    }

    private void RegisterStaticInterstitialCallbacks()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (id, info) =>
        {
            if(id == _staticInterstitialId) _staticLoaded = true;
        };
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (id, info) =>
        {
            if(id == _staticInterstitialId) { _staticLoaded = false; LoadStaticInterstitial(); }
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (id, info) =>
        {
            if(id == _staticInterstitialId) _staticCallback?.Invoke(AdState.Opening);
        };
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (id, info) =>
        {
            if(id == _staticInterstitialId)
            {
                _staticCallback?.Invoke(AdState.Closed);
                _staticCallback = null;
                _staticLoaded = false;
                LoadStaticInterstitial();
            }
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (id, err, info) =>
        {
            if(id == _staticInterstitialId)
            {
                _staticCallback?.Invoke(AdState.Failed);
                _staticCallback = null;
                LoadStaticInterstitial();
            }
        };
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == _staticInterstitialId) TrackRevenue(info, "interstitial_static");
        };
    }

    private void RegisterRewardedCallbacks()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (id, info) =>
        {
            if(id == _rewardedId) _rewardedLoaded = true;
        };
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (id, info) =>
        {
            if(id == _rewardedId) { _rewardedLoaded = false; LoadRewarded(); }
        };
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (id, info) =>
        {
            if(id == _rewardedId) _rewardedCallback?.Invoke(AdState.Opening);
        };
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (id, reward, info) =>
        {
            if(id == _rewardedId)
            {
                _rewardGranted = true;
                _rewardedCallback?.Invoke(AdState.Reward);
            }
        };
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (id, info) =>
        {
            if(id == _rewardedId)
            {
                if(!_rewardGranted) _rewardedCallback?.Invoke(AdState.Canceled);
                _rewardedCallback?.Invoke(AdState.Closed);
                _rewardedCallback = null;
                _rewardedLoaded = false;
                _rewardGranted = false;
                LoadRewarded();
            }
        };
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (id, err, info) =>
        {
            if(id == _rewardedId)
            {
                _rewardedCallback?.Invoke(AdState.Failed);
                _rewardedCallback = null;
                LoadRewarded();
            }
        };
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id == _rewardedId) TrackRevenue(info, "rewarded");
        };
    }

    private static void TrackRevenue(MaxSdkBase.AdInfo info, string placement)
    {
        double revenue = info.Revenue;
        if(revenue <= 0) return;
        //var adj = new AdjustAdRevenue("applovin_max_sdk");
        //adj.SetRevenue(revenue, "USD");
        //adj.AdRevenueNetwork   = info.NetworkName;
        //adj.AdRevenuePlacement = placement;
        //Adjust.TrackAdRevenue(adj);
    }
#endif
}
