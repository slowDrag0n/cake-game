using DG.Tweening;
using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CuttingSequence : LevelSequence
{
    public GameObject FingerController;
    public GameObject HintArrow;
    public Animator KnifeAnimator;
    public Transform Board;
    public Transform[] CuttingItems;
    public Transform ItemUnderKnifePoint;
    public Transform KnifeSlicePoint;
    public float SliceAnimDuration = .3f;
    public int RequiredSteps = 3;

    int _stepsCompleted;

    protected override void Start()
    {
        _stepsCompleted = 0;

        SequenceOpening();
    }

    void SequenceOpening()
    {
        KnifeAnimator.enabled = false;
        FingerController.SetActive(false);

        Board.position += 13f * Vector3.right;
        KnifeAnimator.transform.position += -10f * Vector3.right;

        Board.DOMoveX(0f, 1.3f).SetEase(Ease.OutBack)
            .OnStart(delegate { SoundController.Instance.PlaySound(SoundType.ItemComing); })
            .OnComplete(delegate
            {
                KnifeAnimator.transform.DOMoveX(0f, .8f).SetEase(Ease.OutBack)
                    .OnStart(delegate { SoundController.Instance.PlaySound(SoundType.ItemComing); })
                    .OnComplete(delegate
                    {
                        HintArrow.gameObject.SetActive(true);
                        PlaceNextItemForCut(_stepsCompleted);

                        KnifeAnimator.enabled = true;

                    });
            });
    }

    void SequenceClosing()
    {
        KnifeAnimator.enabled = false;
        Board.DOMoveX(13f, 1.3f).SetEase(Ease.InBack).SetDelay(1f)
            .OnComplete(delegate
            {
                KnifeAnimator.transform.DOMoveX(-10f, .8f).SetEase(Ease.InBack)
                .OnStart(delegate { SoundController.Instance.PlaySound(SoundType.ItemComing); })
                .OnComplete(delegate
                {
                    //OnSequenceDone?.Invoke();
                    gameObject.SetActive(false);
                });
            });
    }

    public void UpdateStepCount()
    {
        _stepsCompleted++;

        if(_stepsCompleted >= RequiredSteps)
        {
            SequenceClosing();
        }
        else
        {
            PlaceNextItemForCut(_stepsCompleted);
        }
    }

    private void PlaceNextItemForCut(int stepsCompleted)
    {
        var nextItem = CuttingItems[stepsCompleted];

        FingerController.SetActive(false);
        nextItem.DOMove(ItemUnderKnifePoint.position, .7f)
                .SetEase(Ease.OutBack)
                .OnComplete(delegate
                {
                    FingerController.SetActive(true);

                    // Log GA Event
                    var levelName = EventManager.DoFireGetLevelName();
                    var eventString = $"{SequenceId}_Fruit{stepsCompleted.ToString()}Started";
                    //Debug.Log($" >>>>>>>>> Log GA Progression Status - Start, {levelName}:{eventString}");
                    GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Start, levelName, eventString);
                });
    }

    public void OnCut()
    {
        FingerController.SetActive(false);

        var cuttingItemAc = CuttingItems[_stepsCompleted].GetComponent<Animator>() ;

        //Knife.transform.DOMove(KnifeSlicePoint.position, .25f).SetLoops(2, LoopType.Yoyo);
        KnifeAnimator.SetTrigger("Slicing");
        DOVirtual.DelayedCall(.5f, delegate
        {
            cuttingItemAc.SetTrigger("Cutting");

            DOVirtual.DelayedCall(SliceAnimDuration, delegate
            {
                cuttingItemAc.transform.position += .2f * _stepsCompleted * Vector3.up;

                UpdateStepCount();
            });
        });
    }
}
