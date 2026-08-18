using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private int initialCountdownValue = 3; 
    private int countdownValue;
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        countdownValue = initialCountdownValue;

        StartCountdown();
    }

    private void StartCountdown()
    {
        UpdateCountdownText();

        StartCoroutine(CountdownCoroutine());
    }

    private void UpdateCountdownText()
    {
        textMeshPro.text = countdownValue.ToString();
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        yield return new WaitForSeconds(1f);
        countdownValue--;

        UpdateCountdownText();

        // Count down each second until the countdown reaches 0
        while(countdownValue > 0)
        {
            yield return new WaitForSeconds(1f);
            countdownValue--;

            UpdateCountdownText();
        }

        OnCountdownComplete();
    }

    private void OnCountdownComplete() { }
}
