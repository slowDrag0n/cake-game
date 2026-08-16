using GameAnalyticsSDK;
using UnityEngine;

public static class Statics 
{
 
    #region GameAnalytics Events


    /// <summary>
    /// Ad Events
    /// </summary>
    public static void GA_InterstitialEventMaxSDK()
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        GameAnalytics.NewDesignEvent("Interstitial_Seen:MaxSDK");
        Debug.Log("Interstitial_Seen:MaxSDK");
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, "MAX", "Intersitial");
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("Ads", "Interstitials");
        //}
    }
 
    public static void GA_RewardedEventMaxSDK()
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        GameAnalytics.NewDesignEvent("Rewarded_Seen:MaxSDK");
        Debug.Log("Rewarded_Seen:MaxSDK");
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, "MAX", "Rewarded");
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("Ads", "Rewarded");
        //}
    }
 

    
    /// <summary>
    /// Hint Events
    /// </summary>
    /// <param name="level"></param>
    public static void GA_Skip_Level(int level)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        GameAnalytics.NewDesignEvent("TakeReward:Skip:Level" + level);
        Debug.Log("Hints:PullPin:Level" + level);
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "MAX", "Skip:Level" + level);
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("RewardedRecieved", "Skip", "Level" + level);
        //}
    }
    public static void GA_Hint_Level(int level)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        GameAnalytics.NewDesignEvent("TakeReward:Hint:Level" + level);
        Debug.Log("Hints:DrawBridge:Level" + level);
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "MAX", "Hint:Level" + level);
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("RewardedRecieved", "Hint", "Level" + level);
        //}
    }
    public static void GA_Hands()
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        GameAnalytics.NewDesignEvent("TakeReward:HandFromShop");
        Debug.Log("Hints:ToiletRush:Level");
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "MAX", "Hand");
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("RewardedRecieved", "Hand");
        //}
    }

    public static void LevelCompleteTimeEvent(int Level, int Time)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

   
        GameAnalytics.NewDesignEvent("LevelCompleteTime:Level" + Level.ToString() + ":" + Time.ToString());
        Debug.Log("CompleteTimeEvent:" + ":Level " + Level + " , " + Time.ToString());
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("LevelTime", "Level" + Level.ToString() ,  Time.ToString());
        //    Debug.Log("Appmetrica >> " + "LevelCompleteTime >> " + "Level" + Level.ToString() + " , " + Time.ToString());
        //}
    }

    /// <summary>
    /// Progression Events
    /// </summary>
    /// <param name="log"></param>
    public static void GA_LevelStartEvent(int Level)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

   

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "ThiefPuzzle", "Level" + Level);
        Debug.Log("StartEvent:" + "ThiefPuzzle" + ":Level " + Level);
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("LevelStart", Level.ToString());
        //    Debug.Log("Appmetrica >> " + "LevelStart >> " + Level.ToString());
        //}
    }
    public static void GA_LevelCompleteEvent(int Level)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

 
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "ThiefPuzzle", "Level" + Level);
        Debug.Log("CompleteEvent:" + "ThiefPuzzle" + ":Level " + Level);


        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("LevelComplete", Level.ToString());
        //    Debug.Log("Appmetrica >> " + "LevelComplete >> " + Level.ToString());
        //}
    }
    public static void GA_LevelFailedEvent(int Level)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

 
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "ThiefPuzzle", "Level" + Level);
        //Debug.Log("FailedEvent:" + "ThiefPuzzle" + ":Level " + Level);
        //if (AMAnalytics.inst != null)
        //{
        //    AMAnalytics.inst.ReportEvent("LevelFail", Level.ToString());
        //    Debug.Log("Appmetrica >> " + "LevelFail >> " + Level.ToString());
        //}
    }


    #endregion

}
