using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public bool WaitForConsent = false;
    public float LoadDelay = 3f;
    [Header("Appopen Ad Options")]
    public bool AppopenAdEnabled = false;
    public float AppopenDelay = 7f;

    private void Awake()
    {
        Application.targetFrameRate = 59;
    }

    private void Start()
    {
        if(WaitForConsent == false)
            StartLoading();
    }

    public void StartLoading()
    {
        StartCoroutine(LoadGameCo());

        if(AppopenAdEnabled == false)
            return;

        if(Profile.FirstTimeFlag)
        {
            StartCoroutine(ShowBigBannerCo());
            Profile.FirstTimeFlag = false;
            return;
        }

        StartCoroutine(ShowAppopenAdCo());
    }



    IEnumerator LoadGameCo()
    {
        EventManager.DoFireShowUiEvent(UiType.Loading);

        yield return new WaitForSecondsRealtime(LoadDelay);

        SceneManager.LoadScene("Gameplay");

        //AdsManager.Ins.HideBigBannerAd();
        EventManager.DoFireHideUiEvent(UiType.Loading);
    }

    IEnumerator ShowAppopenAdCo()
    {
        yield return new WaitForSecondsRealtime(AppopenDelay);
        //AdsManager.Ins.ShowAppOpenAd();
    }

    IEnumerator ShowBigBannerCo()
    {
        yield return new WaitForSecondsRealtime(AppopenDelay - 1f);
        //AdsManager.Ins.ShowBigBannerAd();
    }
}
