using DG.Tweening;
using Lean.Touch;
using System;
using UnityEngine;

public class IcingCreamConeBehaviour : MonoBehaviour
{
    public SpriteRenderer ConeSprite;
    public SpriteRenderer IcingCreamSprite;
    public SpriteRenderer IcingCreamDripSprite;
    [Space]
    public Animator IcingAnimator;
    public string IcingPouringStateName = "PouringIcingCream";
    public float ProgressSpeed = .05f;
    public LeanFingerDown ConeFingerDown;
    public LeanFingerUp ConeFingerUp;
    [Space]
    public GameObject TutHand;

    public Action OnComplete;

    bool _isPainting;
    float _paintingProgress;



    public void Update()
    {
        if(_isPainting == false)
            return;

        _paintingProgress += Time.deltaTime * ProgressSpeed;
        _paintingProgress = Mathf.Clamp01(_paintingProgress);

        IcingAnimator.Play(IcingPouringStateName, 0, _paintingProgress);

        if(_paintingProgress == 1f)
        {
            ConeFingerDown.gameObject.SetActive(false);
            ConeFingerUp.gameObject.SetActive(false);

            _isPainting = false;

            IcingAnimator.SetTrigger("ConeMoveOut");

            DOVirtual.DelayedCall(.7f, delegate { OnComplete?.Invoke(); });
        }
    }



    internal void Init(Sprite spreaderSprite, Sprite upperLayerSprite, Sprite sideLayerSprite, Action onComplete)
    {
        ConeSprite.sprite = spreaderSprite;
        IcingCreamSprite.sprite = upperLayerSprite;
        IcingCreamDripSprite.sprite = sideLayerSprite;

        _isPainting = false;
        _paintingProgress = 0;

        TutHand.gameObject.SetActive(false);
        ConeFingerDown.gameObject.SetActive(false);
        DOVirtual.DelayedCall(1f, delegate
        {
            TutHand.gameObject.SetActive(true);
            ConeFingerDown.gameObject.SetActive(true);
        });

        OnComplete = onComplete;
    }

    public void ConeFingerDownHandler()
    {
        _isPainting = true;

        if(TutHand.gameObject.activeSelf)
            TutHand.gameObject.SetActive(false);
    }

    public void ConeFingerUpHandler()
    {
        _isPainting = false;
    }
}
