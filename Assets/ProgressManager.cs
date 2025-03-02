using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    public Slider progressBar;
    public TextMeshProUGUI roundCounterText;
    private int progressValue = 0;
    private int maxProgress = 5;
    private int currentRound = 1;
    private int maxRounds = 5;

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
    }

    public void UpdateProgress(int change)
    {
        progressValue = Mathf.Clamp(progressValue + change, 0, maxProgress);
        progressBar.value = progressValue;

        if (progressValue >= maxProgress)
        {
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

        // Use GameManager to restart the game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // Fallback if GameManager not available
            ResetGame();

            // Reset the letter spawner
            LetterSpawner letterSpawner = FindObjectOfType<LetterSpawner>();
            if (letterSpawner != null)
            {
                letterSpawner.GenerateNewLetter();
                letterSpawner.ClearFallingLetters();
            }
        }
    }

    // Public method to reset game state
    public void ResetGame()
    {
        currentRound = 1;
        progressValue = 0;
        progressBar.value = progressValue;
        UpdateRoundCounter();
    }
}