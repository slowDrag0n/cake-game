using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuickAdBreak : MonoBehaviour
{
    public GameObject QuickBreakPanel;
    [Header("Auto Run Options")]
    public bool AutoRunEnabled = false;
    public float DelayBeforeNextAd = 15.0f;
    public float AdLoadingDelay = 3f;

    private DateTime lastCallbackTime;
    bool adShown = false;
    bool isInitialTimerDone = false;

    private void Start()
    {
        if(AutoRunEnabled == false)
            return;

        // Set the initial callback time
        lastCallbackTime = DateTime.Now;

        QuickBreakPanel.SetActive(false);

        Invoke("TurnOnAdTimer", 45f);
    }

    private void Update()
    {
        if(AutoRunEnabled == false)
            return;

        if(Profile.Level == 0)
            return;

        if(isInitialTimerDone == false)
            return;

        // Check if the delay has passed since the last callback
        DateTime currentTime = DateTime.Now;
        TimeSpan timeSinceLastCallback = currentTime - lastCallbackTime;

        if(timeSinceLastCallback.TotalSeconds >= DelayBeforeNextAd && adShown == false)
        {
            // Trigger the callback
            Callback();
        }
    }

    public void RunQuickAd()
    {
        StartCoroutine(ShowInterstitialAfterDelayCo());
    }

    private void Callback()
    {
        if(AdsManager.Ins.IsIntAdLoaded == false || AdsManager.Ins.IsInAd)
        {
            ResetTimer();
            return;
        }

        // Your callback function logic goes here
        adShown = true;
        StartCoroutine(ShowInterstitialAfterDelayCo());
    }

    IEnumerator ShowInterstitialAfterDelayCo()
    {
        QuickBreakPanel.SetActive(true);

        yield return new WaitForSeconds(AdLoadingDelay);

        QuickBreakPanel.SetActive(false);
        AdsManager.Ins.ShowInterstitialAd(delegate { ResetTimer(); });
    }

    public void ResetTimer()
    {
        Debug.Log("Done");
        // Update the last callback time
        lastCallbackTime = DateTime.Now;

        adShown = false;
    }

    void TurnOnAdTimer()
    {
        isInitialTimerDone = true;
    }
}
