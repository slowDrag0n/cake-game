using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RewardAdButton : MonoBehaviour
{
    public bool TEST_MODE;

    public UnityEvent OnRewardSuccess;

    Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _btn.onClick.AddListener(OnClickRewardBtn);
    }

    private void OnDisable()
    {
        _btn.onClick.RemoveAllListeners();
    }

    void OnClickRewardBtn()
    {
        if(TEST_MODE)
        {
            OnRewardSuccess?.Invoke();
            return;
        }

        // TODO - Reward ad here calls OnRewardSuccess on completing ad
        AdsManager.instance.ShowRewardedAd(delegate { OnRewardSuccess?.Invoke(); });

        //AdsManager.Ins?.ShowRewardAd(() =>
        //{
        //    OnRewardSuccess?.Invoke();
        //});
    }
}
