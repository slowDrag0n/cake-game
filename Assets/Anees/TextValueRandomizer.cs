using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextValueRandomizer : MonoBehaviour
{
    public string[] Lines;

    TextMeshProUGUI textToUpdate;

    private void Awake()
    {
        textToUpdate = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        textToUpdate.text = Lines[Random.Range(0, Lines.Length)];
    }
}
