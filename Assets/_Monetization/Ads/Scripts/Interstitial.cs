#if ADMOB_SDK
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
//using AdjustSdk;

public class Interstitial
{
    private readonly string _adUnitId;
    private InterstitialAd  _ad;
    private UnityAction<AdState> _callback;

    public Interstitial(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    public void Request()
    {
        Debug.Log($"[AdMob] Loading Interstitial => {_adUnitId}");
        InterstitialAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            if(error != null || ad == null)
            {
                Debug.LogWarning($"[AdMob] Interstitial load failed: {error?.GetMessage()}");
                return;
            }

            _ad = ad;
            RegisterEvents();
            Debug.Log($"[AdMob] Interstitial loaded: {_adUnitId}");
        });
    }

    public void Show(UnityAction<AdState> callback)
    {
        _callback = callback;

        if(_ad == null || !_ad.CanShowAd())
        {
            Debug.LogWarning("[AdMob] Interstitial not ready");
            _callback?.Invoke(AdState.Failed);
            _callback = null;
            return;
        }

        _ad.Show();
    }

    public bool IsReady() => _ad != null && _ad.CanShowAd();

    private void RegisterEvents()
    {
        _ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("[AdMob] Interstitial opened");
            _callback?.Invoke(AdState.Opening);
        };

        _ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdMob] Interstitial closed");
            _callback?.Invoke(AdState.Closed);
            _callback = null;
            Dispose();
        };

        _ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning($"[AdMob] Interstitial show failed: {error.GetMessage()}");
            _callback?.Invoke(AdState.Failed);
            _callback = null;
            Dispose();
        };

        _ad.OnAdPaid += TrackRevenue;
    }

    private void TrackRevenue(AdValue adValue)
    {
        if(adValue.Value <= 0) return;
        //var adj = new AdjustAdRevenue("admob_sdk");
        //adj.SetRevenue(adValue.Value / 1_000_000d, adValue.CurrencyCode);
        //adj.AdRevenueNetwork   = "google_admob";
        //adj.AdRevenueUnit      = _adUnitId;
        //adj.AdRevenuePlacement = "interstitial";
        //Adjust.TrackAdRevenue(adj);
    }

    private void Dispose()
    {
        _ad?.Destroy();
        _ad = null;
    }
}
#endif
