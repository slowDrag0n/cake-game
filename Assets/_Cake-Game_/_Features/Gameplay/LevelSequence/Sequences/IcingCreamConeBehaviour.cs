using DG.Tweening;
using GameAnalyticsSDK;
using Lean.Touch;
using System;
using Unity.VisualScripting;
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

    public Action OnComplete;

    bool _isPainting;
    float _paintingProgress;

    bool hasLoggedConeAnalytics = false;

    private void OnEnable()
    {
        var coneAudioSource = GetComponentInChildren<AudioSource>(true);
        coneAudioSource?.Stop();
    }

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

        ConeFingerDown.gameObject.SetActive(false);
        DOVirtual.DelayedCall(1f, delegate
        {
            ConeFingerDown.gameObject.SetActive(true);
        });

        OnComplete = onComplete;

        //Reset analytics event logging
        hasLoggedConeAnalytics = false;
        OnComplete += delegate
        {
            var levelName = EventManager.DoFireGetLevelName();
            var seq = GetComponentInParent<LevelSequence>();
            var eventString = $"{seq.SequenceId}_CakeConeCompleted";
            //Debug.Log($" >>>>>>>>> Log GA Progression Status - Complete, {levelName}:{eventString}");
            GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete, levelName, eventString);
        };
    }

    public void ConeFingerDownHandler()
    {
        _isPainting = true;

        // log analytics event for icing paint only once per level sequence
        if(!hasLoggedConeAnalytics)
        {
            var levelName = EventManager.DoFireGetLevelName();
            var seq = GetComponentInParent<LevelSequence>();
            var eventString = $"{seq.SequenceId}_CakeConeStarted";
            //Debug.Log($" >>>>>>>>> Log GA Progression Status - Start, {levelName}:{eventString}");
            GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Start, levelName, eventString);

            hasLoggedConeAnalytics = true;
        }
    }

    public void ConeFingerUpHandler()
    {
        _isPainting = false;
    }
}
