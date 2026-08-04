using System;
using System.Collections;
#if ADMOB_SDK
using GoogleMobileAds.Samples;
#endif
using SexyDevs.Ads;
using SexyDevs.Utils;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central ad manager. Public API is identical across all three scenarios.
///
/// DEVELOPER SETUP — three steps only:
///   1. Set AdProvider in the Inspector to your scenario.
///   2. Fill in the ID fields for that scenario (greyed-out fields are ignored).
///   3. Hit Play. Done.
///
/// AD TIMER (optional):
///   Toggle "Use Ad Timer" in the Inspector to enable interval gating.
///   Call TryShowInterstitial() instead of ShowInterstitial() — it returns
///   false and blocks silently while either timer is still counting.
/// </summary>
public class MediationAdsManager : Singleton<MediationAdsManager>
{
    // ─────────────────────────────────────────────────────────────────────
    // Inspector
    // ─────────────────────────────────────────────────────────────────────

    [Header("Scenario")]
    [Tooltip("Pick your scenario. This is the only field that changes between projects.")]
    [SerializeField] private AdProvider adProvider = AdProvider.AdMobOnly;

    [Header("UI")]
    [SerializeField] private GameObject loadingADPanel;
    [SerializeField] private GameObject bannerBgImg;

    [Header("Ad Timer")]
    [Tooltip("Enable interval gating. Use TryShowInterstitial() to respect the timers.")]
    [SerializeField] private bool useAdTimer = true;

    // ── AdMob IDs  [Scenarios 1 & 3] ─────────────────────────────────────
    [Header("AdMob IDs  [Scenarios 1 & 3]")]
    [SerializeField] private string admobInterstitialId;
    [SerializeField] private string admobStaticInterstitialId;
    [SerializeField] private string admobRewardedId;
    [SerializeField] private string admobBannerId;
    [SerializeField] private string admobMrecId;
    [SerializeField] private string admobAppOpenId;

    // ── AppLovin MAX IDs  [Scenarios 1 & 2] ──────────────────────────────
    [Header("AppLovin MAX IDs  [Scenarios 1 & 2]")]
    [SerializeField] private string maxSdkKey;
    [SerializeField] private string maxInterstitialId;
    [SerializeField] private string maxStaticInterstitialId;
    [SerializeField] private string maxRewardedId;
    [SerializeField] private string maxBannerId;
    [SerializeField] private string maxMrecId;
    [SerializeField] private string maxAppOpenId;

    // ─────────────────────────────────────────────────────────────────────
    // Ad state
    // ─────────────────────────────────────────────────────────────────────

    public static bool IsShowingAd    { get; private set; }
    public bool        AdsInitialized { get; private set; }

    private IAdProvider _adProvider;
    private bool        _showingBanner;
    private bool        _mrecShowing;
    private bool        _wasShowingBanner;
    private bool        _wasShowingMrec;

    private UnityAction<AdState> _callback;
    private IEnumerator          _timeoutCoroutine;
    private const float          Timeout = 6f;
    private bool                 _finalStateReached;
    private bool                 _adsInitStarted;

    // ─────────────────────────────────────────────────────────────────────
    // Timer state
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Seconds remaining on the first-ad gate (session-start only).</summary>
    public float FirstIntervalRemaining { get; private set; }

    /// <summary>Seconds remaining on the regular between-ad cooldown.</summary>
    public float AdIntervalRemaining    { get; private set; }

    /// <summary>True once the first-ad gate has expired this session.</summary>
    public bool FirstIntervalExpired    { get; private set; }

    /// <summary>True when both timers are zero — interstitials are allowed.</summary>
    public bool CanShowInterstitial     => !useAdTimer || (FirstIntervalExpired && AdIntervalRemaining <= 0f);

    private float _firstAdInterval;
    private float _adsInterval;
    private bool  _timerConfigured;

    // ─────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────

    private void Start()
    {
        loadingADPanel.SetActive(false);
        bannerBgImg.SetActive(false);
    }

