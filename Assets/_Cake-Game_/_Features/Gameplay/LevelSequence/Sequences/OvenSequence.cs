using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenSequence : LevelSequence
{
    [SerializeField] GameObject OpenOvenDoor;
    [SerializeField] GameObject ClosedOvenDoor;
    [SerializeField] Sprite BakeCake;
    [SerializeField] SpriteRenderer BakeCakeImage;
    [SerializeField] Animator Bowlwithhands, Clock, TutHandAnim;
    [SerializeField] DOTweenController Needle, BigBowl;
    [SerializeField] SpriteChanger TutHand;
    [SerializeField] GameObject[] LineFInger;
    // Start is called before the first frame update
    void Start()
    {
        StartSequence();
    }

    public void OpenOvenDoor_(bool isOpen)
    {
        OpenOvenDoor.SetActive(isOpen);
        ClosedOvenDoor.SetActive(!isOpen);

    }

    public void TurnOnLeanFinger(int index, bool TurnOn)
    {
        LineFInger[index].SetActive(TurnOn);
    }

    public void StartSequence()
    {
        TutHandAnim.gameObject.SetActive(true);
        OpenOvenDoor_(false);
        TurnOnLeanFinger(0, true);
    }

    public void ClickOnLineSequenceOne()
    {
        TutHandAnim.gameObject.SetActive(false);
        OpenOvenDoor_(true);
        BigBowl.enabled = true;
        TurnOnLeanFinger(0, false);
    }

    public void OnCompleteBigBowlTween()
    {
        //TutHandAnim.gameObject.SetActive(true);
        //TutHandAnim.SetTrigger("placebowl");
        TurnOnLeanFinger(1, true);
        //Bowlwithhands.enabled = true;
    }

    public void ClickOnLineSequenceTwo()
    {
        TutHandAnim.gameObject.SetActive(false);
        Bowlwithhands.enabled = true;
        Bowlwithhands.SetTrigger("putin");
        TurnOnLeanFinger(1, false);

        StartCoroutine(PlayAnimWithDelay(2));
    }

    IEnumerator PlayAnimWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TurnOnLeanFinger(3, true);
        TutHandAnim.gameObject.SetActive(true);
    }

    public void ClickOnLineSequencefour()
    {
        TutHandAnim.gameObject.SetActive(false);
        OpenOvenDoor_(false);
        Clock.gameObject.SetActive(true);
        TurnOnLeanFinger(3, false);
    }

    public void OnTimerCompletedToBake()
    {
        BakeCakeImage.sprite = BakeCake;
        TurnOnLeanFinger(2, true);
        //TutHandAnim.gameObject.SetActive(true);
        Clock.SetTrigger("exit");
        DOVirtual.DelayedCall(1f, delegate { ClickOnLineSequenceThree(); });
    }

    public void ClickOnLineSequenceThree()
    {

        //TutHandAnim.gameObject.SetActive(false);
        OpenOvenDoor_(true);
        Bowlwithhands.SetTrigger("takeout");
        StartCoroutine(CloseSequence(2f));
    }

    IEnumerator CloseSequence(float delay)
    {

        yield return new WaitForSeconds(delay);
        Bowlwithhands.enabled = false;
        yield return new WaitForSeconds(1);
        //    OpenOvenDoor_(true);
        BigBowl.TriggerNextTween();
    }
}
