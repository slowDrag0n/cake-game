using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToppingAnimationController : MonoBehaviour
{
    public Transform FirstItem;
    public float FirstItemFallDelay = .5f;
    public float FirstItemFallDuration = .25f;
    public Ease FirstItemFallEase = Ease.Linear;
    public Transform[] ToppingItems;
    [Header("Anim Options")]
    public float FallDuration = .5f;
    public float FallDelayFactor = .1f;

    Vector3 _firstItemTargetPos;
    List<Vector3> _toppingItemTargetPositions;

    private void ResetTopping()
    {
        _firstItemTargetPos = FirstItem.position;
        _toppingItemTargetPositions = new List<Vector3>();
        foreach(var item in ToppingItems)
            _toppingItemTargetPositions.Add(item.position);

        FirstItem.position = new Vector3(FirstItem.position.x, 20f, 0f);
        foreach(var item in ToppingItems)
            item.position = new Vector3(item.position.x, 20f, 0f);
    }

    public void Animate()
    {
        ResetTopping();

        FirstItem.DOMove(_firstItemTargetPos, FirstItemFallDuration)
                    .SetDelay(FirstItemFallDelay)
                    .SetEase(FirstItemFallEase);

        if(ToppingItems.Length == 0)
            return;

        Transform[] bottomToTopItems = SortToppingItemsByPosY(ToppingItems);

        DOVirtual.DelayedCall(FirstItemFallDelay + FirstItemFallDuration + .5f, delegate
        {
            for(int i = 0; i < bottomToTopItems.Length; i++)
                bottomToTopItems[i].DOMove(_toppingItemTargetPositions[i], FallDuration + (i * FallDelayFactor)).SetEase(Ease.Linear);
        });

    }

    private Transform[] SortToppingItemsByPosY(Transform[] items)
    {
        return items.OrderBy(t => t.position.y).ToArray();
    }
}