    private void OnEnable()
    {
        MonetizationEvents.OnRemoteConfigUpdated += OnRemoteConfigUpdated;
        MonetizationEvents.OnAdTimerReset        += ResetAdInterval;
    }

    private void OnDisable()
    {
        MonetizationEvents.OnRemoteConfigUpdated -= OnRemoteConfigUpdated;
        MonetizationEvents.OnAdTimerReset        -= ResetAdInterval;
    }

    private void Update()
    {
        if (!useAdTimer || !_timerConfigured) return;

        float dt = Time.unscaledDeltaTime;

        if (!FirstIntervalExpired)
        {
            FirstIntervalRemaining = Mathf.Max(0f, FirstIntervalRemaining - dt);
            if (FirstIntervalRemaining <= 0f)
            {
                FirstIntervalExpired = true;
                AdIntervalRemaining  = 0f;
                Debug.Log("[ADS] First-ad interval expired — interstitials unlocked");
                MonetizationEvents.OnFirstAdIntervalCompleted?.Invoke();
            }
            return;
        }

        if (AdIntervalRemaining > 0f)
            AdIntervalRemaining = Mathf.Max(0f, AdIntervalRemaining - dt);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────────────────────────────────────

    [Header("Init Delay")]
    [Tooltip("Seconds to wait after Remote Config is ready before configuring timers and initializing the ad SDK. Keeps both in sync since timer values themselves come from Remote Config.")]
    [SerializeField] private float adInitDelay = 1f;

    private void OnRemoteConfigUpdated()
    {
        StartCoroutine(InitAfterDelay(adInitDelay));
    }

    private IEnumerator InitAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        ConfigureTimers();
        if (HasEnoughMemoryAndroid()) InitAds();
    }

    private void ConfigureTimers()
    {
        if (!useAdTimer) return;

        _firstAdInterval = FbaManager.Instance.GetRemoteConfigValue<float>(FbaManager.RemoteConfigKeys.FirstAdInterval);
        _adsInterval     = FbaManager.Instance.GetRemoteConfigValue<float>(FbaManager.RemoteConfigKeys.AdsInterval);

        if (!_timerConfigured)
        {
            FirstIntervalRemaining = _firstAdInterval;
            AdIntervalRemaining    = 0f;
            FirstIntervalExpired   = _firstAdInterval <= 0f;
            _timerConfigured       = true;
            Debug.Log($"[ADS] Timer configured — First: {_firstAdInterval:F0}s | Interval: {_adsInterval:F0}s");
        }
    }

    private void InitAds()
    {
        if (_adsInitStarted) return;
        _adsInitStarted = true;

        _adProvider = adProvider switch
        {
            AdProvider.AppLovinWithAdMobAOA or AdProvider.AppLovinOnly =>
                new AppLovinProvider(maxSdkKey, maxInterstitialId, maxStaticInterstitialId,
                                     maxRewardedId, maxBannerId, maxMrecId),

#if ADMOB_SDK
            _ => new AdMobProvider(admobInterstitialId, admobStaticInterstitialId,
                                   admobRewardedId, admobBannerId, admobMrecId)
#else
            _ => null
#endif
        };

#if ADMOB_SDK
        if (adProvider is AdProvider.AdMobOnly or AdProvider.AppLovinWithAdMobAOA)
        {
            TryGetComponent<GoogleMobileAdsConsentController>(out var consent);
            consent?.GatherConsent(err =>
            {
                if (err != null) Debug.LogError($"[ADS] Consent error: {err}");
                else             Debug.Log("[ADS] Consent gathered");
            });
        }
#endif

        _adProvider.Initialize(() =>
        {
            AdsInitialized = true;
            Debug.Log($"[ADS] {adProvider} initialized");
            StartCoroutine(LoadAllAds());
        });
    }

