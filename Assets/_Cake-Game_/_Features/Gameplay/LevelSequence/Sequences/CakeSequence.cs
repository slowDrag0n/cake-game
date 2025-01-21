using System.Collections;
using System;
using UnityEngine;
using DG.Tweening;
using ScratchCardAsset;
using Lean.Touch;
using Lean.Common;

public class CakeSequence : LevelSequence
{
    [Serializable]
    public enum CakeType
    {
        ChocoCake=0,
        PlaneCake=1,
        StawberryCake=2
    }

    [SerializeField] CakeType cakeType = CakeType.ChocoCake;
    [SerializeField] CakeData[] cakeData;
    [SerializeField] DOTweenController CakeTray;
    [SerializeField] DOTweenController CakeBowl;
    [SerializeField] Transform Cake;
    [SerializeField] Transform CakeTopIcing;
    [SerializeField] GameObject FinalCake;
    [SerializeField] GameObject BowlFront;
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject[] LineFInger;
    [SerializeField] Animator CakeCanvasAnimator;

    [SerializeField] Animator ToppingCanvasAnimator;

    [SerializeField] Animator BowlWithChoco;
    [SerializeField] Animator TutHand;
    [SerializeField] SpriteRenderer IcingToolRenderer;
    [SerializeField] ScratchCardManager FinalCakeScratchCard;
    [SerializeField] EraseProgress CakePaintProgress;
    [SerializeField] GameObject CompletionVfx;

    [Header("SpriteRenderer")]
    public SpriteRenderer CakeImage;
    public SpriteRenderer SpatulaImage;
    public SpriteRenderer BowlFillingImage;
    public SpriteRenderer SpreaderImage;
    public SpriteRenderer UpperLayerImage;
    public SpriteRenderer SideLayerImage;

    [Header("Cake Icing Refs")]
    public LeanDragTranslate IcingToolTranslator;
    public LeanFingerDown IcingToolFingerDown;
    public LeanFingerUp IcingToolFingerUp;
    public Transform IcingDragPoint;
    public float IcingToolDragDistanceThreshold = 1f;

    [Header("Cake Icing Cone Refs")]
    [SerializeField] DOTweenController IcingConeCanvas;
    public IcingCreamConeBehaviour IcingCone;

    [Header("Cake Topping Items Refs")]
    [SerializeField] DOTweenController ToppingSelectionCanvas;
    public ToppingAnimationController[] CakeToppings;



    void Start()
    {
        StartSequence();
        //SpectulaObj.OnDraggingAction += OnSpectulaDrag;
    }



    public void StartSequence()
    {
        CakeTray.gameObject.SetActive(true);

        FinalCakeScratchCard.Card.InputEnabled = false;
        IcingToolFingerDown.gameObject.SetActive(false);
        IcingToolFingerUp.gameObject.SetActive(false);
        IcingToolTranslator.GetComponent<LeanConstrainLocalPosition>().enabled = false;

        IcingCone.gameObject.SetActive(false);

        foreach(var item in CakeToppings)
            item.gameObject.SetActive(false);
    }

    public void OnCakeTrayTweenComplete()
    {
        CakeBowl.gameObject.SetActive(true);
        CakeBowl.GetComponent<DOTweenController>().enabled = true;
        FinalCakeScratchCard.gameObject.SetActive(false);

    }
    public void OnCakeBowlTweenComplete()
    {
        Arrow.gameObject.SetActive(true);
        TurnOnLeanFinger(0, true);
    }

    public void OnCLickLeanFingerOne()
    {
        Arrow.gameObject.SetActive(false);
        CakeMovementToTray();
        TurnOnLeanFinger(0, false);
    }
    public void TurnOnLeanFinger(int index, bool TurnOn)
    {
        LineFInger[index].SetActive(TurnOn);
    }

