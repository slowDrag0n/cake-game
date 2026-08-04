using System;
using SexyDevs.Ads;
using SexyDevs.Utils;
using UnityEngine;

/// <summary>
/// App Open Ad manager.  Delegates all SDK work to IAppOpenProvider so the
/// correct backend (AdMob or AppLovin MAX) is used based on the scenario
/// chosen in MediationAdsManager.
///
/// USAGE:
///   Call AppOpenAdManager.Instance.ShowAdIfReady() when the app resumes.
///   That's it — provider selection is automatic.
/// </summary>
public class AppOpenAdManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    // Inspector
    // ─────────────────────────────────────────────────────────────────────
    public static AppOpenAdManager Instance { get; private set; }
    // ─────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Guards against showing AOA while another ad is visible.</summary>
    public bool canShowAD = true;

    /// <summary>True while an App Open Ad is on screen.</summary>
    public bool AoaImpression => _provider?.IsShowingAOA ?? false;

    private IAppOpenProvider _provider;


    private void Awake()
    {
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Internal init — called by MediationAdsManager after SDK boots
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// MediationAdsManager calls this once the SDK is ready,
    /// passing the correct provider for the chosen scenario.
    /// </summary>
    ///
    public void Init(IAppOpenProvider provider)
    {
        _provider = provider;
        if (provider != null)
            Debug.Log($"[AOA] Provider set: {provider.GetType().Name}");
        else
            Debug.LogWarning("[AOA] Init called with a null provider — App Open Ads will not function until a valid provider is set.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Pre-fetch the next App Open Ad.</summary>
    public void Load()
    {
        if (MonetizationData.RemoveAds) return;
        _provider?.Load();
    }

    /// <summary>
    /// Show the App Open Ad if one is ready and canShowAD is true.
    /// Call this from your AppStateEventNotifier / OnApplicationFocus handler.
    /// </summary>
    public void ShowAdIfReady()
    {
        if (!canShowAD || MonetizationData.RemoveAds) return;
        _provider?.ShowIfReady();
    }
}