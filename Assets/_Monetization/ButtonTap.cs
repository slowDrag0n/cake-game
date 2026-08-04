using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

enum ButtonType
{
    Generic,
    InApp,
    Back,
    Custom
}

[RequireComponent(typeof(Button))]
public class ButtonTap : MonoBehaviour
{
    protected Button _button;
    [SerializeField] bool ResetButtonInteractionOnClick = true;
    [SerializeField] private UnityEvent ButtonAction;
    [SerializeField] ButtonType ButtonType = ButtonType.Generic;
    [SerializeField] private bool DelayButtonReset = false;
    private Vector3 _cachedScale;


    private void Awake()
    {
        _button = GetComponent<Button>();
        _cachedScale = transform.localScale;
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (ResetButtonInteractionOnClick && ButtonType != ButtonType.Custom)
        {
            CoroutineRunner.Instance.WaitForTimeDelayAndExecute(
                () => _button.interactable = true,
                DelayButtonReset ? 2f : 0.2f
            );
        }

        ButtonAction.Invoke();
    }
}