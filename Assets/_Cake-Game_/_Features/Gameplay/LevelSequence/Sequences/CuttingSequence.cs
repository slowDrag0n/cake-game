using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingSequence : LevelSequence
{
    public GameObject FingerController;
    public Transform Knife;
    public Transform Board;
    public Transform[] CuttingItems;
    public Transform ItemUnderKnifePoint;
    public Transform KnifeSlicePoint;
    public float SliceAnimDuration = .3f;
    public int RequiredSteps = 3;
    public GameObject CompletionVfx;

    int _stepsCompleted;

    private void Start()
    {
        _stepsCompleted = 0;

        SequenceOpening();

        OnSequenceDone += delegate { Debug.Log("CUTTING COMPLETED!"); };
    }

    void SequenceOpening()
    {
        FingerController.SetActive(false);

        Board.position += 13f * Vector3.right;
        Knife.position += -10f * Vector3.right;

        Board.DOMoveX(0f, 1.3f).SetEase(Ease.OutBack)
            .OnComplete(delegate
            {
                Knife.DOMoveX(0f, .8f).SetEase(Ease.OutBack)
                .OnComplete(delegate
                {
                    PlaceNextItemForCut(_stepsCompleted);
                });
            });
    }

    void SequenceClosing()
    {
        Instantiate(CompletionVfx, transform);

        Board.DOMoveX(13f, 1.3f).SetEase(Ease.InBack).SetDelay(1f)
            .OnComplete(delegate
            {
                Knife.DOMoveX(-10f, .8f).SetEase(Ease.InBack)
                .OnComplete(delegate
                {
                    OnSequenceDone?.Invoke();
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
                });
    }

    public void OnCut()
    {
        FingerController.SetActive(false);

        var cuttingItemAc = CuttingItems[_stepsCompleted].GetComponent<Animator>() ;

        Knife.DOMove(KnifeSlicePoint.position, .25f).SetLoops(2, LoopType.Yoyo);
        DOVirtual.DelayedCall(.25f, delegate
        {
            cuttingItemAc.SetTrigger("Cutting");

            DOVirtual.DelayedCall(SliceAnimDuration, delegate
            {
                cuttingItemAc.transform.position += .6f * _stepsCompleted * Vector3.up;

                UpdateStepCount();
            });
        });
    }
}