    private IEnumerator LoadAllAds()
    {
        IAppOpenProvider aoaProvider = adProvider switch
        {
            AdProvider.AppLovinOnly => new AppLovinAppOpenProvider(maxAppOpenId),
#if ADMOB_SDK
            _                      => new AdMobAppOpenProvider(admobAppOpenId)
#else
            _                      => null
#endif
        };
        AppOpenAdManager.Instance.Init(aoaProvider);

        yield return null; AppOpenAdManager.Instance.Load();
        yield return null; _adProvider.LoadInterstitial();
        yield return null; _adProvider.LoadStaticInterstitial();
        yield return null; _adProvider.LoadRewarded();
        yield return null; _adProvider.LoadBanner();
        yield return null; _adProvider.LoadMrec();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Show a video interstitial.
    /// If useAdTimer is on, silently blocks while either interval is still counting.
    /// </summary>
    public void ShowInterstitial(UnityAction<AdState> cb, bool showPanel = true)
    {
        if (!CanShowInterstitial)
        {
            if (!FirstIntervalExpired)
                Debug.Log($"[ADS] Interstitial blocked — first-ad gate: {FirstIntervalRemaining:F1}s remaining");
            else
                Debug.Log($"[ADS] Interstitial blocked — ad interval: {AdIntervalRemaining:F1}s remaining");
            return;
        }
        ShowInternal(AdType.Interstitial, cb, showPanel);
    }

    /// <summary>
    /// Show a static (image) interstitial.
    /// If useAdTimer is on, silently blocks while either interval is still counting.
    /// </summary>
    public void ShowStaticInterstitial(UnityAction<AdState> cb, bool showPanel = true)
    {
        if (!CanShowInterstitial)
        {
            if (!FirstIntervalExpired)
                Debug.Log($"[ADS] Static interstitial blocked — first-ad gate: {FirstIntervalRemaining:F1}s remaining");
            else
                Debug.Log($"[ADS] Static interstitial blocked — ad interval: {AdIntervalRemaining:F1}s remaining");
            return;
        }
        ShowInternal(AdType.InterstitialStatic, cb, showPanel);
    }

    /// <summary>Show a rewarded ad. Never gated by the timer.</summary>
    public void ShowRewarded(UnityAction<AdState> cb, bool showPanel = true) =>
        ShowInternal(AdType.Rewarded, cb, showPanel);

    public void ShowBanner()
    {
        if (MonetizationData.RemoveAds) return;
        _adProvider?.ShowBanner();
        _showingBanner = true;
    }

    public void HideBanner()
    {
        _adProvider?.HideBanner();
        _showingBanner = false;
    }

    public void ShowMrec()
    {
        if (MonetizationData.RemoveAds) return;
        _adProvider?.ShowMrec();
        _mrecShowing = true;
    }

    public void HideMrec()
    {
        _adProvider?.HideMrec();
        _mrecShowing = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Core show logic
    // ─────────────────────────────────────────────────────────────────────

    private void ShowInternal(AdType type, UnityAction<AdState> userCb, bool showPanel)
    {
        if (!AdsInitialized || IsShowingAd)
        {
            userCb?.Invoke(AdState.Canceled);
            return;
        }

        _wasShowingBanner  = _showingBanner;
        _wasShowingMrec    = _mrecShowing;
        IsShowingAd        = true;
        _finalStateReached = false;

        HideBanner();
        HideMrec();

        _callback = state =>
        {
            userCb?.Invoke(state);
            if (state == AdState.Opening) Time.timeScale = 0;
            if (!_finalStateReached && state is AdState.Closed or AdState.Failed or AdState.Canceled)
            {
                _finalStateReached = true;
                HandleAdClose();
            }
        };

        if (showPanel)
        {
            loadingADPanel.SetActive(true);
            StartTimeout();
        }

        StartCoroutine(ShowNextFrame(type, _callback));
    }

    private IEnumerator ShowNextFrame(AdType type, UnityAction<AdState> cb)
    {
        yield return null;
        switch (type)
        {
            case AdType.Interstitial:       _adProvider.ShowInterstitial(cb);       break;
            case AdType.InterstitialStatic: _adProvider.ShowStaticInterstitial(cb); break;
            case AdType.Rewarded:           _adProvider.ShowRewarded(cb);           break;
        }
    }

    private void HandleAdClose()
    {
        Time.timeScale = 1;
        IsShowingAd    = false;
        StopTimeout();
        loadingADPanel.SetActive(false);
        MonetizationEvents.OnAdTimerReset?.Invoke();
        if (MonetizationData.RemoveAds) return;
        if (_wasShowingBanner) ShowBanner();
        if (_wasShowingMrec)   ShowMrec();
        _wasShowingBanner = _wasShowingMrec = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Timer
    // ─────────────────────────────────────────────────────────────────────

    private void ResetAdInterval()
    {
        if (!useAdTimer || !_timerConfigured) return;
        AdIntervalRemaining = _adsInterval;
        Debug.Log($"[ADS] Ad interval reset — {_adsInterval:F0}s cooldown");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Timeout
    // ─────────────────────────────────────────────────────────────────────

    private void StartTimeout()
    {
        if (_timeoutCoroutine != null) return;
        _timeoutCoroutine = RunTimeout();
        StartCoroutine(_timeoutCoroutine);
    }

    private void StopTimeout()
    {
        if (_timeoutCoroutine == null) return;
        StopCoroutine(_timeoutCoroutine);
        _timeoutCoroutine = null;
    }

    private IEnumerator RunTimeout()
    {
        yield return new WaitForSecondsRealtime(Timeout);
        if (!_finalStateReached) _callback?.Invoke(AdState.Canceled);
        _timeoutCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Memory guard
    // ─────────────────────────────────────────────────────────────────────

    private int _minMemoryMB = 400;

    private bool HasEnoughMemoryAndroid()
    {
        if (!FbaManager.Instance.GetRemoteConfigValue<bool>(FbaManager.RemoteConfigKeys.UseMemory)) return true;
        _minMemoryMB = FbaManager.Instance.GetRemoteConfigValue<int>(FbaManager.RemoteConfigKeys.MinMemory);

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var activityClass   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity        = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject manager = activity.Call<AndroidJavaObject>("getSystemService", "activity");
            if (manager == null) return HasEnoughMemoryGeneric();
            try
            {
                using var memInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                manager.Call("getMemoryInfo", memInfo);
                long  availMB = memInfo.Get<long>("availMem")  / (1024 * 1024);
                long  totalMB = memInfo.Get<long>("totalMem")  / (1024 * 1024);
                bool  lowMem  = memInfo.Get<bool>("lowMemory");
                float pct     = (float)availMB / totalMB * 100f;
                Debug.Log($"[Memory] Avail:{availMB}MB Total:{totalMB}MB Low:{lowMem}");
                return !lowMem && availMB >= _minMemoryMB && pct >= 20f;
            }
            finally { manager?.Dispose(); }
        }
        catch (Exception e) { Debug.LogWarning($"[Memory] Android check failed: {e.Message}"); return HasEnoughMemoryGeneric(); }
#else
        return HasEnoughMemoryGeneric();
#endif
    }

    private bool HasEnoughMemoryGeneric()
    {
        long free = SystemInfo.systemMemorySize
                    - UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        return free >= _minMemoryMB;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Shared enums
// ─────────────────────────────────────────────────────────────────────────────

public enum AdProvider
{
    /// <summary>AppLovin MAX for Banner/Inter/Rewarded. AdMob for App Open only.</summary>
    AppLovinWithAdMobAOA,
    /// <summary>AppLovin MAX for everything including App Open.</summary>
    AppLovinOnly,
    /// <summary>AdMob for everything including App Open.</summary>
    AdMobOnly
}

public enum AdType  { Interstitial, InterstitialStatic, Rewarded }
public enum AdState { Opening, Reward, Closed, Failed, Canceled, Loaded }
