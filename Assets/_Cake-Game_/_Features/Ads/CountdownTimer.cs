using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private int initialCountdownValue = 3; // Set the initial countdown value in the Inspector
    private int countdownValue;
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        // Get the TextMeshProUGUI component attached to the GameObject
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Set the initial countdown value
        countdownValue = initialCountdownValue;

        // Start the countdown when the script is enabled
        StartCountdown();
    }

    private void StartCountdown()
    {
        // Update the text to show the initial countdown value
        UpdateCountdownText();

        // Start a coroutine to count down
        StartCoroutine(CountdownCoroutine());
    }

    private void UpdateCountdownText()
    {
        // Update the TextMeshProUGUI text to display the current countdown value
        textMeshPro.text = countdownValue.ToString();
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        // Wait for a short duration before starting the countdown
        yield return new WaitForSeconds(1f);
        countdownValue--;

        // Update the countdown text
        UpdateCountdownText();

        // Count down each second until the countdown reaches 0
        while(countdownValue > 0)
        {
            yield return new WaitForSeconds(1f);
            countdownValue--;

            // Update the countdown text
            UpdateCountdownText();
        }

        // Optionally, perform any action when the countdown reaches 0
        // For example, you can call a method or trigger an event.
        OnCountdownComplete();
    }

    private void OnCountdownComplete()
    {
        //Debug.Log("Countdown complete! You can add your custom logic here.");
    }
}
