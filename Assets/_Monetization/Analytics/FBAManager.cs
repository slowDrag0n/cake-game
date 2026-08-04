using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using SexyDevs.Utils;
using UnityEngine;

public class FbaManager : Singleton<FbaManager>
{
    public static class RemoteConfigKeys
    {
        // <------ Ads General ------>

        public const string AdsInterval = "ad_interval";
        public const string InterAdStart = "Inter_ad_start";
        public const string InterAdShow = "inter_ad_show";
        public const string InterAdTrigger = "inter_ad_trigger";
        public const string InterAdType = "inter_ad_type";
        public const string LoadingADScreen = "ad_loading";
        public const string GameplayAds = "afk_interval";
        public const string LargeBanner = "ban_large";
        public const string FirstAdInterval = "first_ad_interval";


        //<------ App Open AD ------>
        public const string AppOpen = "app_open_ad";
        public const string AppOpenSession = "app_open_ses";
        public const string AppOpenType = "app_open_type";

        //<------ Resume AD ------>
        public const string ResumeAds = "resume_ad";
        public const string ResumeAdType = "resume_ad_type";


        // <------ Remove Ads / Monetization ------>

        public const string RemoveAdPopup = "remove_ad_popup";
        public const string RemoveAdLevel = "remove_ad_level";

        // <------ Rating & Engagement ------>

        public const string RateUs = "rate_us";
        public const string RateUsLevel = "rate_us_level";
        public const string PushNotification = "push_notification";

        // <------ Device & Performance ------>

        public const string LowEndDevice = "low_end_device";

        // <------ No Internet ------>

        public const string NoInternetPopup = "no_net_popup";
        public const string NoInternetPopupLevel = "no_net_pos";

        // <------ IAP ------>

        public const string IAPSubscriptionPopup = "iap_subscription";


        //<----- Gameplay ----->
        public const string UtmLink = "utm_link";
        public const string UtmPopup = "utm_popup";
        public const string UtmPopupPos = "utm_popup_pos";
        public const string SpearBtn = "spear_btn";
        public const string ProgressionTimer = "progression_timer";


        //<----- Memory Remote Config ----->
        public const string UseMemory = "CanUseMemoryCheck";
        public const string MinMemory = "MinMemoryCheck";
    }


    private Dictionary<string, object> remoteConfigValues = new()
    {
        { RemoteConfigKeys.AdsInterval, 30f },
        { RemoteConfigKeys.NoInternetPopup, false },
        { RemoteConfigKeys.InterAdStart, 1 },
        { RemoteConfigKeys.InterAdShow, true },
        { RemoteConfigKeys.InterAdTrigger, true },
        { RemoteConfigKeys.RemoveAdPopup, false },
        { RemoteConfigKeys.RateUs, true },
        { RemoteConfigKeys.InterAdType, false },
        { RemoteConfigKeys.RemoveAdLevel, 7 },
        { RemoteConfigKeys.LowEndDevice, 2 },
        { RemoteConfigKeys.LoadingADScreen, false },
        { RemoteConfigKeys.NoInternetPopupLevel, 5 },
        { RemoteConfigKeys.IAPSubscriptionPopup, true },
        { RemoteConfigKeys.ResumeAds, true },
        { RemoteConfigKeys.AppOpenSession, false },
        { RemoteConfigKeys.PushNotification, true },
        { RemoteConfigKeys.GameplayAds, 15f },
        { RemoteConfigKeys.RateUsLevel, 20 },
        { RemoteConfigKeys.AppOpenType, false },
        { RemoteConfigKeys.ResumeAdType, true },
        { RemoteConfigKeys.UtmLink, "https://play.google.com/store/apps/developers?id=Terafort+IEG" },
        { RemoteConfigKeys.UtmPopup, false },
        { RemoteConfigKeys.UtmPopupPos, 30 },
        { RemoteConfigKeys.SpearBtn, true },
        { RemoteConfigKeys.AppOpen, true },
        { RemoteConfigKeys.LargeBanner, true },
        { RemoteConfigKeys.FirstAdInterval, 60f },
        { RemoteConfigKeys.ProgressionTimer, false },
        { RemoteConfigKeys.MinMemory, 400 },
        { RemoteConfigKeys.UseMemory, true }
    };

