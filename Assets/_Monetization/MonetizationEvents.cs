using UnityEngine.Events;

/// <summary>
/// Events owned and fired by the monetization SDK.
/// Your game subscribes to these — do not fire them from game code.
/// </summary>
public static class MonetizationEvents
{
    // ── Remote Config ─────────────────────────────────────────────────────

    /// <summary>
    /// Fired by FbaManager when Remote Config has fetched and activated.
    /// MediationAdsManager subscribes to this to initialize ads.
    /// Subscribe in your game to read config values after this fires.
    /// </summary>
    public static UnityAction OnRemoteConfigUpdated;

    // ── Ads ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired when the session first-ad gate expires and interstitials are unlocked.
    /// Subscribe to enable ad-related UI or trigger your first interstitial.
    /// </summary>
    public static UnityAction OnFirstAdIntervalCompleted;

    /// <summary>
    /// Fired when any full-screen ad closes (inter, static, rewarded, app open).
    /// MediationAdsManager uses this internally to reset the ad interval timer.
    /// </summary>
    public static UnityAction OnAdTimerReset;

    // ── IAP ───────────────────────────────────────────────────────────────

    /// <summary>Fired when Unity IAP has connected to the store and is ready.</summary>
    public static UnityAction OnIAPInit;

    /// <summary>Fired when any purchase completes successfully.</summary>
    public static UnityAction OnIAPPurchased;

    /// <summary>Fired when a Remove Ads product has been purchased.</summary>
    public static UnityAction OnRemoveAdsPurchased;
}
