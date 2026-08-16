using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using Firebase.Extensions; // REQUIRED: For ContinueWithOnMainThread
using UnityEngine;

public class FirebaseHandler : MonoBehaviour
{
    public static FirebaseHandler Instance;

    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    // Remote Config Variables
    public static bool isArrangeByOrder = true;
    public static bool isBannerOn = true;
    public static bool isInterstitialOn = true;
    public static bool isMRecOn = true;
    public static bool isUserInActivity = true;

    public string jsonValue;
    public static int adTimer = 30;
    public static int userInactivityTimer = 20;
    public int[] orderLevels;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // FIX: Use ContinueWithOnMainThread instead of ContinueWith
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;

            if(dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Firebase Dependency Error : " + dependencyStatus);
            }
        });
    }

    async void InitializeFirebase()
    {
        Debug.Log("Firebase Initialized");

        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        FirebaseAnalytics.SetUserProperty(
            FirebaseAnalytics.UserPropertySignUpMethod,
            "Google");

        await InitializeRemoteConfig();
    }

    async Task InitializeRemoteConfig()
    {
        // Default values
        var defaults = new Dictionary<string, object>()
        {
            { "isArrangeByOrder_Remote", true },
            { "isBannerOn", false },
            { "isInterstitialOn", true },
            { "isMRecOn", true },
            { "isUserInActivity", true },

            { "jsonValue", "[19,13,2,3,4,5,6,7,8,9,10,11,12,1,14,15,16,17,18,0]" },

            { "TakeAdTimer_st", "30" },
            { "userInactivityTimer_st", "20" }
        };

        await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

        // Fetch every hour
        await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(1));

        await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

        LoadRemoteConfigValues();
    }

    void LoadRemoteConfigValues()
    {
        var config = FirebaseRemoteConfig.DefaultInstance;

        isArrangeByOrder = config.GetValue("isArrangeByOrder_Remote").BooleanValue;
        isBannerOn = config.GetValue("isBannerOn").BooleanValue;
        isInterstitialOn = config.GetValue("isInterstitialOn").BooleanValue;
        isMRecOn = config.GetValue("isMRecOn").BooleanValue;
        isUserInActivity = config.GetValue("isUserInActivity").BooleanValue;

        jsonValue = config.GetValue("jsonValue").StringValue;
        orderLevels = jsonValue
            .Trim('[', ']')
            .Split(',')
            .Select(int.Parse)
            .ToArray();

        adTimer = int.Parse(config.GetValue("TakeAdTimer_st").StringValue);
        userInactivityTimer = int.Parse(config.GetValue("userInactivityTimer_st").StringValue);

        Debug.Log("========= Remote Config =========");

        Debug.Log("Arrange By Order : " + isArrangeByOrder);
        Debug.Log("Banner : " + isBannerOn);
        Debug.Log("Interstitial : " + isInterstitialOn);
        Debug.Log("MRec : " + isMRecOn);
        Debug.Log("User Inactivity : " + isUserInActivity);

        Debug.Log("JSON : " + jsonValue);
        Debug.Log("Ad Timer : " + adTimer);
        Debug.Log("User Inactivity Timer : " + userInactivityTimer);
    }
}