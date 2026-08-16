using UnityEngine;
using GameAnalyticsSDK;
using Firebase.Analytics;
public class Analytics : MonoBehaviour
{
    public static Analytics instance;
    private void Awake()
    {
        instance = this;
        
    }
    private void Start()
    {
        GameAnalytics.Initialize();
    }
    public void StartGAprogression(string st,int lvlno)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, st , "LvlNo"+lvlno.ToString());
    }
    public void CompleteGAprogression(string st, int lvlno)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, st, "LvlNo" + lvlno.ToString());
    }
    public static void LevelStart(string lvl, int level)
    {
        GameAnalytics.NewDesignEvent($"Level:Start:{lvl}:{level}");

        Debug.Log($"LevelStart : {lvl} {level}");
    }

    public static void LevelEnd(string lvl, int level)
    {
        GameAnalytics.NewDesignEvent($"Level:Complete:{lvl}:{level}");

        Debug.Log($"LevelEnd : {lvl} {level}, Success");
    }

    public static void SketchTask(string lvl, int level, bool isStart)
    {
        if (isStart)
            GameAnalytics.NewDesignEvent($"Sketch:Start:{lvl}:{level}");
        else
            GameAnalytics.NewDesignEvent($"Sketch:End:{lvl}:{level}");

        Debug.Log($"Sketch {(isStart ? "Start" : "End")} : {lvl} {level}");
    }

    public static void ColorTask(string lvl, int level, bool isStart)
    {
        if (isStart)
            GameAnalytics.NewDesignEvent($"Color:Start:{lvl}:{level}");
        else
            GameAnalytics.NewDesignEvent($"Color:End:{lvl}:{level}");

        Debug.Log($"Color {(isStart ? "Start" : "End")} : {lvl} {level}");
    }
}
