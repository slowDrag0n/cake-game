using UnityEngine;

/// <summary>
/// Read-only documentation asset for the SDK_For_Ads plugin.
/// Create via: Assets → Create → SexyDevs → SDK README
/// Open in the Inspector to read integration docs without leaving Unity.
/// </summary>
[CreateAssetMenu(fileName = "SDK_README", menuName = "SexyDevs/SDK README", order = 0)]
public class SDKReadme : ScriptableObject
{
    [Header("SDK For Ads — Monetization Plugin")]
    [Space(4)]

    [Header("Maintainer")]
    public string maintainer = "SexyDevs (SexyDevs.Utils namespace)";
    public string lastUpdated = "June 29, 2026";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── DEVELOPER SETUP — 3 STEPS ──────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(3, 3)]
    public string setup = 
        "1. Set Ad Provider dropdown on MediationAdsManager\n" +
        "2. Fill in ID fields for that scenario (others are hidden)\n" +
        "3. Hit Play";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── AD PROVIDER SCENARIOS ────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(5, 5)]
    public string scenarios =
        "AdMobOnly            — AdMob handles everything incl. App Open\n" +
        "AppLovinOnly         — AppLovin MAX handles everything incl. App Open\n" +
        "AppLovinWithAdMobAOA — MAX for Banner/Inter/Rewarded, AdMob for App Open\n\n" +
        "To enable AppLovin: import MAX plugin → add MAX_SDK scripting define symbol";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── PUBLIC API ───────────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(10, 10)]
    public string adsAPI =
        "// Interstitials — gated by timer when Use Ad Timer is on\n" +
        "MediationAdsManager.Instance.ShowInterstitial(state => { });\n" +
        "MediationAdsManager.Instance.ShowStaticInterstitial(state => { });\n\n" +
        "// Rewarded — never gated\n" +
        "MediationAdsManager.Instance.ShowRewarded(state => { });\n\n" +
        "// Banner / MREC\n" +
        "MediationAdsManager.Instance.ShowBanner();\n" +
        "MediationAdsManager.Instance.HideBanner();\n" +
        "MediationAdsManager.Instance.ShowMrec();\n" +
        "MediationAdsManager.Instance.HideMrec();\n\n" +
        "// App Open — call on foreground/focus\n" +
        "AppOpenAdManager.Instance.ShowAdIfReady();\n" +
        "AppOpenAdManager.Instance.canShowAD = false; // gate during gameplay";

    [TextArea(5, 5)]
    public string iapAPI =
        "InappManager.Instance.Purchase_product(\"product_id\");\n" +
        "InappManager.Instance.IsProductOwned(\"product_id\");       // bool\n" +
        "InappManager.Instance.IsSubscriptionActive(\"product_id\"); // bool\n\n" +
        "// Products configured in InappManager Inspector via InappManagerEditor";

    [TextArea(4, 4)]
    public string analyticsAPI =
        "FbaManager.Instance.SendCustomEvent(\"event_name\");\n" +
        "FbaManager.Instance.GetRemoteConfigValue<float>(FbaManager.RemoteConfigKeys.AdsInterval);\n" +
        "FbaManager.Instance.IsRemoteConfigReady; // bool\n" +
        "// Firebase Analytics + Remote Config. Initialized automatically on startup.";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── AD TIMER ─────────────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(7, 7)]
    public string adTimer =
        "Toggle 'Use Ad Timer' on MediationAdsManager to enable interval gating.\n\n" +
        "FirstAdInterval  (first_ad_interval, default 60s)\n" +
        "  Session-start gate. No interstitial fires until this expires.\n" +
        "  Fires MonetizationEvents.OnFirstAdIntervalCompleted when done.\n\n" +
        "AdsInterval  (ad_interval, default 30s)\n" +
        "  Between-ad cooldown. Resets on every full-screen ad close.\n" +
        "  ShowInterstitial() blocks silently while counting — no extra checks needed.";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── EVENTS ───────────────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(8, 8)]
    public string gameEvents =
        "GameEvents — YOUR GAME fires these:\n\n" +
        "GameEvents.OnLevelStart?.Invoke();\n" +
        "GameEvents.OnLevelComplete?.Invoke();\n" +
        "GameEvents.OnLevelFailed?.Invoke();\n" +
        "GameEvents.OnLevelRestart?.Invoke();\n" +
        "GameEvents.OnLevelSpawned?.Invoke();\n" +
        "GameEvents.OnNotEnoughResources?.Invoke(\"message\");\n" +
        "GameEvents.OnAdsRemoved?.Invoke();";

    [TextArea(9, 9)]
    public string monetizationEvents =
        "MonetizationEvents — SDK fires these, YOUR GAME subscribes:\n\n" +
        "MonetizationEvents.OnRemoteConfigUpdated      // Remote Config ready\n" +
        "MonetizationEvents.OnFirstAdIntervalCompleted // interstitials unlocked\n" +
        "MonetizationEvents.OnAdTimerReset             // any full-screen ad closed\n" +
        "MonetizationEvents.OnIAPInit                  // IAP store ready\n" +
        "MonetizationEvents.OnIAPPurchased             // purchase completed\n" +
        "MonetizationEvents.OnRemoveAdsPurchased       // remove ads granted\n\n" +
        "Example:\n" +
        "MonetizationEvents.OnFirstAdIntervalCompleted += EnableAdButton;";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── AD STATE ENUM ────────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(7, 7)]
    public string adState =
        "AdState.Opening   — ad is on screen\n" +
        "AdState.Reward    — reward earned (rewarded only)\n" +
        "AdState.Closed    — user dismissed\n" +
        "AdState.Failed    — show or load failed\n" +
        "AdState.Canceled  — timed out or blocked by timer\n" +
        "AdState.Loaded    — informational\n\n" +
        "Example: MediationAdsManager.Instance.ShowRewarded(state => {\n" +
        "    if (state == AdState.Reward) GiveReward();\n" +
        "});";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── MONETIZATION DATA ────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(7, 7)]
    public string monetizationData =
        "Static PlayerPrefs wrappers — access anywhere without a reference:\n\n" +
        "MonetizationData.RemoveAds              // bool\n" +
        "MonetizationData.RemoveAdsPurchased     // bool\n" +
        "MonetizationData.SubscriptionPurchased  // bool\n" +
        "MonetizationData.CurrentLevel           // int\n" +
        "MonetizationData.SessionCounter         // int\n" +
        "MonetizationData.Currency               // int";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── REMOTE CONFIG KEYS ──────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(18, 18)]
    public string remoteConfigKeys =
        "ad_interval           float  30s   Between-ad cooldown\n" +
        "first_ad_interval     float  60s   Session-start gate\n" +
        "inter_ad_show         bool   true  Whether to show interstitials\n" +
        "inter_ad_trigger      bool   true  Trigger condition\n" +
        "inter_ad_type         bool   false Which type to show\n" +
        "Inter_ad_start        int    1     Level to start ads\n" +
        "afk_interval          float  15s   Gameplay idle interval\n" +
        "app_open_ad           bool   true  AOA enabled\n" +
        "app_open_ses          bool   false AOA per session\n" +
        "resume_ad             bool   true  Resume/foreground ad\n" +
        "ban_large             bool   true  Show MREC\n" +
        "ad_loading            bool   false Show loading panel\n" +
        "remove_ad_popup       bool   false Remove-ads popup\n" +
        "remove_ad_level       int    7     Level to show popup\n" +
        "rate_us               bool   true  Rate Us prompt\n" +
        "rate_us_level         int    20    Level to show it\n" +
        "CanUseMemoryCheck     bool   true  Memory guard enabled\n" +
        "MinMemoryCheck        int    400   Min free MB to load ads";

    // ─────────────────────────────────────────────────────────────────────
    [Space(8)]
    [Header("── EXTERNAL SDKs ────────────────────────────────────────────")]
    // ─────────────────────────────────────────────────────────────────────

    [TextArea(6, 6)]
    public string externalSDKs =
        "Google Mobile Ads (AdMob) — GoogleMobileAds, GoogleMobileAds.Core,\n" +
        "                            GoogleMobileAds.Unity, GoogleMobileAds.Editor\n" +
        "Firebase                  — Firebase.App, Firebase.Analytics,\n" +
        "                            Firebase.RemoteConfig, Firebase.TaskExtension\n" +
        "Adjust                    — AdjustSdk\n" +
        "com.unity.ads is NOT used.";

    [TextArea(5, 5)]
    public string upmPackages =
        "com.unity.purchasing   5.1.2  IAP\n" +
        "com.unity.inputsystem  1.18.0 UI input\n" +
        "com.unity.ugui         2.0.0  Unity UI\n" +
        "com.unity.timeline     1.8.10 Timeline\n" +
        "com.coplaydev.unity-mcp 9.7.3 MCP for Unity (tooling)";
}
