using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    private bool isInitialized;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Initialize the Firebase Analytics
    void InitializeFirebase()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isInitialized = true;
    }

    // Log an event with no parameters
    public void LogEvent(string eventName)
    {
        FirebaseAnalytics.LogEvent(eventName);
    }

    // Log an event with one string parameter
    public void LogEvent(string eventName, string paramName, string paramValue)
    {
        FirebaseAnalytics.LogEvent(eventName,
            new Parameter(paramName, paramValue));
    }

    // Log a level start event
    public void LogLevelStart(int levelNum)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart,
                                    new Parameter(FirebaseAnalytics.ParameterLevel, levelNum.ToString()));
    }

    // Log a level end event
    public void LogLevelEnd(int levelNum, bool success)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd,
                                    new Parameter(FirebaseAnalytics.ParameterLevel, levelNum.ToString()),
                                    new Parameter(FirebaseAnalytics.ParameterSuccess, success.ToString()));
    }
}
