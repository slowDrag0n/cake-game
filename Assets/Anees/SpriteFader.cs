using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpriteFadeIn : MonoBehaviour
{
    public SpriteRenderer SpriteToFade;
    public float FadeToValue = 1f;
    public float FadeDuration = 1f;
    public float FadeDelay = 0f;

    void Awake()
    {
        if (SpriteToFade == null)
            SpriteToFade = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (SpriteToFade == null)
            return;

        SpriteToFade.DOFade(0f, 0f);
        SpriteToFade.DOFade(FadeToValue, FadeDuration).SetDelay(FadeDelay);
    }
}
