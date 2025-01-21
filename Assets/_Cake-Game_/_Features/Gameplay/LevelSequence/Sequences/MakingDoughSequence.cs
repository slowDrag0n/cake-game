using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MakingDoughSequence : LevelSequence
{

    [SerializeField] Animator SugarPot;
    [SerializeField] GameObject SugarSpreadInBowl;
    [SerializeField] GameObject LeanFingerHandler;
    [SerializeField] GameObject SugarParticles;
    [SerializeField] GameObject CompletionVfx;

    Action _fingerDownCurrentAction;

    public void StartSugar()
    {
        SugarPot.gameObject.SetActive(true);
        StartCoroutine(TurnOnFingerHandler(1.5f, LeanFingerHandler));
        DOVirtual.DelayedCall(0.89f, delegate
        {
            _fingerDownCurrentAction = delegate
            {
                StartCoroutine(TurnOnFingerHandler(1.5f, SugarParticles));
                SugarPot.SetTrigger("Pouring");
                _fingerDownCurrentAction = null;

                DOVirtual.DelayedCall(2.5f, delegate { SugarSpreadInBowl.transform.DOScale(Vector3.one, 1.25f); });
                DOVirtual.DelayedCall(4.163f, delegate { OnDonePouringSugar(); });
            };
        });
    }

    IEnumerator TurnOnFingerHandler(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    void OnDonePouringSugar()
    {
        SugarParticles.SetActive(false);


    }

    public void HandleFingerDown()
    {
        _fingerDownCurrentAction?.Invoke();
    }

    public void SpawnCompletionVfx()
    {
        //Instantiate(CompletionVfx, transform);
        GetComponentInParent<Level>().SpawnCompletionVfx();
    }
}
