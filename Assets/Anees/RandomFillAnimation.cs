using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RandomFillAnimation : MonoBehaviour
{
    public Image fillImage;  // Reference to the UI Image component
    //public float duration = 3f; // Total duration of the fill animation
    //public int speedChanges = 10; // Number of times speed changes randomly

    private void Awake()
    {
        if(fillImage == null)
            fillImage = GetComponent<Image>();
    }

    private void Start()
    {
        fillImage.fillAmount = 0f;
        var seq = DOTween.Sequence();
        seq.Append(fillImage.DOFillAmount(Random.Range(.15f, .25f), 2f))
           .Append(fillImage.DOFillAmount(Random.Range(.65f, .85f), .5f).SetDelay(Random.Range(.15f, .25f)))
           .Append(fillImage.DOFillAmount(1f, 6f).SetDelay(Random.Range(.15f, .25f)));
    }

}
