using System;
using UnityEngine;
using UnityEngine.Events;
using SexyDevs.Ads;
#if ADMOB_SDK
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Hybrid IAdProvider for Scenario 4 (AppLovin MAX + AdMob App Open &amp; MREC).
/// AppLovin MAX handles Banner, Interstitial, Static Interstitial, and Rewarded.
/// AdMob handles MREC only (App Open is injected separately via AdMobAppOpenProvider).
/// </summary>
public class AppLovinWithAdMobMrecProvider : IAdProvider
{
    private readonly AppLovinProvider _max;
    private readonly string _admobMrecId;

#if ADMOB_SDK
    private BannerView _mrecView;
    private bool       _mrecLoaded;
    private bool       _mrecVisible;
#endif

    private bool _adMobReady;
    private bool _maxReady;
    private Action _onReady;

    public AppLovinWithAdMobMrecProvider(
        string maxSdkKey,
        string maxInterstitialId,
        string maxStaticInterstitialId,
        string maxRewardedId,
        string maxBannerId,
        string admobMrecId)
    {
        // Pass empty MREC id to MAX — MREC is served by AdMob in this scenario.
        _max = new AppLovinProvider(
            maxSdkKey,
            maxInterstitialId,
            maxStaticInterstitialId,
            maxRewardedId,
            maxBannerId,
            string.Empty);
        _admobMrecId = admobMrecId;
    }

    public void Initialize(Action onReady)
    {
        _onReady = onReady;

#if ADMOB_SDK
        MobileAds.Initialize(_ =>
        {
            _adMobReady = true;
            Debug.Log("[HybridProvider] AdMob initialized (MREC)");
            TryCompleteInit();
        });
#else
        _adMobReady = true;
        Debug.LogWarning("[HybridProvider] ADMOB_SDK not defined — AdMob MREC disabled.");
#endif

        _max.Initialize(() =>
        {
            _maxReady = true;
            Debug.Log("[HybridProvider] AppLovin MAX initialized");
            TryCompleteInit();
        });
    }

    private void TryCompleteInit()
    {
        if (_adMobReady && _maxReady)
            _onReady?.Invoke();
    }

    // ── Full-screen / Banner → MAX ────────────────────────────────────────

    public void LoadInterstitial()       => _max.LoadInterstitial();
    public void ShowInterstitial(UnityAction<AdState> cb) => _max.ShowInterstitial(cb);

    public void LoadStaticInterstitial() => _max.LoadStaticInterstitial();
    public void ShowStaticInterstitial(UnityAction<AdState> cb) => _max.ShowStaticInterstitial(cb);

    public void LoadRewarded()           => _max.LoadRewarded();
    public void ShowRewarded(UnityAction<AdState> cb) => _max.ShowRewarded(cb);

    public void LoadBanner()             => _max.LoadBanner();
    public void ShowBanner()             => _max.ShowBanner();
    public void HideBanner()             => _max.HideBanner();

    // ── MREC → AdMob ──────────────────────────────────────────────────────

    public void LoadMrec()
    {
        if (MonetizationData.RemoveAds) return;
        if (string.IsNullOrWhiteSpace(_admobMrecId))
        {
            Debug.LogWarning("[HybridProvider] AdMob MREC ID is empty — skipping LoadMrec.");
            return;
        }

#if ADMOB_SDK
        _mrecView = new BannerView(_admobMrecId, AdSize.MediumRectangle, AdPosition.Top);
        _mrecView.OnBannerAdLoaded += () =>
        {
            _mrecLoaded = true;
            HideMrec();
            Debug.Log("[HybridProvider] AdMob MREC loaded");
        };
        _mrecView.OnBannerAdLoadFailed += e =>
            Debug.LogWarning($"[HybridProvider] AdMob MREC failed: {e}");
        _mrecView.LoadAd(new AdRequest());
#else
        Debug.LogWarning("[HybridProvider] ADMOB_SDK not defined — cannot load AdMob MREC.");
#endif
    }

    public void ShowMrec()
    {
        if (MonetizationData.RemoveAds) return;
#if ADMOB_SDK
        if (_mrecLoaded && !_mrecVisible && _mrecView != null)
        {
            _mrecView.Show();
            _mrecVisible = true;
        }
#endif
    }

    public void HideMrec()
    {
#if ADMOB_SDK
        _mrecView?.Hide();
        _mrecVisible = false;
#endif
    }
}
