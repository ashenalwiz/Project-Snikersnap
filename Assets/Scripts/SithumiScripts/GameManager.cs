using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI countdownText;
    public LetterSpawner letterSpawner;
    public BasketController basketController;
    public ProgressManager progressManager;
    public EndGameMenu endGameMenu; // Added reference to end game menu

    [Header("Skip Button")]
    public GameObject skipButtonObject; // Reference to the Skip Button GameObject

    [Header("Countdown Settings")]
    public float countdownDuration = 1.0f; // Time each number displays for
    public AudioSource countdownSound; // Optional sound effect
    public AudioSource startGameSound; // Optional sound for game start
    public AudioSource skipSound; // Optional sound for skipping rounds

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

        // Disable game systems until countdown finishes
        if (letterSpawner != null) letterSpawner.enabled = false;
        if (basketController != null) basketController.enabled = false;

        // Hide skip button during countdown if it exists
        if (skipButtonObject != null)
            skipButtonObject.SetActive(false);

        // Make sure end menu is hidden at start
        if (endGameMenu != null && endGameMenu.endMenuPanel != null)
            endGameMenu.endMenuPanel.SetActive(false);

        // Start the countdown
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // Make sure countdown text is visible
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            // Countdown from 3
            countdownText.text = " 3";
            if (countdownSound) countdownSound.Play();
            yield return new WaitForSeconds(countdownDuration);

            countdownText.text = " 2";
            if (countdownSound) countdownSound.Play();
            yield return new WaitForSeconds(countdownDuration);

            countdownText.text = " 1";
            if (countdownSound) countdownSound.Play();
            yield return new WaitForSeconds(countdownDuration);

            countdownText.text = "GO!";
            if (startGameSound) startGameSound.Play();
            yield return new WaitForSeconds(countdownDuration);

            // Hide countdown text
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // If no text component, just wait 3 seconds
            yield return new WaitForSeconds(3f);
        }

        // Start the game
        StartGame();
    }

    public void StartGame()
    {
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

        // Hide skip button during countdown
        if (skipButtonObject != null)
            skipButtonObject.SetActive(false);

        // Hide end menu panel if it's visible
        if (endGameMenu != null && endGameMenu.endMenuPanel != null)
            endGameMenu.endMenuPanel.SetActive(false);

        // Start countdown again
        StartCoroutine(StartCountdown());
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