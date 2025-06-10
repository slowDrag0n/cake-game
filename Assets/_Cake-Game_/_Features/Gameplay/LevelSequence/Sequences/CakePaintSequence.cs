using DG.Tweening;
using JetBrains.Annotations;
using Lean.Common;
using Lean.Touch;
using ScratchCardAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CakeSequence;

public class CakePaintSequence : LevelSequence
{
    [Header("Cake Painting Refs")]
    public GameObject Cake1;
    public ScratchCardManager FinalCakeScratchCard1;
    public EraseProgress      CakePaintProgress1;
    public LeanDragTranslate PaintingTool1Translator;
    public LeanFingerDown    PaintingTool1FingerDown;
    public LeanFingerUp      PaintingTool1FingerUp;
    public Transform PaintingDragPoint1;
    [Space]
    public GameObject Cake2;
    public ScratchCardManager FinalCakeScratchCard2;
    public EraseProgress      CakePaintProgress2;
    public LeanDragTranslate PaintingTool2Translator;
    public LeanFingerDown    PaintingTool2FingerDown;
    public LeanFingerUp      PaintingTool2FingerUp;
    public Transform PaintingDragPoint2;

    [Header("Current Cake")]
    [SerializeField] ScratchCardManager FinalCakeScratchCard;
    [SerializeField] EraseProgress CakePaintProgress;
    [SerializeField] LeanDragTranslate PaintingToolTranslator;
    [SerializeField] LeanFingerDown    PaintingToolFingerDown;
    [SerializeField] LeanFingerUp      PaintingToolFingerUp;
    [SerializeField] Transform PaintingDragPoint;
    [SerializeField] float PaintingToolDragDistanceThreshold = 1f;
    [Space]
    public SpriteRenderer SpoonRend;
    public Image[] CurvyLineImage;
    public SpriteRenderer[] BallToppingsRends;
    public SpriteRenderer DollToppingRend;
    public Sprite Spoon0;
    public Sprite Spoon1;
    public Sprite CakePaint0;
    public Sprite CakePaint1;
    public Sprite CurvyLine0;
    public Sprite CurvyLine1;
    public Sprite Ball0;
    public Sprite Ball1;
    public Sprite Doll0;
    public Sprite Doll1;
    public GameObject Doll0Canvas;
    public GameObject Doll1Canvas;
    public UnityEvent OnPaintingComplete;

    private int _selectedCakeType;



    protected override void Start()
    {
        base.Start();

        StartSequence();
    }

    public void StartSequence()
    {
        FinalCakeScratchCard1.Card.InputEnabled = false;
        FinalCakeScratchCard2.Card.InputEnabled = false;

        PaintingTool1FingerDown.gameObject.SetActive(false);
        PaintingTool1FingerUp.gameObject.SetActive(false);
        PaintingTool1Translator.GetComponent<LeanConstrainLocalPosition>().enabled = false;
        PaintingTool1Translator.gameObject.SetActive(false);

        PaintingTool2FingerDown.gameObject.SetActive(false);
        PaintingTool2FingerUp.gameObject.SetActive(false);
        PaintingTool2Translator.GetComponent<LeanConstrainLocalPosition>().enabled = false;
        PaintingTool2Translator.gameObject.SetActive(false);

        Cake1.gameObject.SetActive(true);
        Cake2.gameObject.SetActive(true);
    }

    public void OnCakeButtonClick(int type)
    {
        _selectedCakeType = type;

        if(_selectedCakeType == 0)
        {
            FinalCakeScratchCard = FinalCakeScratchCard1;
            CakePaintProgress = CakePaintProgress1;
            PaintingToolTranslator = PaintingTool1Translator;
            PaintingToolFingerDown = PaintingTool1FingerDown;
            PaintingToolFingerUp = PaintingTool1FingerUp;
            PaintingDragPoint = PaintingDragPoint1;
            Cake2.gameObject.SetActive(false);
        }
        else
        {
            FinalCakeScratchCard = FinalCakeScratchCard2;
            CakePaintProgress = CakePaintProgress2;
            PaintingToolTranslator = PaintingTool2Translator;
            PaintingToolFingerDown = PaintingTool2FingerDown;
            PaintingToolFingerUp = PaintingTool2FingerUp;
            PaintingDragPoint = PaintingDragPoint2;
            Cake1.gameObject.SetActive(false);
        }

        AssignCakeProperties(type);

        //CakeCanvasAnimator.SetTrigger("out");

        //IcingToolRenderer.sortingOrder = 11;

        FinalCakeScratchCard.gameObject.SetActive(true);
        CakePaintProgress.OnProgress += OnSpectulaDrag;

        PaintingToolFingerDown.gameObject.SetActive(true);
        PaintingToolFingerUp.gameObject.SetActive(true);
        paintingToolStartingPos = PaintingToolTranslator.transform.position;
        FinalCakeScratchCard.Card.InputEnabled = true;
        startedPainting = false;

        // activate spoon and animate
        PaintingToolTranslator.gameObject.SetActive(true);
        PaintingToolTranslator.transform.position = paintingToolStartingPos + new Vector2(13f, 0f);
        PaintingToolTranslator.transform.DOMoveX(paintingToolStartingPos.x, .25f).SetEase(Ease.OutBack);
    }



    void AssignCakeProperties(int selectedCake)
    {
        //cakeData[(int)type].CakeSprite.SetActive(true);
        //SpoonRend.sprite = selectedCake == 0 ? Spoon0 : Spoon1;
        //DollToppingRend.sprite = selectedCake == 0 ? Doll0 : Doll1;
        //for(int i = 0; i < CurvyLineImage.Length; i++)
        //    CurvyLineImage[i].sprite = selectedCake == 0 ? CurvyLine0 : CurvyLine1;
        //for(int i = 0; i < BallToppingsRends.Length; i++)
        //    BallToppingsRends[i].sprite = selectedCake == 0 ? Ball0 : Ball1;
        SetupCakeToPaint(selectedCake == 0 ? CakePaint0 : CakePaint1);
    }

    private void SetupCakeToPaint(Sprite cakeSprite)
    {
        FinalCakeScratchCard.gameObject.SetActive(true);

        FinalCakeScratchCard.SpriteCard.GetComponent<SpriteRenderer>().sprite = cakeSprite;
        FinalCakeScratchCard.Card.SetScratchTexture(cakeSprite.texture);
        FinalCakeScratchCard.Card.Mode = ScratchCard.ScratchMode.Restore;
        FinalCakeScratchCard.Card.FillInstantly();
    }

    public void ShowDollCanvas()
    {
        if(_selectedCakeType == 0)
            Doll0Canvas.gameObject.SetActive(true);
        else
            Doll1Canvas.gameObject.SetActive(true);
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
        if(FinalCakeScratchCard.Progress.currentProgress < .01f)
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
