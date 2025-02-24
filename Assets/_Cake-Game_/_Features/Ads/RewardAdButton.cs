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
#if !UNITY_EDITOR
        TEST_MODE = false;
#endif
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnClickRewardBtn);

    }

    void OnClickRewardBtn()
    {
        if(TEST_MODE) { OnRewardSuccess?.Invoke(); return; }

        AdsManager.Ins?.ShowRewardAd(() =>
        {
            OnRewardSuccess?.Invoke();
        });
    }
}
