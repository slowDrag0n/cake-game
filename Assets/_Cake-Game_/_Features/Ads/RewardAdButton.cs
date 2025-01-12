using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RewardAdButton : MonoBehaviour
{
    public UnityEvent OnRewardSuccess;

    Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnClickRewardBtn);
    }

    void OnClickRewardBtn()
    {
        AdsManager.Ins.ShowRewardAd(() =>
        {
            OnRewardSuccess?.Invoke();
        });
    }
}
