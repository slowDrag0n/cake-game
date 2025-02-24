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
    [SerializeField] ScratchCardManager FinalCakeScratchCard;
    [SerializeField] EraseProgress CakePaintProgress;

    [Header("Cake Painting Refs")]
    public LeanDragTranslate PaintingToolTranslator;
    public LeanFingerDown    PaintingToolFingerDown;
    public LeanFingerUp      PaintingToolFingerUp;
    public Transform PaintingDragPoint;
    public float PaintingToolDragDistanceThreshold = 1f;
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
        FinalCakeScratchCard.Card.InputEnabled = false;
        PaintingToolFingerDown.gameObject.SetActive(false);
        PaintingToolFingerUp.gameObject.SetActive(false);
        PaintingToolTranslator.GetComponent<LeanConstrainLocalPosition>().enabled = false;

        PaintingToolTranslator.gameObject.SetActive(false);
    }

    public void OnCakeButtonClick(int type)
    {
        _selectedCakeType = type;

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
        SpoonRend.sprite = selectedCake == 0 ? Spoon0 : Spoon1;
        DollToppingRend.sprite = selectedCake == 0 ? Doll0 : Doll1;
        for(int i = 0; i < CurvyLineImage.Length; i++)
            CurvyLineImage[i].sprite = selectedCake == 0 ? CurvyLine0 : CurvyLine1;
        for(int i = 0; i < BallToppingsRends.Length; i++)
            BallToppingsRends[i].sprite = selectedCake == 0 ? Ball0 : Ball1;
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
