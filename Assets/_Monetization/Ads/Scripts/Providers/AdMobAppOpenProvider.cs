#if ADMOB_SDK
using System;
using UnityEngine;
using GoogleMobileAds.Api;
using AdjustSdk;
using SexyDevs.Ads;

/// <summary>
/// IAppOpenProvider backed by AdMob App Open Ads.
/// Used in AppLovinWithAdMobAOA, AppLovinWithAdMobAOAAndMREC, and AdMobOnly.
/// </summary>
public class AdMobAppOpenProvider : IAppOpenProvider
{
    private readonly string _adUnitId;

    private AppOpenAd _ad;
    private DateTime  _loadTime;

    public bool IsShowingAOA { get; private set; }

    public AdMobAppOpenProvider(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    // ── IAppOpenProvider ──────────────────────────────────────────────────

    public void Load()
    {
        if(MonetizationData.RemoveAds) return;

        Debug.Log("[AdMobAOA] Loading App Open Ad...");

        AppOpenAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            if(error != null || ad == null)
            {
                Debug.LogError("[AdMobAOA] Load failed: " + (error?.GetMessage() ?? "null ad"));
                _ad = null;
                return;
            }

            Debug.Log("[AdMobAOA] Loaded successfully");
            _ad = ad;
            _loadTime = DateTime.UtcNow;

            _ad.OnAdFullScreenContentOpened += () =>
            {
                IsShowingAOA = true;
                Debug.Log("[AdMobAOA] Opened");
            };

            _ad.OnAdFullScreenContentClosed += () =>
            {
                CoroutineRunner.Instance.WaitForTimeDelayAndExecute(() =>
                {
                    IsShowingAOA = false;
                }, 1f);

                Debug.Log("[AdMobAOA] Closed — reloading");
                Load();
                MonetizationEvents.OnAdTimerReset?.Invoke();
            };

            _ad.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogError("[AdMobAOA] Show failed: " + err.GetMessage());
                Load();
            };

            _ad.OnAdPaid += OnRevenuePaid;
        });
    }

    public void ShowIfReady()
    {
        if(MonetizationData.RemoveAds) return;

        if(_ad != null && _ad.CanShowAd() && !IsExpired())
        {
            Debug.Log("[AdMobAOA] Showing");
            _ad.Show();
            _ad = null;
        }
        else
        {
            Debug.Log("[AdMobAOA] Not ready or expired — reloading");
            Load();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private bool IsExpired() =>
        (DateTime.UtcNow - _loadTime).TotalHours >= 4;

    private static void OnRevenuePaid(AdValue adValue)
    {
        if(adValue.Value <= 0) return;
        double revenue = adValue.Value / 1_000_000d;
        var adj = new AdjustAdRevenue("admob_sdk");
        adj.SetRevenue(revenue, "USD");
        adj.AdRevenueNetwork = "google_admob";
        adj.AdRevenuePlacement = "app_open";
        Adjust.TrackAdRevenue(adj);
    }
}
#endif
