using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGameMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenuPanel;
    public GameObject[] starObjects;
    public TextMeshProUGUI resultText;
    public Button restartButton;

    [Header("Audio")]
    public AudioSource starSound;
    public AudioSource menuOpenSound;

    [Header("Animation")]
    public float starDelay = 0.5f;
    public float menuFadeInTime = 0.5f;

    [Header("References")]
    public ProgressManager progressManager;
    public GameManager gameManager;

    private int roundsCompleted = 0;
    private int roundsSkipped = 0;

    // Initialize the menu and set up references
    private void Awake()
    {
        if (endMenuPanel != null)
            endMenuPanel.SetActive(false);

        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    // Hide all stars at the start
    private void Start()
    {
        foreach (GameObject star in starObjects)
        {
            if (star != null)
                star.SetActive(false);
        }
    }

    // Show the end menu with results
    public void ShowEndMenu(int completed, int skipped)
    {
        roundsCompleted = completed;
        roundsSkipped = skipped;

        int stars = CalculateStars(completed, skipped);
        UpdateResultText(stars);

        if (menuOpenSound != null)
            menuOpenSound.Play();

        endMenuPanel.SetActive(true);
        StartCoroutine(AnimateStars(stars));
    }

    // Calculate the number of stars earned
    private int CalculateStars(int completed, int skipped)
    {
        int totalRounds = 5;

        if (skipped == 0 && completed == totalRounds)
            return 3;
        else if (completed >= 3)
            return 2;
        else if (completed >= 1)
            return 1;
        else
            return 0;
    }

    // Update the result text based on earned stars
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

    // Animate the stars appearing one by one
    private IEnumerator AnimateStars(int starsEarned)
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < starsEarned; i++)
        {
            if (i < starObjects.Length && starObjects[i] != null)
            {
                starObjects[i].SetActive(true);

                if (starSound != null)
                    starSound.Play();

                yield return new WaitForSeconds(starDelay);
            }
        }
    }

    // Handle restart button click
    private void OnRestartClicked()
    {
        endMenuPanel.SetActive(false);

        foreach (GameObject star in starObjects)
        {
            if (star != null)
                star.SetActive(false);
        }

        if (gameManager != null)
            gameManager.RestartGame();
    }
}