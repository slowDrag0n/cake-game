using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ToppingAnimationController : MonoBehaviour
{
    [Header("Play on Start?")]
    public bool AutoAnimate = true;

    [Header("Topping  Items")]
    public Transform FirstItem;
    public float FirstItemFallDelay = .5f;
    public float FirstItemFallDuration = .25f;
    public Ease FirstItemFallEase = Ease.Linear;
    public Transform[] ToppingItems;

    [Header("Anim Options")]
    public float FallDuration = .5f;
    public float FallDelayFactor = .1f;

    [Header("On Complete animating items")]
    public UnityEvent OnCompleteFirstItem;
    public UnityEvent OnCompleteAllOtherItems;

    Vector3 _firstItemTargetPos;
    List<Vector3> _toppingItemTargetPositions;

    private void Start()
    {
        if(AutoAnimate)
            Animate();
    }

    private void ResetTopping()
    {
        if(FirstItem != null)
        {
            _firstItemTargetPos = FirstItem.position;
            FirstItem.position = new Vector3(FirstItem.position.x, 20f, 0f);
        }

        _toppingItemTargetPositions = new List<Vector3>();
        foreach(var item in ToppingItems)
            _toppingItemTargetPositions.Add(item.position);

        foreach(var item in ToppingItems)
            item.position = new Vector3(item.position.x, 20f, 0f);
    }

    public void Animate()
    {
        ResetTopping();

        if(FirstItem != null)
            FirstItem.DOMove(_firstItemTargetPos, FirstItemFallDuration)
                    .SetDelay(FirstItemFallDelay)
                    .SetEase(FirstItemFallEase)
                    .OnComplete(delegate { OnCompleteFirstItem?.Invoke(); });

        if(ToppingItems.Length == 0)
            return;

        Transform[] bottomToTopItems = SortToppingItemsByPosY(ToppingItems);

        DOVirtual.DelayedCall(FirstItemFallDelay + FirstItemFallDuration + .5f, delegate
        {
            for(int i = 0; i < bottomToTopItems.Length; i++)
            {
                bottomToTopItems[i].DOMove(_toppingItemTargetPositions[i], FallDuration + (i * FallDelayFactor)).SetEase(Ease.Linear);

                // invoke oncompleteallitems for last item
                if(i == bottomToTopItems.Length - 1)
                {
                    bottomToTopItems[i].DOMove(_toppingItemTargetPositions[i], FallDuration + (i * FallDelayFactor)).SetEase(Ease.Linear)
                                        .OnComplete(delegate
                                        {
                                            Debug.Log("Invoked for completing all items");
                                            OnCompleteAllOtherItems?.Invoke();
                                        });

                }
            }
        });

    }

    private Transform[] SortToppingItemsByPosY(Transform[] items)
    {
        return items.OrderBy(t => t.position.y).ToArray();
    }
}
