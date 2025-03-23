using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    public Slider progressBar;
    public TextMeshProUGUI roundCounterText;
    public Button skipButton; // Reference to the Skip Button
    public EndGameMenu endGameMenu; // Reference to the EndGameMenu component

    private int progressValue = 0;
    private int maxProgress = 5;
    private int currentRound = 1;
    private int maxRounds = 5;

    // Track completion statistics
    private int roundsCompleted = 0;
    private int roundsSkipped = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        progressBar.value = progressValue;
        UpdateRoundCounter();

        // Add button click listener
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCurrentRound);
        }

        // Find end menu if not assigned
        if (endGameMenu == null)
            endGameMenu = FindObjectOfType<EndGameMenu>();
    }

    public void UpdateProgress(int change)
    {
        progressValue = Mathf.Clamp(progressValue + change, 0, maxProgress);
        progressBar.value = progressValue;

        if (progressValue >= maxProgress)
        {
            roundsCompleted++;
            HandleRoundComplete();
        }
    }

    private void HandleRoundComplete()
    {
        currentRound++;

        if (currentRound <= maxRounds)
        {
            // Reset progress but continue game
            progressValue = 0;
            progressBar.value = progressValue;
            UpdateRoundCounter();

            // Use GameManager to restart the round
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartRound();
            }
            else
            {
                // Fallback if GameManager not available
                LetterSpawner letterSpawner = FindObjectOfType<LetterSpawner>();
                if (letterSpawner != null)
                {
                    letterSpawner.GenerateNewLetter();
                    letterSpawner.ClearFallingLetters();
                }
            }
        }
        else
        {
            // Game is completely finished after all rounds
            GameComplete();
        }
    }

    private void UpdateRoundCounter()
    {
        if (roundCounterText != null)
        {
            roundCounterText.text = "Round: " + currentRound + "/" + maxRounds;
        }
    }

    private void GameComplete()
    {
        Debug.Log("Game Complete! All 5 rounds finished.");
        Debug.Log("Rounds completed: " + roundsCompleted + ", Rounds skipped: " + roundsSkipped);

        // Stop the letter spawner
        LetterSpawner letterSpawner = FindObjectOfType<LetterSpawner>();
        if (letterSpawner != null)
        {
            letterSpawner.StopLetterSpawning();
            letterSpawner.ClearFallingLetters();
        }

        // Show end menu with results
        if (endGameMenu != null)
        {
            endGameMenu.ShowEndMenu(roundsCompleted, roundsSkipped);
        }
        else
        {
            // Fallback if no end menu - just restart
            ResetGame();
        }
    }

    // Public method to reset game state
    public void ResetGame()
    {
        currentRound = 1;
        progressValue = 0;
        roundsCompleted = 0;
        roundsSkipped = 0;
        progressBar.value = progressValue;
        UpdateRoundCounter();
    }

    // Skip current round method - unlimited skips
    public void SkipCurrentRound()
    {
        // Count this as a skipped round
        roundsSkipped++;

        // Force complete the current round
        progressValue = maxProgress;
        progressBar.value = progressValue;
        HandleRoundComplete();
    }
}