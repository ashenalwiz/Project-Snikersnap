using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI countdownText; // We'll keep this reference but not use it
    public LetterSpawner letterSpawner;
    public BasketController basketController;
    public ProgressManager progressManager;
    public EndGameMenu endGameMenu;

    [Header("Skip Button")]
    public GameObject skipButtonObject;

    [Header("Audio")]
    public AudioSource startGameSound; // Sound for game start
    public AudioSource skipSound; // Sound for skipping rounds

    private bool gameActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Find references if not assigned
        if (letterSpawner == null) letterSpawner = FindObjectOfType<LetterSpawner>();
        if (basketController == null) basketController = FindObjectOfType<BasketController>();
        if (progressManager == null) progressManager = FindObjectOfType<ProgressManager>();
        if (endGameMenu == null) endGameMenu = FindObjectOfType<EndGameMenu>();

        // Make sure end menu is hidden at start
        if (endGameMenu != null && endGameMenu.endMenuPanel != null)
            endGameMenu.endMenuPanel.SetActive(false);

        // Hide countdown text if it exists
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Start the game immediately
        StartGame();
    }

    public void StartGame()
    {
        // Play start sound
        if (startGameSound) startGameSound.Play();

        // Enable game systems
        if (letterSpawner != null)
        {
            letterSpawner.enabled = true;
            letterSpawner.StartLetterSpawning();
        }

        if (basketController != null)
            basketController.enabled = true;

        // Show skip button
        if (skipButtonObject != null)
            skipButtonObject.SetActive(true);

        gameActive = true;
    }

    public void RestartRound()
    {
        // Reset the current round
        if (letterSpawner != null)
        {
            letterSpawner.ClearFallingLetters();
            letterSpawner.GenerateNewLetter();
        }
    }

    public void RestartGame()
    {
        // Full game restart
        gameActive = false;

        // Reset progress and rounds
        if (progressManager != null)
            progressManager.ResetGame();

        // Hide end menu panel if it's visible
        if (endGameMenu != null && endGameMenu.endMenuPanel != null)
            endGameMenu.endMenuPanel.SetActive(false);

        // Start the game again immediately
        StartGame();
    }

    // Method to handle skipping a round
    public void SkipRound()
    {
        if (progressManager != null && gameActive)
        {
            // Play skip sound if available
            if (skipSound) skipSound.Play();

            progressManager.SkipCurrentRound();
        }
    }
}