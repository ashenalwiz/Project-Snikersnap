using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text chancesText;
    public TMP_Text scoreText;
    public TMP_Text missedNumberText;
    public TMP_Text gameOverText;
    public TMP_Text progressText;

    public int TargetNumber { get; private set; }

    public Button replayButton;
    public Button progressButton;
    public Button exitButton;
    public Button backButton;

    public AudioSource audioSource;
    public AudioClip balloonPopSound;
    public AudioClip wrongChoiceSound;
    public AudioClip missedItSound;
    public AudioClip gameOverSound;
    public AudioClip[] numberAudioClips; // Store audio clips for numbers 1-10

    private int remainingChances = 5;
    private int score = 0;
    private readonly List<int> availableNumbers = new();
    private bool showingMissedNumber = false;
    private bool gameOver = false;
    private BalloonSpawner balloonSpawner;

    private List<int> missedNumbers = new();
    private Dictionary<int, int> incorrectAttemptsPerNumber = new();

    private int incorrectAttempts = 0;
    private int totalMissedNumbers = 0;
    private float startTime;
    private List<float> timeTakenList = new();

    void Start() 
    {

        progressText.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        HideGameOverUI();
        if (!ValidateUI()) return;

        balloonSpawner = FindAnyObjectByType<BalloonSpawner>();
        if (balloonSpawner == null)
        {
            Debug.LogError("BalloonSpawner not found!");
            return;
        }

        gameOverText.gameObject.SetActive(false);
        for (int i = 1; i <= 10; i++)
            availableNumbers.Add(i);

        SetNewNumber();
    }

    bool ValidateUI()
    {
        if (instructionText == null || chancesText == null || scoreText == null ||
            missedNumberText == null || gameOverText == null || audioSource == null)
        {
            Debug.LogError("UI elements or AudioSource are not assigned! Assign them in the Inspector.");
            return false;
        }
        return true;
    }

    public void SetNewNumber()
    {
        if (!ValidateUI() || gameOver) return;

        if (availableNumbers.Count == 0 || score >= 10)
        {
            GameOver();
            return;
        }

        int index = Random.Range(0, availableNumbers.Count);
        TargetNumber = availableNumbers[index]; // Get the value, not the index
        instructionText.text = $"Pop number {TargetNumber}!"; // Fix incorrect indexing

        remainingChances = 5;
        chancesText.text = $"Chances: {remainingChances}";
        missedNumberText.gameObject.SetActive(false);

        incorrectAttempts = 0;
        startTime = Time.time;

        PlayNumberAudio(TargetNumber);
        if (!gameOver)
            balloonSpawner.SpawnBalloons();
    }


    public void CorrectNumberPopped()
    {
        availableNumbers.Remove(TargetNumber);
        score++;
        scoreText.text = $"Points: {score}";
        showingMissedNumber = false;
        audioSource.PlayOneShot(balloonPopSound);

        // Record time and stats in ProgressManager
        ProgressManager.Instance.RecordTimeTaken(Time.time - startTime);

        if (score >= 10)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(WaitAndSetNewNumber());
        }
    }


    IEnumerator WaitAndSetNewNumber()
    {
        yield return new WaitForSeconds(0.5f);
        SetNewNumber();
    }

    public void CheckNumber(int number)
    {
        if (showingMissedNumber || gameOver) return;

        remainingChances--;
        chancesText.text = $"Chances: {remainingChances}";

        if (number != TargetNumber)
        {
            incorrectAttempts++;
            if (!incorrectAttemptsPerNumber.ContainsKey(TargetNumber))
                incorrectAttemptsPerNumber[TargetNumber] = 0;
            incorrectAttemptsPerNumber[TargetNumber]++;
            audioSource.PlayOneShot(wrongChoiceSound);
            ProgressManager.Instance.RecordIncorrectAttempt();
        }


        if (remainingChances <= 0)
        {
            StartCoroutine(ShowMissedMessage());
        }
    }

    IEnumerator ShowMissedMessage()
    {
        ProgressManager.Instance.RecordMissedNumber(TargetNumber);

        showingMissedNumber = true;
        instructionText.text = "You missed!";
        missedNumberText.text = $"The number was {TargetNumber}";
        missedNumberText.gameObject.SetActive(true);

        balloonSpawner.StopSpawningBalloons();
        balloonSpawner.HideAllBalloons();

        totalMissedNumbers++;
        missedNumbers.Add(TargetNumber);

        audioSource.PlayOneShot(missedItSound);

        yield return new WaitForSeconds(5f);

        missedNumberText.gameObject.SetActive(false);
        showingMissedNumber = false;
        SetNewNumber();
        balloonSpawner.ShowAllBalloons();
        balloonSpawner.ResumeSpawningBalloons();
    }

    void GameOver()
    {
        gameOver = true;
        balloonSpawner.StopSpawningBalloons();
        balloonSpawner.HideAllBalloons();

        instructionText.gameObject.SetActive(false);
        chancesText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        missedNumberText.gameObject.SetActive(false);

        StartCoroutine(ShowFinalMessage());

        // Save progress after the game ends
        ProgressManager.Instance.SaveProgress();

    }


    IEnumerator ShowFinalMessage()
    {
        gameOverText.text = $"Congratulations!\nYou earned {score} Points";
        gameOverText.gameObject.SetActive(true);
        audioSource.PlayOneShot(gameOverSound);

        // Immediately show the buttons
        ShowEndGameButtons();

        yield break; // Ends the coroutine immediately
    }


    void ShowEndGameButtons()
    {
        replayButton.gameObject.SetActive(true);
        progressButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);

        replayButton.onClick.AddListener(ReplayGame);
        progressButton.onClick.AddListener(ShowProgressDetails);
        exitButton.onClick.AddListener(ExitGame);
    }

    void HideGameOverUI()
    {
        replayButton.gameObject.SetActive(false);
        progressButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);
    }

    /*public void ReplayGame()
    {
        SaveProgress();
        ResetGame();
    }*/
    public void ReplayGame()
    {
        // Save current progress before restarting (optional)
        //ProgressManager.Instance.SaveProgress();

        // Clear existing session and start a new session
        ProgressManager.Instance.StartNewSession();  // Start a new session

        // Reload the scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the game
    }



    public void ExitGame()
    {
        SaveProgress();
        Application.Quit();
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetFloat("AvgTime", timeTakenList.Count > 0 ? timeTakenList.Average() : 0);
        PlayerPrefs.SetString("MissedNumbers", string.Join(",", missedNumbers));
        PlayerPrefs.Save();
    }

    void ShowProgressDetails()
    {
        gameOverText.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        progressText.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(false);
        progressButton.gameObject.SetActive(false);

        // Get current session data from ProgressManager
        ProgressData currentSession = ProgressManager.Instance.progressHistory.allSessions.LastOrDefault();

        if (currentSession != null)
        {
            float avgTime = currentSession.correctAttempts > 0
                ? (currentSession.totalTimeTaken / currentSession.correctAttempts)
                : 0f;

            string missedNumbersList = currentSession.missedNumbers.Count > 0
                ? string.Join(", ", currentSession.missedNumbers)
                : "None";

            progressText.text = $"-Game Summary-\n" +
                                $"  Score: {score}\n" +
                                $"  Avg Time: {avgTime:F2} sec\n" +
                                $"  Missed: {missedNumbersList}";
        }
        else
        {
            progressText.text = "-Game Summary-\n No progress data available.";
        }
    }
    public void BackToGameUI()
    {
        progressText.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        // Restore previous UI state if needed
        gameOverText.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(true);
        progressButton.gameObject.SetActive (true);
        exitButton.gameObject.SetActive(true);
    }


    public void PlayNumberAudio(int number)
    {
        if (number >= 1 && number <= 10 && numberAudioClips[number - 1] != null)
        {
            audioSource.PlayOneShot(numberAudioClips[number - 1]);
        }
    }
    public bool IsShowingMissedNumber() => showingMissedNumber;
    public bool IsGameOver() => gameOver;
    

}

