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
        // Manual loading when not requiring user consent for ads
        if(WaitForConsent == false)
        {
            AdmobIntilization.Instance.InitializeAdmob();
            StartLoading();
        }
    }

    public void StartLoading()
    {
        StartCoroutine(LoadGameCo());

        //if(Profile.FirstTimeFlag)
        //{
        //    Profile.FirstTimeFlag = false;
        //    //AdsManager.Ins.ShowBannerAdAfterInitDelay();
        //    return;
        //}

        //AdsManager.Ins.ShowBannerAdAfterInitDelay();
        //AdsManager.Ins.ShowBigBannerAdAfterInitDelay();

    }



    IEnumerator LoadGameCo()
    {
        EventManager.DoFireShowUiEvent(UiType.Loading);

        yield return new WaitForSecondsRealtime(LoadDelay);

        SceneManager.LoadScene("Gameplay");

        EventManager.DoFireHideUiEvent(UiType.Loading);
    }
}
