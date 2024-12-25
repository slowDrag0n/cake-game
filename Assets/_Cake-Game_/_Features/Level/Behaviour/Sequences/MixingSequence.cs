using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixingSequence : LevelSequence
{
    [SerializeField] Animator Stirrer;
    [SerializeField] GameObject[] FlourStirringImages;
    [SerializeField] GameObject StartingImage;
    [SerializeField] GameObject ButtonsCanvas;
    [SerializeField] GameObject Leanfinger;
    [SerializeField] GameObject LeanfingerPouring;
    [SerializeField] DragObjectWithinBounds DragStirrer;
    [SerializeField] DOTweenController  StirrerParent;
    [SerializeField] DOTweenController  smallbowlFlour;
    [SerializeField] DOTweenController  BigBowlFlour;
    [SerializeField] Animator BlueBallAnimator,SmallBlueBall;
    [SerializeField] GameObject BigBowl;
    [SerializeField] GameObject BowlArrow;
    [SerializeField] GameObject PouringParticle;
    [SerializeField] SpriteRenderer StirrerSpriteRend;
    [SerializeField] Sprite Stirrer1, Stirrer2;
    [SerializeField] Sprite StirrerTip1, StirrerTip2;
    [SerializeField] SpriteRenderer StirrerTipRend;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Header("NEW")]
    public float MixtureFrameInterval = .1f;
    public float MixtureTipFrameInterval = .1f;
    public float MixtureRequiredDuration = 3f;

    float mixerTimeCounter = 0f;
    float mixerFrameIntervalCounter = 0f;
    float mixerTipIntervalCounter = 0f;
    bool isCurrentlyMixing;
    bool isDoneMixing;
    int imageCounter=0;
    bool mixerTipSpriteFlag;

    void Start()
    {
        ButtonsCanvas.SetActive(true);
        //Leanfinger.SetActive(true);
        //DragStirrer.enabled = false;
    }

    private void Update()
    {
        if(isDoneMixing)
            return;

        if(isCurrentlyMixing == false)
            return;

        if(mixerTimeCounter >= MixtureRequiredDuration)
        {
            OnDoneMixing();
            return;
        }

        if(mixerFrameIntervalCounter >= MixtureFrameInterval)
        {
            mixerFrameIntervalCounter = 0f;
            RandomizeMixtureImage();
        }

        if(mixerTipIntervalCounter >= MixtureTipFrameInterval)
        {
            mixerTipIntervalCounter = 0f;
            mixerTipSpriteFlag = !mixerTipSpriteFlag;
            StirrerTipRend.sprite = mixerTipSpriteFlag ? StirrerTip1 : StirrerTip2;
        }

        mixerTimeCounter += Time.deltaTime;
        mixerFrameIntervalCounter += Time.deltaTime;
        mixerTipIntervalCounter += Time.deltaTime;
    }



    #region SELF

    public void StartMixing()
    {
        if(isDoneMixing)
            return;

        isCurrentlyMixing = true;

        if(StartingImage.activeInHierarchy)
        {
            StartingImage.gameObject.SetActive(false);
            RandomizeMixtureImage();
        }
    }

    public void StopMixing()
    {
        isCurrentlyMixing = false;
    }

    public void OnDoneMixing()
    {
        isDoneMixing = true;

        foreach(var item in FlourStirringImages)
            item.SetActive(false);
        FlourStirringImages[^1].gameObject.SetActive(true);


        StirrerParent.TriggerNextTween();
        StartCoroutine(PlayAnimWithDelay(2f));
    }

    #endregion









    #region KHALID

    public void OnStirrerButtonClick1()
    {
        StirrerSpriteRend.sprite = Stirrer1;
        StirrerParent.gameObject.SetActive(true);
        ButtonsCanvas.GetComponent<DOTweenController>().TriggerNextTween();
    }

    public void OnStirrerButtonClick2()
    {
        StirrerSpriteRend.sprite = Stirrer2;
        StirrerParent.gameObject.SetActive(true);
        ButtonsCanvas.GetComponent<DOTweenController>().TriggerNextTween();
    }

    public void PlayStirrerAnimation(string trigger)
    {
        Stirrer.SetTrigger(trigger);
    }

    private int previousIndex = -1;
    private int tempcounter =0;
    public void ChangeImages()
    {
        StartingImage.SetActive(false);
        if(imageCounter < 15 && DragStirrer.enabled == false)
        {
            int randomIndex = RandomizeMixtureImage();
            PlayStirrerAnimation("still");
            // Store the current index as previous for the next iteration
            previousIndex = randomIndex;

            // Increment the counter
            imageCounter++;
            if(imageCounter > 0)
            {
                imageCounter = 0;
                DragStirrer.enabled = true;
                Leanfinger.SetActive(false);
                DragStirrer.OnDraggingAction += ChangeImages;
            }
        }
        else if(imageCounter < 10 && DragStirrer.enabled == true)
        {
            if(imageCounter >= 0)
            {
                StirrerParent.TriggerNextTween();
            }
            StartCoroutine(ExecuteAfterDelay(0.3f, () =>
            {
                Debug.Log("Here");
                int randomIndex = UnityEngine.Random.Range(0, FlourStirringImages.Length - 1);

                if(previousIndex != -1)
                {
                    FlourStirringImages[previousIndex].gameObject.SetActive(false);
                }

                FlourStirringImages[randomIndex].gameObject.SetActive(true);
                PlayStirrerAnimation("move");
                // Store the current index sas previous for the next iteration
                previousIndex = randomIndex;

                if(tempcounter >= 20)
                {
                    tempcounter = 0;
                    imageCounter++;
                }
                tempcounter++;
                if(imageCounter == 10)
                {
                    StirrerParent.gameObject.SetActive(false);
                    DragStirrer.OnDraggingAction -= ChangeImages;
                    DragStirrer.enabled = false;
                    FlourStirringImages[^1].gameObject.SetActive(true);
                    FlourStirringImages[previousIndex].gameObject.SetActive(false);
                    StartCoroutine(PlayAnimWithDelay(1f));
                }
            }));
        }
    }

    private int RandomizeMixtureImage()
    {
        // Randomly select an image index
        int randomIndex = UnityEngine.Random.Range(0, FlourStirringImages.Length - 2);

        foreach(var item in FlourStirringImages)
            item.SetActive(false);
        FlourStirringImages[randomIndex].gameObject.SetActive(true);

        return randomIndex;
    }

    IEnumerator PlayAnimWithDelay(float delay)
    {
        yield return null;

        yield return new WaitForSeconds(delay);
        BlueBallAnimator.SetTrigger("moveout");

        yield return new WaitForSeconds(delay*.5f);
        BigBowl.SetActive(true);

        yield return new WaitForSeconds(1f);
        SmallBlueBall.gameObject.SetActive(true);
        SmallBlueBall.SetTrigger("movein");

        yield return new WaitForSeconds(1f);
        BowlArrow.SetActive(true);
        LeanfingerPouring.SetActive(true);
    }
    public IEnumerator ExecuteAfterDelay(float delays, Action action)
    {
        // Wait until the condition is true
        yield return new WaitForSeconds(delays);

        // Execute the passed method
        action?.Invoke();

        yield return null;
    }
    public void PourFlourInBowl()
    {
        BowlArrow.SetActive(false);
        SmallBlueBall.SetTrigger("rotate");
        StartCoroutine(ExecuteAfterDelay(0.3f, () =>
        {
            PouringParticle.SetActive(true);
            smallbowlFlour.enabled = true;
            BigBowlFlour.enabled = true;

        }));
    }

    public void EndSequence()
    {
        SmallBlueBall.transform.rotation = new Quaternion(0, 0, 0, SmallBlueBall.transform.rotation.w);
        SmallBlueBall.SetTrigger("moveout");
        BigBowl.GetComponent<DOTweenController>().TriggerNextTween();
    }

    #endregion
}

