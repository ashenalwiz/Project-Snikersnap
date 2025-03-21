using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownController : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public float delayBetweenNumbers = 0.8f;
    public AudioSource countdownSound; // Optional sound for each countdown number
    public AudioSource goSound; // Optional special sound for "GO!"

    private void Start()
    {
        // Make sure the text component is assigned
        if (countdownText == null)
        {
            Debug.LogWarning("Countdown Text is not assigned to CountdownController!");
            return;
        }

        // Start the countdown sequence
        StartCoroutine(PlayCountdown());
    }

    private IEnumerator PlayCountdown()
    {
        // Make sure the countdown text is visible
        countdownText.gameObject.SetActive(true);

        // Display "3"
        countdownText.text = "3";
        countdownText.fontSize = 100;
        if (countdownSound) countdownSound.Play();
        yield return new WaitForSeconds(delayBetweenNumbers);

        // Display "2"
        countdownText.text = "2";
        countdownText.fontSize = 100;
        if (countdownSound) countdownSound.Play();
        yield return new WaitForSeconds(delayBetweenNumbers);

        // Display "GO!"
        countdownText.text = "GO!";
        countdownText.fontSize = 100;
        if (goSound) goSound.Play();
        yield return new WaitForSeconds(delayBetweenNumbers);

        // Hide the countdown text
        countdownText.gameObject.SetActive(false);
    }
}
