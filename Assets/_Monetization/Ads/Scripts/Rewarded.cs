#if ADMOB_SDK
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using AdjustSdk;

public class Rewarded
{
    private readonly string _adUnitId;
    private RewardedAd      _ad;
    private UnityAction<AdState> _callback;

    public Rewarded(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    public void Request()
    {
        Debug.Log($"[AdMob] Loading Rewarded => {_adUnitId}");
        RewardedAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            if(error != null || ad == null)
            {
                Debug.LogWarning($"[AdMob] Rewarded load failed: {error?.GetMessage()}");
                return;
            }

            _ad = ad;
            RegisterEvents();
            Debug.Log($"[AdMob] Rewarded loaded: {_adUnitId}");
        });
    }

    public void Show(UnityAction<AdState> callback)
    {
        _callback = callback;

        if(_ad == null || !_ad.CanShowAd())
        {
            Debug.LogWarning("[AdMob] Rewarded not ready");
            _callback?.Invoke(AdState.Failed);
            _callback = null;
            return;
        }

        _ad.Show(reward =>
        {
            Debug.Log($"[AdMob] Reward earned: {reward.Type} x{reward.Amount}");
            _callback?.Invoke(AdState.Reward);
        });
    }

    public bool IsReady() => _ad != null && _ad.CanShowAd();

    private void RegisterEvents()
    {
        _ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("[AdMob] Rewarded opened");
            _callback?.Invoke(AdState.Opening);
        };

        _ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdMob] Rewarded closed");
            _callback?.Invoke(AdState.Closed);
            _callback = null;
            Dispose();
        };

        _ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning($"[AdMob] Rewarded show failed: {error.GetMessage()}");
            _callback?.Invoke(AdState.Failed);
            _callback = null;
            Dispose();
        };

        _ad.OnAdPaid += TrackRevenue;
    }

    private void TrackRevenue(AdValue adValue)
    {
        if(adValue.Value <= 0) return;
        var adj = new AdjustAdRevenue("admob_sdk");
        adj.SetRevenue(adValue.Value / 1_000_000d, adValue.CurrencyCode);
        adj.AdRevenueNetwork = "google_admob";
        adj.AdRevenueUnit = _adUnitId;
        adj.AdRevenuePlacement = "rewarded";
        Adjust.TrackAdRevenue(adj);
    }

    private void Dispose()
    {
        _ad?.Destroy();
        _ad = null;
    }
}
#endif
