using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsButton : MonoBehaviour
{
    Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClickSettingsBtn);
    }

    private void OnClickSettingsBtn()
    {
        EventManager.DoFireShowUiEvent(UiType.Setting);
    }
}
