using DG.Tweening;
using JetBrains.Annotations;
using Lean.Common;
using Lean.Touch;
using ScratchCardAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CakeSequence;

public class CakeMultiPaintSequence : LevelSequence
{
    [SerializeField] ScratchCardManager FinalCakeScratchCard;
    [SerializeField] EraseProgress CakePaintProgress;

    [Header("Cake Painting Refs")]
    public LeanDragTranslate PaintingToolTranslator;
    public LeanFingerDown    PaintingToolFingerDown;
    public LeanFingerUp      PaintingToolFingerUp;
    public Transform PaintingDragPoint;
    public float PaintingToolDragDistanceThreshold = 1f;
    [Space]
    public SpriteRenderer SpoonRenderer;
    public SpriteRenderer PaintInBowlRenderer;
    public SpriteRenderer CreamConeRenderer;
    public SpriteRenderer CreamTopRenderer;
    public SpriteRenderer CreamDripRenderer;
    public CakeProfileData[] CakeDataProfiles;
    public UnityEvent OnPaintingComplete;

    private int _selectedCakeType;



    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime((1f / 60f) * 2f);
        StartSequence();
    }

    public void StartSequence()
    {
        OnSequenceStart?.Invoke();

        FinalCakeScratchCard.Card.InputEnabled = false;
        PaintingToolFingerDown.gameObject.SetActive(false);
        PaintingToolFingerUp.gameObject.SetActive(false);
        PaintingToolTranslator.GetComponent<LeanConstrainLocalPosition>().enabled = false;

        //PaintingToolTranslator.gameObject.SetActive(false);
        FinalCakeScratchCard.gameObject.SetActive(false);
    }

    public void OnCakeButtonClick(int type)
    {
        _selectedCakeType = type;

        AssignCakeProperties(type);

        //CakeCanvasAnimator.SetTrigger("out");

        //IcingToolRenderer.sortingOrder = 11;

        FinalCakeScratchCard.gameObject.SetActive(true);
        CakePaintProgress.OnProgress += OnSpectulaDrag;

        //PaintingToolFingerDown.gameObject.SetActive(true);
        //PaintingToolFingerUp.gameObject.SetActive(true);
        paintingToolStartingPos = PaintingToolTranslator.transform.position;
        FinalCakeScratchCard.Card.InputEnabled = true;
        startedPainting = false;

        //// activate spoon and animate
        //PaintingToolTranslator.gameObject.SetActive(true);
        //PaintingToolTranslator.transform.position = paintingToolStartingPos + new Vector2(13f, 0f);
        //PaintingToolTranslator.transform.DOMoveX(paintingToolStartingPos.x, .25f).SetEase(Ease.OutBack);
    }



    void AssignCakeProperties(int selectedCake)
    {
        SpoonRenderer.sprite = CakeDataProfiles[selectedCake].PaintSpoon;
        PaintInBowlRenderer.sprite = CakeDataProfiles[selectedCake].PaintInBowl;
        SetupCakeToPaint(CakeDataProfiles[selectedCake].CakePaint);
    }

    private void SetupCakeToPaint(Sprite cakeSprite)
    {
        FinalCakeScratchCard.gameObject.SetActive(true);

        FinalCakeScratchCard.SpriteCard.GetComponent<SpriteRenderer>().sprite = cakeSprite;
        FinalCakeScratchCard.Card.SetScratchTexture(cakeSprite.texture);
        FinalCakeScratchCard.Card.Mode = ScratchCard.ScratchMode.Restore;
        FinalCakeScratchCard.Card.FillInstantly();
    }

    //public void ShowDollCanvas()
    //{
    //    if(_selectedCakeType == 0)
    //        Doll0Canvas.gameObject.SetActive(true);
    //    else
    //        Doll1Canvas.gameObject.SetActive(true);
    //}

    public void OnIcingConeClick(int type)
    {
        AssignIcingConeProperties(type);
    }

    private void AssignIcingConeProperties(int type)
    {
        CreamConeRenderer.sprite = CakeDataProfiles[type].CreamConeBottle;
        CreamTopRenderer.sprite = CakeDataProfiles[type].CreamTop;
        CreamTopRenderer.transform.localPosition = CakeDataProfiles[type].TransformData.PosC1;
        CreamDripRenderer.sprite = CakeDataProfiles[type].CreamDrip;
        CreamDripRenderer.transform.localPosition = CakeDataProfiles[type].TransformData.PosC2;
        CreamDripRenderer.transform.localScale = CakeDataProfiles[type].TransformData.ScaleC2;
    }


    #region PAINTING

    bool startedPainting = false;
    Vector2 paintingToolStartingPos = Vector2.zero;
    Tween moveBackPaintingToolTween;

    public void PaintingToolFingerDownHandler(LeanFinger finger)
    {
        var fingerPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        var paintingToolPos = PaintingDragPoint.position;
        var fingerDistance = Vector2.Distance(fingerPos, paintingToolPos);
        Debug.Log("Finger Distance: " + fingerDistance);

        startedPainting = true;
        PaintingToolTranslator.GetComponent<LeanConstrainLocalPosition>().enabled = true;

        if(fingerDistance < PaintingToolDragDistanceThreshold)
        {
            PaintingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = true;
            if(moveBackPaintingToolTween != null)
                moveBackPaintingToolTween.Kill();
        }

    }

    public void PaintingToolFingerUpHandler(LeanFinger finger)
    {
        PaintingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = false;

        if(startedPainting)
        {
            moveBackPaintingToolTween = PaintingToolTranslator.transform.DOMove(paintingToolStartingPos, .5f);
        }
    }

    public void OnSpectulaDrag(float progress)
    {
        if(FinalCakeScratchCard.Progress.currentProgress < .002f)
        {
            CakePaintProgress.OnProgress -= OnSpectulaDrag;

            PaintingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = false;
            PaintingToolFingerDown.gameObject.SetActive(false);
            PaintingToolFingerUp.gameObject.SetActive(false);

            //PaintingToolTranslator.gameObject.SetActive(false);

            FinalCakeScratchCard.Card.ClearInstantly();

            PaintingToolTranslator.transform.DOMove(paintingToolStartingPos, 1f)
                .OnComplete(delegate { OnPaintingComplete?.Invoke(); });
        }
    }

    #endregion

}

[Serializable]
public class CakeProfileData
{
    public string CakeDesc;
    public Sprite CakePaint;
    public Sprite PaintSpoon;
    public Sprite PaintInBowl;
    public Sprite CreamConeBottle;
    public Sprite CreamTop;
    public Sprite CreamDrip;
    public TransformDataForCakeProfile TransformData;
}

[Serializable]
public struct TransformDataForCakeProfile
{
    public Vector3 PosC1;
    public Vector3 PosC2;
    public Vector3 ScaleC2;
}