using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Jar : SimpleSequenceInteractable
{
    public Transform[] JarItems;

    private void Start()
    {
        HideItems();
    }

    public void SetItemParent(Transform itemParent)
    {
        foreach(var item in JarItems)
            item.parent = itemParent;
    }

    public void HideItems()
    {
        foreach(var item in JarItems)
            item.gameObject.SetActive(false);
    }
}
