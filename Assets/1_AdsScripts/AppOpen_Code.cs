using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class AppOpen_Code : MonoBehaviour
{
    [Header("Ad Unit")]
    public string AppOpenId;

    [Header("Splash Scene Build Index")]
    public int splashSceneIndex = 0;

    private AppOpenAd appOpenAd;
    private bool isLoading;
    private bool isShowing;
    private bool startupRequest;
    private bool resumedFromBackground;

    private readonly TimeSpan timeout = TimeSpan.FromHours(4);
    private DateTime expireTime;

    #region Resume

    private void OnApplicationPause(bool paused)
    {
        if(!paused)
        {
            if(AdmobIntilization.Instance.isMobileAdsInitialized)
            {
                resumedFromBackground = true;

                ShowAppOpenAdIfAvailable();

                if(!IsAdAvailable())
                {
                    LoadOpenApp();
                } 
            }
        }
    }

    #endregion

    #region Startup

    IEnumerator StartSequence()
    {
        LoadOpenApp();

        float timer = 0f;

        while (timer < 5f)
        {
            if (IsAdAvailable())
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        // First launch -> Don't show App Open
        if (PlayerPrefs.GetInt("AppOpenFirstTime", 0) == 0)
        {
            PlayerPrefs.SetInt("AppOpenFirstTime", 1);
            startupRequest = false;
            yield break;
        }

        // Only show on Splash Scene
        if (SceneManager.GetActiveScene().buildIndex == splashSceneIndex)
        {
            ShowAppOpenAdIfAvailable();
        }

        startupRequest = false;
    }

    public void CallingAppOpen()
    {
        startupRequest = true;
        resumedFromBackground = false;

        StartCoroutine(StartSequence());
    }

    #endregion

    #region Load

    public void LoadOpenApp()
    {
        if (isLoading)
            return;

        if (appOpenAd != null && IsAdAvailable())
            return;

        isLoading = true;

        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        AdRequest request = new AdRequest();

        AppOpenAd.Load(AppOpenId, request,
            (AppOpenAd ad, LoadAdError error) =>
            {
                isLoading = false;

                if (error != null || ad == null)
                {
                    Debug.Log("App Open failed: " + error);
                    return;
                }

                appOpenAd = ad;
                expireTime = DateTime.Now + timeout;

                RegisterEvents(appOpenAd);

                Debug.Log("App Open Loaded");
            });
    }

    #endregion

    #region Show

    public void ShowAppOpenAdIfAvailable()
    {
        if (AdsManager.instance != null &&
            AdsManager.instance.isPausedDuetoAd)
            return;

        if (isShowing)
            return;

        if (!IsAdAvailable())
            return;

        // Startup App Open ONLY on Splash Scene
        if (startupRequest && !resumedFromBackground)
        {
            if (SceneManager.GetActiveScene().buildIndex != splashSceneIndex)
            {
                Debug.Log("Startup App Open cancelled because Splash finished.");
                return;
            }
        }

        isShowing = true;
        appOpenAd.Show();
    }

    #endregion

    #region Events

    void RegisterEvents(AppOpenAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App Open Opened");
            isShowing = true;
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App Open Closed");

            isShowing = false;
            resumedFromBackground = false;

            appOpenAd.Destroy();
            appOpenAd = null;

            LoadOpenApp();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.Log("App Open Failed To Show");

            isShowing = false;
            resumedFromBackground = false;

            appOpenAd.Destroy();
            appOpenAd = null;

            LoadOpenApp();
        };
    }

    #endregion

    #region Helpers

    bool IsAdAvailable()
    {
        return appOpenAd != null &&
               DateTime.Now < expireTime;
    }

    #endregion
}