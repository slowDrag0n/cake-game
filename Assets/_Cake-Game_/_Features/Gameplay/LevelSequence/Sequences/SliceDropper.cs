using Lean.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceDropper : MonoBehaviour
{
    public LeanManualTranslate Mover;
    public List<Rigidbody2D> Slices;
    public Transform SliceSpawnPoint;
    public float DropDelay = .55f;

    public bool IsDropping { get; set; }
    public Action OnAllSlicesDropped { get; set; }

    float _dropNextTimer = 0;
    int _slicesLeft;

    private void Update()
    {
        if(IsDropping == false)
        {
            _dropNextTimer = 0f;
            return;
        }

        _dropNextTimer += Time.deltaTime;
        if(_dropNextTimer > DropDelay)
        {
            _dropNextTimer = 0f;

            DropSlice();
        }
    }

    public void Initialize(Action OnDone = null)
    {
        OnAllSlicesDropped = OnDone;
        _slicesLeft = Slices.Count - 1;
    }

    public void DropSlice()
    {
        // initial check before start dropping
        if(Slices.Count == 0) return;

        var sliceSpawn = Slices[_slicesLeft];
        _slicesLeft--;

        sliceSpawn.transform.parent = null;
        sliceSpawn.isKinematic = false;
        sliceSpawn.GetComponent<Collider2D>().enabled = true;

        var sliceSpawnSprite = sliceSpawn.GetComponent<SpriteRenderer>();
        sliceSpawnSprite.color = Color.white;
        sliceSpawnSprite.sortingLayerName = "New Layer 1";
        sliceSpawnSprite.sortingOrder = -1;

        if(_slicesLeft == 0)
        {
            OnAllSlicesDropped?.Invoke();
        }
    }

    public void RemoveDroppedSlices()
    {
        foreach(var slice in Slices)
        {
            slice.gameObject.SetActive(false);
        }
    }
}
