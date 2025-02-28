using UnityEngine;
using System.Collections;

public class MainMenuTransition : MonoBehaviour
{
    public CanvasGroup canvasGroup;  // Reference to the CanvasGroup
    public float fadeDuration = 2f;  // Duration of the fade-in effect

    void Start()
    {
        // Ensure the canvas starts fully transparent
        canvasGroup.alpha = 0;

        // Start the fade-in effect
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
    }
}
