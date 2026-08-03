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

        LoadingBarFill.fillAmount = 0;

        var seq = DOTween.Sequence();
        seq.Append(LoadingBarFill.DOFillAmount(Random.Range(.15f, .25f), 2f))
           .Append(LoadingBarFill.DOFillAmount(Random.Range(.65f, .85f), .5f).SetDelay(Random.Range(.15f, .25f)))
           .Append(LoadingBarFill.DOFillAmount(1f, 6f).SetDelay(Random.Range(.15f, .25f)));
    }
}
