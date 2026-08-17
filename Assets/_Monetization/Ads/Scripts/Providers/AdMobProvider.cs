#if ADMOB_SDK
using System;
using UnityEngine;
using UnityEngine.Events;
using GoogleMobileAds.Api;
using AdjustSdk;
using SexyDevs.Ads;

/// <summary>
/// IAdProvider backed by Google Mobile Ads (AdMob).
/// Plain single-unit loading — no waterfall.
/// </summary>
public class AdMobProvider : IAdProvider
{
    // ── Ad unit IDs ───────────────────────────────────────────────────────
    private readonly string _interstitialId;
    private readonly string _staticInterstitialId;
    private readonly string _rewardedId;
    private readonly string _bannerId;
    private readonly string _mrecId;

    // ── Ad instances ──────────────────────────────────────────────────────
    private Interstitial _inter;
    private Interstitial _staticInter;
    private Rewarded     _rewarded;

    // ── Banner / MREC ─────────────────────────────────────────────────────
    private BannerView _bannerView;
    private bool       _bannerShowing;

    private BannerView _mrecView;
    private bool       _mrecLoaded;
    private bool       _mrecVisible;

    private bool _initialized;

    public AdMobProvider(
        string interstitialId,
        string staticInterstitialId,
        string rewardedId,
        string bannerId,
        string mrecId)
    {
        _interstitialId = interstitialId;
        _staticInterstitialId = staticInterstitialId;
        _rewardedId = rewardedId;
        _bannerId = bannerId;
        _mrecId = mrecId;
    }

    // ── IAdProvider: Init ─────────────────────────────────────────────────

    public void Initialize(Action onReady)
    {
        if(_initialized) return;
        MobileAds.Initialize(_ =>
        {
            _initialized = true;
            Debug.Log("[AdMobProvider] Initialized");
            onReady?.Invoke();
        });
    }

    // ── IAdProvider: Interstitial ─────────────────────────────────────────

    public void LoadInterstitial()
    {
        _inter = new Interstitial(_interstitialId);
        _inter.Request();
        Debug.Log("[AdMobProvider] Loading Interstitial");
    }

    public void ShowInterstitial(UnityAction<AdState> callback)
    {
        if(_inter == null) { callback?.Invoke(AdState.Failed); return; }
        _inter.Show(state =>
        {
            callback?.Invoke(state);
            if(state is AdState.Closed or AdState.Failed or AdState.Canceled)
                LoadInterstitial();
        });
    }

    // ── IAdProvider: Static Interstitial ──────────────────────────────────

    public void LoadStaticInterstitial()
    {
        _staticInter = new Interstitial(_staticInterstitialId);
        _staticInter.Request();
        Debug.Log("[AdMobProvider] Loading Static Interstitial");
    }

    public void ShowStaticInterstitial(UnityAction<AdState> callback)
    {
        if(_staticInter == null) { callback?.Invoke(AdState.Failed); return; }
        _staticInter.Show(state =>
        {
            callback?.Invoke(state);
            if(state is AdState.Closed or AdState.Failed or AdState.Canceled)
                LoadStaticInterstitial();
        });
    }

    // ── IAdProvider: Rewarded ─────────────────────────────────────────────

    public void LoadRewarded()
    {
        _rewarded = new Rewarded(_rewardedId);
        _rewarded.Request();
        Debug.Log("[AdMobProvider] Loading Rewarded");
    }

    public void ShowRewarded(UnityAction<AdState> callback)
    {
        if(_rewarded == null) { callback?.Invoke(AdState.Failed); return; }
        _rewarded.Show(state =>
        {
            callback?.Invoke(state);
            if(state is AdState.Closed or AdState.Failed or AdState.Canceled)
                LoadRewarded();
        });
    }

    // ── IAdProvider: Banner ───────────────────────────────────────────────

    public void LoadBanner()
    {
        if(MonetizationData.RemoveAds) return;
        _bannerView = new BannerView(
            _bannerId,
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth),
            AdPosition.Bottom);
        _bannerView.OnBannerAdLoaded += () => { _bannerView.Show(); _bannerShowing = true; Debug.Log("[AdMobProvider] Banner loaded"); };
        _bannerView.OnBannerAdLoadFailed += e => Debug.LogWarning($"[AdMobProvider] Banner failed: {e}");
        _bannerView.OnAdPaid += v => TrackRevenue(v, "banner");
        _bannerView.LoadAd(new AdRequest());
    }

    public void ShowBanner()
    {
        if(_bannerView == null || _bannerShowing) return;
        _bannerView.Show();
        _bannerShowing = true;
    }

    public void HideBanner()
    {
        if(_bannerView == null) return;
        _bannerView.Hide();
        _bannerShowing = false;
    }

    // ── IAdProvider: MREC ─────────────────────────────────────────────────

    public void LoadMrec()
    {
        if(MonetizationData.RemoveAds) return;
        _mrecView = new BannerView(_mrecId, AdSize.MediumRectangle, AdPosition.Top);
        _mrecView.OnBannerAdLoaded += () => { _mrecLoaded = true; HideMrec(); };
        _mrecView.OnAdPaid += v => TrackRevenue(v, "mrec");
        _mrecView.LoadAd(new AdRequest());
    }

    public void ShowMrec()
    {
        if(_mrecLoaded && !_mrecVisible && !MonetizationData.RemoveAds)
        {
            _mrecView.Show();
            _mrecVisible = true;
        }
    }

    public void HideMrec()
    {
        _mrecView?.Hide();
        _mrecVisible = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void TrackRevenue(AdValue adValue, string placement)
    {
        if(adValue.Value <= 0) return;
        double revenue = adValue.Value / 1_000_000d;
        var adj = new AdjustAdRevenue("admob_sdk");
        adj.SetRevenue(revenue, adValue.CurrencyCode);
        adj.AdRevenueNetwork = "google_admob";
        adj.AdRevenuePlacement = placement;
        Adjust.TrackAdRevenue(adj);
    }
}
#endif