    public void CakeMovementToTray()
    {
        Cake.parent = CakeTray.transform;
        Cake.DOMoveY(Cake.position.y + 2.5f, 1.2f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                BowlFront.SetActive(false);
                Cake.DOLocalMove(new Vector3(-0.02f, 2.56f, 0), 2f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Cake.DOScale(new Vector3(0.908f, 0.876f, 0.75f), 1f);
                    CakeBowl.TriggerNextTween();
                    CakeCanvasAnimator.gameObject.SetActive(true);
                    CakeCanvasAnimator.SetTrigger("in");
                });
            });
    }

    void AssignCakeProperties(CakeType type)
    {
        //cakeData[(int)type].CakeSprite.SetActive(true);
        SpatulaImage.sprite = cakeData[(int)type].SpatulaSpritee;
        BowlFillingImage.sprite = cakeData[(int)type].BowlFillingSprite;
        SetupCakeToPaint(cakeData[(int)type].CakeSprite);
    }

    private void SetupCakeToPaint(Sprite cakeSprite)
    {
        FinalCakeScratchCard.gameObject.SetActive(true);

        FinalCakeScratchCard.SpriteCard.GetComponent<SpriteRenderer>().sprite = cakeSprite;
        FinalCakeScratchCard.Card.SetScratchTexture(cakeSprite.texture);
        FinalCakeScratchCard.Card.Mode = ScratchCard.ScratchMode.Restore;
        FinalCakeScratchCard.Card.FillInstantly();
    }

    public void OnCakeButtonClick(int type)
    {
        AssignCakeProperties((CakeType)type);

        //CakeCanvasAnimator.SetTrigger("out");

        BowlWithChoco.gameObject.SetActive(true);
        BowlWithChoco.SetTrigger("moveinspread");

        //StartCoroutine(ExecuteAfterDelay(1f, () => { CakeCanvasAnimator.gameObject.SetActive(false); }));
        StartCoroutine(ExecuteAfterDelay(1f, () =>
        {
            BowlWithChoco.SetTrigger("getchoco");

            StartCoroutine(ExecuteAfterDelay(3.1f, () =>
            {
                BowlWithChoco.enabled = false;
                TutHand.gameObject.SetActive(true);
                TutHand.SetTrigger("spectulatocake");
                IcingToolRenderer.sortingOrder = 11;

                FinalCakeScratchCard.gameObject.SetActive(true);
                CakePaintProgress.OnProgress += OnSpectulaDrag;

                IcingToolFingerDown.gameObject.SetActive(true);
                IcingToolFingerUp.gameObject.SetActive(true);
                icingToolStartingPos = IcingToolTranslator.transform.position;
                FinalCakeScratchCard.Card.InputEnabled = true;
                startedPaintingIcing = false;
            }));
        }));
    }

    public IEnumerator ExecuteAfterDelay(float delays, Action action)
    {
        // Wait until the condition is true
        yield return new WaitForSeconds(delays);

        // Execute the passed method
        action?.Invoke();

        yield return null;
    }

    public void OnSpectulaDrag(float progress)
    {
        if(FinalCakeScratchCard.Progress.currentProgress < .002f)
        {
            CakePaintProgress.OnProgress -= OnSpectulaDrag;

            IcingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = false;
            IcingToolFingerDown.gameObject.SetActive(false);
            IcingToolFingerUp.gameObject.SetActive(false);

            IcingToolRenderer.gameObject.SetActive(false);
            IcingConeCanvas.gameObject.SetActive(true);

            DOVirtual.DelayedCall(1f, delegate { SpawnCompletionVfx(); });
        }
    }

    public void SpawnCompletionVfx()
    {
        //Instantiate(CompletionVfx, transform);
        GetComponentInParent<Level>().SpawnCompletionVfx();
    }

    public void OnIcingButtonClick(int type)
    {
        var cakeType = (CakeType)type;
        IcingCone.Init(cakeData[(int)type].SpreaderSprite, cakeData[(int)type].UpperLayerSprite, cakeData[(int)type].SideLayerSprite, OnIcingConeComplete);

        IcingCone.gameObject.SetActive(true);
    }

    public void Topping_Canvas()
    {

        ToppingCanvasAnimator.gameObject.SetActive(true);
        ToppingCanvasAnimator.SetTrigger("in");
    }



    #region ICING

    bool startedPaintingIcing = false;
    Vector2 icingToolStartingPos = Vector2.zero;
    Tween moveBackIcingToolTween;
    public void IcingFingerDownHandler(LeanFinger finger)
    {
        var fingerPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        var icingToolPos = IcingDragPoint.position;
        var fingerDistance = Vector2.Distance(fingerPos, icingToolPos);
        Debug.Log("Finger Distance: " + fingerDistance);

        startedPaintingIcing = true;
        IcingToolTranslator.GetComponent<LeanConstrainLocalPosition>().enabled = true;

        if(fingerDistance < IcingToolDragDistanceThreshold)
        {
            IcingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = true;
            if(moveBackIcingToolTween != null)
                moveBackIcingToolTween.Kill();
        }

    }

    public void IcingFingerUpHandler(LeanFinger finger)
    {
        IcingToolTranslator.enabled = FinalCakeScratchCard.Card.InputEnabled = false;

        if(startedPaintingIcing)
        {
            moveBackIcingToolTween = IcingToolTranslator.transform.DOMove(icingToolStartingPos, .5f);
        }
    }

    #endregion



    #region ICING CONE
    void OnIcingConeComplete()
    {
        ToppingSelectionCanvas.gameObject.SetActive(true);
    }

    public void OnClick_ToppingSelectionButton(int id)
    {
        CakeToppings[id].gameObject.SetActive(true);
        CakeToppings[id].Animate();
    }

    #endregion



    #region TOPPINGS

    #endregion

}

[Serializable]
public class CakeData
{
    public Sprite CakeSprite;
    public Sprite SpatulaSpritee;
    public Sprite BowlFillingSprite;
    public Sprite SpreaderSprite;
    public Sprite UpperLayerSprite;
    public Sprite SideLayerSprite;
}
