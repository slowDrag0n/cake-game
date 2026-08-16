using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForCheckDevice : MonoBehaviour
{
    private void Awake()
    {
        if (SystemInfo.systemMemorySize <= 1024)
        {
            PlayerPrefs.SetInt("NoAds", 1);
            gameObject.SetActive(false);
        }
        if (SystemInfo.systemMemorySize <= 2048)
        {
            PlayerPrefs.SetInt("MaxAdStop", 1);
        }
    }
}
