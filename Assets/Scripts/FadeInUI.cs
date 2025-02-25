using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInUI : MonoBehaviour
{
    public CanvasGroup mainMenuGroup; // Reference to the CanvasGroup
    public float fadeDuration = 2.0f; // Duration of the fade-in effect

    void Start()
    {
        StartCoroutine(FadeIn()); // Start the fade-in effect when the game starts
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            mainMenuGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration); // Smooth fade effect
            yield return null;
        }
        mainMenuGroup.alpha = 1; // Ensure it's fully visible at the end
    }
}
