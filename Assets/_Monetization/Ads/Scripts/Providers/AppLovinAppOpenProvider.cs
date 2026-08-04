using UnityEngine;
//using AdjustSdk;
using SexyDevs.Ads;

/// <summary>
/// IAppOpenProvider backed by AppLovin MAX App Open Ads.
/// Used in Scenario 2 (AppLovin plain — MAX handles AOA natively).
///
/// NOTE: Guarded by #if MAX_SDK. Add the MAX_SDK scripting define
/// once the AppLovin MAX plugin is imported into the project.
/// </summary>
public class AppLovinAppOpenProvider : IAppOpenProvider
{
    private readonly string _adUnitId;

    public bool IsShowingAOA { get; private set; }

    public AppLovinAppOpenProvider(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    // ── IAppOpenProvider ──────────────────────────────────────────────────

    public void Load()
    {
        if(MonetizationData.RemoveAds) return;

#if MAX_SDK
        MaxSdk.LoadAppOpenAd(_adUnitId);
        RegisterCallbacks();
        Debug.Log("[AppLovinAOA] Load requested");
#else
        Debug.LogWarning("[AppLovinAOA] MAX SDK not imported. Define MAX_SDK scripting symbol.");
#endif
    }

    public void ShowIfReady()
    {
        if(MonetizationData.RemoveAds) return;

#if MAX_SDK
        if(MaxSdk.IsAppOpenAdReady(_adUnitId))
        {
            Debug.Log("[AppLovinAOA] Showing");
            MaxSdk.ShowAppOpenAd(_adUnitId);
        }
        else
        {
            Debug.Log("[AppLovinAOA] Not ready — reloading");
            Load();
        }
#endif
    }

    // ── MAX Callbacks ─────────────────────────────────────────────────────

#if MAX_SDK
    private bool _callbacksRegistered;

    private void RegisterCallbacks()
    {
        if(_callbacksRegistered) return;
        _callbacksRegistered = true;

        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += (id, info) =>
        {
            if(id == _adUnitId) Debug.Log("[AppLovinAOA] Loaded");
        };
        MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += (id, info) =>
        {
            if(id == _adUnitId)
            {
                Debug.LogError("[AppLovinAOA] Load failed: " + info.Message);
                Load();
            }
        };
        MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += (id, info) =>
        {
            if(id == _adUnitId) IsShowingAOA = true;
        };
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += (id, info) =>
        {
            if(id == _adUnitId)
            {
                CoroutineRunner.Instance.WaitForTimeDelayAndExecute(() =>
                {
                    IsShowingAOA = false;
                }, 1f);

                MonetizationEvents.OnAdTimerReset?.Invoke();
                Load();
            }
        };
        MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += (id, err, info) =>
        {
            if(id == _adUnitId)
            {
                Debug.LogError("[AppLovinAOA] Show failed: " + err.Message);
                Load();
            }
        };
        MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (id, info) =>
        {
            if(id != _adUnitId || info.Revenue <= 0) return;
            //var adj = new AdjustAdRevenue("applovin_max_sdk");
            //adj.SetRevenue(info.Revenue, "USD");
            //adj.AdRevenueNetwork   = info.NetworkName;
            //adj.AdRevenuePlacement = "app_open";
            //Adjust.TrackAdRevenue(adj);
        };
    }
#endif
}
