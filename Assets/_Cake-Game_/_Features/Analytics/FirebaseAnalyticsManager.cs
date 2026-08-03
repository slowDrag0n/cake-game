using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Crashlytics;
using UnityEngine;

public class FirebaseAnalyticsManager : Singleton<FirebaseAnalyticsManager>
{
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    private bool isInitialized;

    private FirebaseApp app;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Initialize the Firebase Analytics
    void InitializeFirebase()
    {
        app = FirebaseApp.DefaultInstance;

        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        Crashlytics.ReportUncaughtExceptionsAsFatal = true;

        isInitialized = true;
    }

    // Log an event with no parameters
    public void LogEvent(string eventName)
    {
        if(isInitialized)
            FirebaseAnalytics.LogEvent(eventName);
    }

    // Log an event with one string parameter
    public void LogEvent(string eventName, string paramName, string paramValue)
    {
        if(isInitialized)
            FirebaseAnalytics.LogEvent(eventName,
            new Parameter(paramName, paramValue));
    }

    // Log a level start event
    public void LogLevelStart(int levelNum)
    {
        if(isInitialized)
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart,
                                    new Parameter(FirebaseAnalytics.ParameterLevel, levelNum.ToString()));
    }

    // Log a level end event
    public void LogLevelEnd(int levelNum, bool success)
    {
        if(isInitialized)
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd,
                                    new Parameter(FirebaseAnalytics.ParameterLevel, levelNum.ToString()),
                                    new Parameter(FirebaseAnalytics.ParameterSuccess, success.ToString()));
    }
}