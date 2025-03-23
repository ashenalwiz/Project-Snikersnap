using UnityEngine;
using System.Collections;

public class WelcomeScreen : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject loginUI;
    public float displayTime = 6f; // Time before fade-out starts
    public float fadeDuration = 2f; // Time taken to fade out

    private CanvasGroup canvasGroup;

    void Start()
    {
        loginUI.SetActive(false); // Hide login UI at start

        canvasGroup = welcomePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing on WelcomePanel!");
            return;
        }

        StartCoroutine(FadeOutWelcome());
    }

    IEnumerator FadeOutWelcome()
    {
        yield return new WaitForSeconds(displayTime); // Wait before fading

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        welcomePanel.SetActive(false); // Hide after fade-out
        loginUI.SetActive(true);       // Show Login UI
    }
}
