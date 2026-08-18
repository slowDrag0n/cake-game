using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : UiPanel
{
    public GameObject FirstTimePanel;
    public GameObject RegularPanel;
    public Image LoadingBarFill;

    private void OnEnable()
    {
        //if(!Profile.FirstTimeFlag)
            //AdsManager.Ins.ShowBigBannerAdAfterInitDelay();
    }

    private void OnDisable()
    {
        //if(!Profile.FirstTimeFlag)
            //AdsManager.Ins.HideBigBannerAd();
    }

    private void Start()
    {
        FirstTimePanel.SetActive(Profile.FirstTimeFlag);
        RegularPanel.SetActive(!Profile.FirstTimeFlag);
    }
}
