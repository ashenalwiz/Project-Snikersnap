using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGameMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenuPanel;
    public GameObject[] starObjects; // Array of the 3 star images
    public TextMeshProUGUI resultText;
    public Button restartButton;

    [Header("Audio")]
    public AudioSource starSound; // Sound when stars appear
    public AudioSource menuOpenSound; // Sound when menu appears

    [Header("Animation")]
    public float starDelay = 0.5f; // Delay between each star appearing
    public float menuFadeInTime = 0.5f; // Time for menu to fade in

    [Header("References")]
    public ProgressManager progressManager;
    public GameManager gameManager;

    // Private tracking variables
    private int roundsCompleted = 0;
    private int roundsSkipped = 0;

    private void Awake()
    {
        // Hide menu at start
        if (endMenuPanel != null)
            endMenuPanel.SetActive(false);

        // Find references if not assigned
        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        // Set up restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void Start()
    {
        // Hide all stars initially
        foreach (GameObject star in starObjects)
        {
            if (star != null)
                star.SetActive(false);
        }
    }

    // Call this method from ProgressManager when the game completes
    public void ShowEndMenu(int completed, int skipped)
    {
        roundsCompleted = completed;
        roundsSkipped = skipped;

        // Calculate stars earned
        int stars = CalculateStars(completed, skipped);

        // Update result text
        UpdateResultText(stars);

        // Play open sound
        if (menuOpenSound != null)
            menuOpenSound.Play();

        // Show the panel
        endMenuPanel.SetActive(true);

        // Animate stars appearing
        StartCoroutine(AnimateStars(stars));
    }

    private int CalculateStars(int completed, int skipped)
    {
        int totalRounds = 5; // Based on your maxRounds in ProgressManager

        if (skipped == 0 && completed == totalRounds)
            return 3; // 3 stars for completing all rounds without skipping
        else if (completed >= 3) // 3 or 4 rounds completed
            return 2;
        else if (completed >= 1) // 1 or 2 rounds completed
            return 1;
        else
            return 0; // No rounds completed
    }

    private void UpdateResultText(int stars)
    {
        if (resultText != null)
        {
            switch (stars)
            {
                case 3:
                    resultText.text = "Perfect!";
                    break;
                case 2:
                    resultText.text = "Good job!";
                    break;
                case 1:
                    resultText.text = "Nice try!";
                    break;
                case 0:
                    resultText.text = "Try again!";
                    break;
            }
        }
    }

    private IEnumerator AnimateStars(int starsEarned)
    {
        // Wait a moment before showing stars
        yield return new WaitForSeconds(0.5f);

        // Activate each earned star with a delay
        for (int i = 0; i < starsEarned; i++)
        {
            if (i < starObjects.Length && starObjects[i] != null)
            {
                starObjects[i].SetActive(true);

                // Play star sound for each star
                if (starSound != null)
                    starSound.Play();

                yield return new WaitForSeconds(starDelay);
            }
        }
    }

    private void OnRestartClicked()
    {
        // Hide the end menu
        endMenuPanel.SetActive(false);

        // Reset all stars
        foreach (GameObject star in starObjects)
        {
            if (star != null)
                star.SetActive(false);
        }

        // Restart the game
        if (gameManager != null)
            gameManager.RestartGame();
    }
}