using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance; // Singleton instance

    public Slider progressBar; // UI progress bar
    public TextMeshProUGUI roundCounterText; // Displays the current round
    public Button skipButton; // Skip button reference
    public EndGameMenu endGameMenu; // Reference to the end game menu

    private int progressValue = 0; // Current progress
    private int maxProgress = 5; // Progress needed to complete a round
    private int currentRound = 1; // Current round number
    private int maxRounds = 5; // Total number of rounds

    // Track completion statistics
    private int roundsCompleted = 0;
    private int roundsSkipped = 0;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        progressBar.value = progressValue;
        UpdateRoundCounter();

        // Add skip button functionality
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCurrentRound);
        }

        // Find the end menu if not manually assigned
        if (endGameMenu == null)
            endGameMenu = FindObjectOfType<EndGameMenu>();
    }

    // Updates the progress and checks if a round is completed
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

    // Handles the completion of a round
    private void HandleRoundComplete()
    {
        currentRound++;

        if (currentRound <= maxRounds)
        {
            // Reset progress and continue to the next round
            progressValue = 0;
            progressBar.value = progressValue;
            UpdateRoundCounter();

            // Restart the round using GameManager if available
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartRound();
            }
            else
            {
                // Fallback: reset the letter spawner manually
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
            // All rounds completed
            GameComplete();
        }
    }

    // Updates the UI text for round count
    private void UpdateRoundCounter()
    {
        if (roundCounterText != null)
        {
            roundCounterText.text = "Round: " + currentRound + "/" + maxRounds;
        }
    }

    // Handles the completion of all rounds
    private void GameComplete()
    {
        Debug.Log("Game Complete! All rounds finished.");
        Debug.Log("Rounds completed: " + roundsCompleted + ", Rounds skipped: " + roundsSkipped);

        // Stop letter spawning and clear letters
        LetterSpawner letterSpawner = FindObjectOfType<LetterSpawner>();
        if (letterSpawner != null)
        {
            letterSpawner.StopLetterSpawning();
            letterSpawner.ClearFallingLetters();
        }

        // Show the end game menu or restart if unavailable
        if (endGameMenu != null)
        {
            endGameMenu.ShowEndMenu(roundsCompleted, roundsSkipped);
        }
        else
        {
            ResetGame();
        }
    }

    // Resets the game state to the initial values
    public void ResetGame()
    {
        currentRound = 1;
        progressValue = 0;
        roundsCompleted = 0;
        roundsSkipped = 0;
        progressBar.value = progressValue;
        UpdateRoundCounter();
    }

    // Skips the current round and forces completion
    public void SkipCurrentRound()
    {
        roundsSkipped++;
        progressValue = maxProgress;
        progressBar.value = progressValue;
        HandleRoundComplete();
    }
}
