using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 59;
    }

    private void Start()
    {
        StartLoading();
    }

    public void StartLoading()
    {
        StartCoroutine(LoadGameCo());

        //if(Profile.FirstTimeFlag)
        //{
        //    StartCoroutine(ShowBigBannerCo());
        //    Profile.FirstTimeFlag = false;
        //    return;
        //}

        //StartCoroutine(ShowAppopenAdCo());
    }

    IEnumerator LoadGameCo()
    {
        EventManager.DoFireShowUiEvent(UiType.Loading);

        yield return new WaitForSecondsRealtime(1f);

        SceneManager.LoadScene("Gameplay");

        //AdsManager.Ins.HideBigBannerAd();
        EventManager.DoFireHideUiEvent(UiType.Loading);
    }

    IEnumerator ShowAppopenAdCo()
    {
        yield return new WaitForSecondsRealtime(7f);
        //AdsManager.Ins.ShowAppOpenAd();
    }

    IEnumerator ShowBigBannerCo()
    {
        yield return new WaitForSecondsRealtime(6f);
        //AdsManager.Ins.ShowBigBannerAd();
    }
}
