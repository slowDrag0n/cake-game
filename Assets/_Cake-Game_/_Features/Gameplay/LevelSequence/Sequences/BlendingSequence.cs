using DG.Tweening;
using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendingSequence : LevelSequence
{
    [Header("Hint Hand")]
    public GameObject TutHand;
    public GameObject SliceDropTut;
    public Transform TutPosForOpenLid;
    public Transform TutPosForGreen;
    [Space]
    public GameObject FingerDownHandler;
    public Animator JuicerAnimator;
    public GameObject JuicerContent;
    public SliceDropper DropperHand;
    public Animator MilkAnimator;
    public Animator SugarAnimator;
    public GameObject CompletionVfx;



    Action       _fingerDownCurrentAction;
    Action<bool> _fingerDownCurrentBoolAction;
    Action       _fingerUpCurrentAction;
    Action<bool> _fingerUpCurrentBoolAction;

    private void Start()
    {
        SequenceOpening();
    }

    private void ResetFingerActions()
    {
        _fingerDownCurrentAction = null;
        _fingerDownCurrentBoolAction = null;
        _fingerUpCurrentAction = null;
        _fingerUpCurrentBoolAction = null;
    }

    void SequenceOpening()
    {
        ResetFingerActions();

        FingerDownHandler.SetActive(false);
        JuicerContent.SetActive(false);
        DropperHand.gameObject.SetActive(false);
        TutHand.gameObject.SetActive(false);
        MilkAnimator.gameObject.SetActive(false);
        SugarAnimator.gameObject.SetActive(false);

        JuicerAnimator.transform.position += 15 * Vector3.right;
        JuicerAnimator.transform.DOMoveX(0.22f, .89f).SetEase(Ease.OutBack)
            .OnComplete(delegate
            {
                //TutHand.transform.position = TutPosForOpenLid.position;
                TutHand.SetActive(true);

                _fingerDownCurrentAction = OnOpenLid;
                FingerDownHandler.SetActive(true);
            });
    }



    #region JUG

    void OnOpenLid()
    {
        ResetFingerActions();

        JuicerAnimator.SetTrigger("OpenLid");
        TutHand.SetActive(false);

        DOVirtual.DelayedCall(1f, delegate { StartDroppingFruit(); });
    }

    public void StartDroppingFruit()
    {
        ResetFingerActions();

        DropperHand.gameObject.SetActive(true);
        DropperHand.Mover.enabled = false;
        var handPos = DropperHand.transform.position.y;
        DropperHand.transform.position += 9f * Vector3.up;
        DropperHand.transform.DOMoveY(handPos, 1f).SetEase(Ease.OutBack)
            .OnComplete(delegate
            {
                DropperHand.Initialize(OnDoneDroppingSlices);

                _fingerDownCurrentBoolAction = SwitchSliceDroppingFlag;
                _fingerUpCurrentBoolAction = SwitchSliceDroppingFlag;

                DropperHand.Mover.enabled = true;

                SliceDropTut.gameObject.SetActive(true);

                // Log GA Event
                var levelName = EventManager.DoFireGetLevelName();
                var eventString = $"{SequenceId}_DropFruitStarted";
                //Debug.Log($" >>>>>>>>> Log GA Progression Status - Start, {levelName}:{eventString}");
                GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Start, levelName, eventString);
            });
    }

    void SwitchSliceDroppingFlag(bool flag)
    {
        DropperHand.IsDropping = flag;
    }

    void OnDoneDroppingSlices()
    {
        ResetFingerActions();
        DropperHand.Mover.enabled = false;

        DropperHand.transform.DOMoveY(20f, 1f).SetEase(Ease.InBack).SetDelay(1f)
            .OnStart(delegate
            {
                //Instantiate(CompletionVfx, transform);
                GetComponentInParent<Level>().SpawnCompletionVfx();
            })
            .OnComplete(delegate
            {
                DropperHand.gameObject.SetActive(false);
                StartCoroutine(StartMilk());

                // Log GA Event
                var levelName = EventManager.DoFireGetLevelName();
                var eventString = $"{SequenceId}_DropFruitCompleted";
                //Debug.Log($" >>>>>>>>> Log GA Progression Status - Complete, {levelName}:{eventString}");
                GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete, levelName, eventString);
            });
    }

    void Mixing()
    {
        ResetFingerActions();

        TutHand.SetActive(false);
        DropperHand.RemoveDroppedSlices();

        JuicerAnimator.SetTrigger("Mixing");

        DOVirtual.DelayedCall(3.28f, delegate
        {
            //Instantiate(CompletionVfx, transform);
            GetComponentInParent<Level>().SpawnCompletionVfx();
        });
        DOVirtual.DelayedCall(5.28f, delegate
        {
            OnSequenceDone?.Invoke();
        });
    }

    #endregion



    #region MILK

    IEnumerator StartMilk()
    {
        MilkAnimator.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.89f);

        _fingerDownCurrentAction = delegate
        {
            MilkAnimator.SetTrigger("Pouring");
            _fingerDownCurrentAction = null;

            DOVirtual.DelayedCall(5f, delegate
            {
                OnDonePouringMilk();
            });
        };
    }

    private void OnDonePouringMilk()
    {

        ResetFingerActions();

        //DOVirtual.DelayedCall(1.75f, delegate
        //{
        //    JuicerContent.SetActive(true);
        //    MilkAnimator.gameObject.SetActive(false);

        //    //Instantiate(CompletionVfx, transform);
        //    GetComponentInParent<Level>().SpawnCompletionVfx();
        //    StartSugar();
        //});
    }

    #endregion



    #region SUGAR

    void StartSugar()
    {
        SugarAnimator.gameObject.SetActive(true);

        DOVirtual.DelayedCall(0.89f, delegate
        {
            _fingerDownCurrentAction = delegate
            {
                SugarAnimator.SetTrigger("Pouring");
                _fingerDownCurrentAction = null;

                // these are animation clip lengths
                DOVirtual.DelayedCall(4.163f + 1.250f, delegate { OnDonePouringSugar(); });
            };
        });
    }

    void OnDonePouringSugar()
    {
        ResetFingerActions();

        //GetComponentInParent<Level>().SpawnCompletionVfx();

        //JuicerAnimator.SetTrigger("CloseLid");

        //TutHand.transform.position = TutPosForGreen.position;
        //TutHand.SetActive(true);

        //DOVirtual.DelayedCall(.5f, delegate { _fingerDownCurrentAction = Mixing; });
    }

    #endregion







    public void HandleFingerDownEvent()
    {
        if(_fingerDownCurrentAction != null)
            _fingerDownCurrentAction.Invoke();
        else if(_fingerDownCurrentBoolAction != null)
            _fingerDownCurrentBoolAction.Invoke(true);
    }

    public void HandleFingerUpEvent()
    {
        if(_fingerUpCurrentAction != null)
            _fingerUpCurrentAction.Invoke();
        else if(_fingerUpCurrentBoolAction != null)
            _fingerUpCurrentBoolAction.Invoke(false);
    }

}
