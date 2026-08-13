using System;
using GameAnalyticsSDK;
using UnityEngine;

public class GAManager : Singleton<GAManager>
{
    const string LevelProgression = "Level";

    public bool IsInitialized { get; private set; }

    public event Action<bool> OnInitialized;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        GameAnalytics.onInitialize += HandleGameAnalyticsInitialized;
        GameAnalytics.Initialize();
    }

    protected override void OnDestroy()
    {
        GameAnalytics.onInitialize -= HandleGameAnalyticsInitialized;
        base.OnDestroy();
    }

    void HandleGameAnalyticsInitialized(object sender, bool success)
    {
        IsInitialized = success;

        if(!success)
            Debug.LogWarning("GAManager: GameAnalytics failed to initialize.");

        OnInitialized?.Invoke(success);
    }

    bool CanLog()
    {
        if(IsInitialized)
            return true;

        Debug.LogWarning("GAManager: GameAnalytics is not initialized yet.");
        return false;
    }

    public void LogEvent(string eventName)
    {
        GameAnalytics.NewDesignEvent(eventName);
    }

    public void LogEvent(string eventName, string paramName, string paramValue)
    {
        string designEvent = $"{eventName}:{paramName}";

        if(float.TryParse(paramValue, out float numericValue))
            GameAnalytics.NewDesignEvent(designEvent, numericValue);
        else
            GameAnalytics.NewDesignEvent($"{designEvent}:{paramValue}");
    }

    public void LogLevelStart(int levelNum)
    {
        GameAnalytics.NewProgressionEvent(
            GAProgressionStatus.Start,
            $"Level_{levelNum.ToString()}");
    }

    public void LogLevelEnd(int levelNum, bool success)
    {
        GameAnalytics.NewProgressionEvent(
            success ? GAProgressionStatus.Complete : GAProgressionStatus.Fail,
            $"Level_{levelNum.ToString()}");
    }

    public void LogDesignEvent(string eventName, float eventValue = 0f)
    {
        if(eventValue != 0f)
            GameAnalytics.NewDesignEvent(eventName, eventValue);
        else
            GameAnalytics.NewDesignEvent(eventName);
    }

    public void LogProgressionEvent(GAProgressionStatus status,
        string progression01,
        string progression02 = null,
        string progression03 = null)
    {
        if(progression03 != null)
            GameAnalytics.NewProgressionEvent(status, progression01, progression02, progression03);
        else if(progression02 != null)
            GameAnalytics.NewProgressionEvent(status, progression01, progression02);
        else
            GameAnalytics.NewProgressionEvent(status, progression01);
    }

    public void LogResourceEvent(
        GAResourceFlowType flowType,
        string currency,
        float amount,
        string itemType,
        string itemId)
    {
        GameAnalytics.NewResourceEvent(flowType, currency, amount, itemType, itemId);
    }

    public void LogErrorEvent(GAErrorSeverity severity, string message)
    {
        GameAnalytics.NewErrorEvent(severity, message);
    }

    public void LogAdEvent(
        GAAdAction adAction,
        GAAdType adType,
        string adSdkName,
        string adPlacement)
    {
        GameAnalytics.NewAdEvent(adAction, adType, adSdkName, adPlacement);
    }

    public void LogBusinessEvent(
        string currency,
        int amountCents,
        string itemType,
        string itemId,
        string cartType)
    {
        GameAnalytics.NewBusinessEvent(currency, amountCents, itemType, itemId, cartType);
    }
}