    public bool IsRemoteConfigReady { get; private set; }

    private new void Awake()
    {
        base.Awake();
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        InitFirebase();
        yield return new WaitForEndOfFrame();
    }

    private void InitFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                Debug.Log("✅ Firebase Initialized");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }

            if (task.IsCompletedSuccessfully)
            {
                InitRemoteConfig();
            }
        });
    }

    private void InitRemoteConfig()
    {
        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(remoteConfigValues).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Remote Config default values set.");
            if (task.IsCompletedSuccessfully)
            {
                FetchRemoteConfig();
            }
        });
    }

    private void PrintAllRemoteConfigValues()
    {
        foreach (var kvp in remoteConfigValues)
        {
            Debug.Log($"➡️ {kvp.Key} = {kvp.Value}");
        }
    }

    private void FetchRemoteConfig()
    {
        FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Remote Config fetch successful.");
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    if (activateTask.IsCompletedSuccessfully)
                    {
                        Debug.Log("Remote Config activated.");
                        GetRemoteConfigValues();
                    }
                });
            }
            else
            {
                Debug.LogError("Remote Config fetch failed.");
            }
        });
    }

    private void OnDisable()
    {
        GameEvents.OnLevelStart -= HandleLevelStart;
        GameEvents.OnLevelComplete -= HandleOnLevelComplete;
        GameEvents.OnLevelFailed -= HandleOnLevelFailed;
    }

    private void OnEnable()
    {
        GameEvents.OnLevelStart += HandleLevelStart;
        GameEvents.OnLevelComplete += HandleOnLevelComplete;
        GameEvents.OnLevelFailed += HandleOnLevelFailed;
    }

    private void HandleOnLevelFailed()
    {
        SendLevelStartEvent("level_fail_", MonetizationData.CurrentLevel + 1);
    }

    private void HandleOnLevelComplete()
    {
        SendLevelCompleteEvent("level_complete_", MonetizationData.CurrentLevel + 1);
    }

    private void HandleLevelStart()
    {
        SendLevelStartEvent("level_start_", MonetizationData.CurrentLevel + 1);
    }

    public void SendCustomEvent(string eventName)
    {
        try
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void SendLevelStartEvent(string eventName, int level)
    {
        try
        {
            FirebaseAnalytics.LogEvent(eventName + level);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void SendLevelCompleteEvent(string eventName, int level)
    {
        try
        {
            FirebaseAnalytics.LogEvent(eventName + level);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void GetRemoteConfigValues()
    {
        Debug.Log("Remote Config values Before:");
        PrintAllRemoteConfigValues();
        var rc = FirebaseRemoteConfig.DefaultInstance;

        foreach (var key in remoteConfigValues.Keys.ToList())
        {
            var defaultValue = remoteConfigValues[key];
            var value = rc.GetValue(key);

            switch (defaultValue)
            {
                case bool:
                    remoteConfigValues[key] = value.BooleanValue;
                    break;
                case int:
                    remoteConfigValues[key] = (int)value.LongValue;
                    break;
                case float:
                    remoteConfigValues[key] = (float)value.DoubleValue;
                    break;
                case double:
                    remoteConfigValues[key] = value.DoubleValue;
                    break;
                case string:
                    remoteConfigValues[key] = value.StringValue;
                    break;
            }
        }

        IsRemoteConfigReady = true;
        MonetizationEvents.OnRemoteConfigUpdated?.Invoke();
        Debug.Log("Remote Config values After:");
        PrintAllRemoteConfigValues();
    }


    private void CacheBool(string key)
    {
        remoteConfigValues[key] = FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
    }

    private void CacheDouble(string key)
    {
        remoteConfigValues[key] = FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
    }

    public T GetRemoteConfigValue<T>(string key)
    {
        if (remoteConfigValues.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                    return typedValue;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to convert key '{key}' to {typeof(T)}: {e.Message}");
            }
        }

        Debug.LogWarning($"Remote Config value for key '{key}' not found, using default.");
        return default;
    }
}