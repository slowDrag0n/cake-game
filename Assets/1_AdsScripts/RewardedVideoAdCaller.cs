using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RewardedVideoAdCaller : MonoBehaviour
{
    public static bool isRewardAd = false;
    public Action rewardEvent;

    public void WatchRewardedVideo()
    {
        if (PlayerPrefs.GetInt("MaxAdStop") == 0)
        {
            if (AdsManager.instance != null)
                AdsManager.instance.ShowRewardedAd(VideoWatches);
        }
        else
        {
            if (AdmobIntilization._instance != null)
                AdmobIntilization._instance.ShowRewardAd(VideoWatches);
        }
    }
    public void CallRewrdedAdmobForTest()
    {
        if (AdmobIntilization._instance != null)
            AdmobIntilization._instance.ShowRewardAd(VideoWatches);
    }   
    public void CallRewrdedMaxForTest()
    {
        if (AdsManager.instance != null)
            AdsManager.instance.ShowRewardedAdMax(VideoWatches);
    }
    public void VideoWatches()
    {
        Debug.Log("Rewarded");
    }


}

