using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BarFiller : MonoBehaviour
{
    public float Duration = 3f; // Total duration of the fill animation

    Image fillImage;  // Reference to the UI Image component

    private void Awake()
    {
        if(fillImage == null)
            fillImage = GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
    }

    private void OnEnable()
    {
        fillImage.fillAmount = 0f;
        fillImage.DOFillAmount(1f, Duration).SetEase(Ease.Linear);
    }

    private void OnDisable()
    {
        fillImage.DOKill();
    }
}
