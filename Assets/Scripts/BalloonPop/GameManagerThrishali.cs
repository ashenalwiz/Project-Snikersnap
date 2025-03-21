using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class GameManagerThrishali : MonoBehaviour
{
    public TMP_Text instructionText, chancesText, scoreText, missedNumberText, gameOverText;
    public Button replayButton, progressButton, exitButton;
    public AudioSource audioSource;
    public AudioClip balloonPopSound, wrongChoiceSound, missedItSound, gameOverSound;
    public AudioClip[] numberAudioClips;
    public GameObject progressPanel, rowPrefab, headerRow;
    public Transform rowContainer; 
    public GameObject backButton;


    private int remainingChances = 5, score = 0;
    private List<int> availableNumbers = new(), missedNumbers = new();
    private Dictionary<int, int> incorrectAttemptsPerNumber = new();
    private int incorrectAttempts = 0;
    private int totalMissedNumbers = 0;
    private float startTime;
    private List<float> timeTakenList = new();
    private bool gameOver = false, showingMissedNumber = false;
    public int TargetNumber { get; private set; }
    [SerializeReference]
    private List<ProgressData> progressList = new List<ProgressData>();
    private Dictionary<int, ProgressData> progressDict = new Dictionary<int, ProgressData>();
    private bool sessionSaved = false;


    private BalloonSpawner balloonSpawner;
    


    private void Start()
    {
        progressPanel.SetActive(false);
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
        {
            availableNumbers.Add(i);
            progressDict[i] = new ProgressData(i, 0, 0f);
        }
        SetNewNumber();

        progressButton.onClick.AddListener(ShowProgressDetails);
    }

    private bool ValidateUI()
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
        TargetNumber = availableNumbers[index];

        instructionText.text = $"Pop number {TargetNumber}!";
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
        float timeTaken = Time.time - startTime;
        int usedAttempts = 5 - remainingChances + 1; // How many chances were used before success

        availableNumbers.Remove(TargetNumber);
        score++;
        scoreText.text = $"Points: {score}";
        showingMissedNumber = false;
        audioSource.PlayOneShot(balloonPopSound);
        timeTakenList.Add(timeTaken);

        // Record progress using the exact chance number used
        RecordAttempt(TargetNumber, usedAttempts, timeTaken);

        if (score >= 10)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(WaitAndSetNewNumber());
        }
    }
    private IEnumerator WaitAndSetNewNumber()
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
        }

        if (remainingChances <= 0)
        {
            StartCoroutine(ShowMissedMessage());
        }
    }
    IEnumerator ShowMissedMessage()
    {
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
    private void RecordAttempt(int number, int usedAttempts, float timeTaken)
    {
        if (!progressDict.ContainsKey(number))
        {
            progressDict[number] = new ProgressData(number, 0, 0f);
        }

        ProgressData data = progressDict[number];

        // Update total attempts
        data.attempts += usedAttempts;

        // Update average time
        data.avgTime = ((data.avgTime * (data.attempts - usedAttempts)) + timeTaken) / data.attempts;

        // Calculate accuracy (Assume penalty factor is 5)
        float penaltyFactor = 5f;
        float calculatedAccuracy = Mathf.Max(0, 100 - (data.avgTime * penaltyFactor));
        data.accuracy = calculatedAccuracy;
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
        //SetBlurVisibility(false);

        StartCoroutine(ShowFinalMessage());
        
    }
    private IEnumerator ShowFinalMessage()
    
    {
        
        gameOverText.text = $"Congratulations!";
        gameOverText.gameObject.SetActive(true);
        audioSource.PlayOneShot(gameOverSound);
        ShowEndGameButtons();
        yield break;
    }
    private void ShowEndGameButtons()
    
    {
        replayButton.gameObject.SetActive(true);
        progressButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);

    }
    private void HideGameOverUI()
    {
        replayButton.gameObject.SetActive(false);
        progressButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }
    private void SaveSessionData(float overallAccuracy)
    {
        string filePath = Application.persistentDataPath + "/sessions.json";

        // Load existing session data if file exists
        SessionDataList sessionDataList = new SessionDataList();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            sessionDataList = JsonUtility.FromJson<SessionDataList>(json);
        }

        // Create new session entry
        SessionData newSession = new SessionData
        {
            sessionID = sessionDataList.sessions.Count + 1,
            date = System.DateTime.Now.ToString("yyyy-MM-dd"),
            accuracy = overallAccuracy
        };

        // Add to session list (Keep all past sessions)
        sessionDataList.sessions.Add(newSession);

        // Save back to JSON file
        string updatedJson = JsonUtility.ToJson(sessionDataList, true);
        File.WriteAllText(filePath, updatedJson);

        Debug.Log("Session saved: " + updatedJson);
    }

    public void ShowProgressDetails()
    {
        progressPanel.SetActive(true);
        backButton.SetActive(true);

        // Clear existing rows except for the header
        foreach (Transform child in rowContainer)
        {
            if (child == headerRow.transform) continue;
            Destroy(child.gameObject);
        }

        float totalAccuracy = 0f;
        int totalNumbers = progressDict.Count;

        // Display accuracy for each number
        foreach (var entry in progressDict)
        {
            int number = entry.Key;
            ProgressData data = entry.Value;

            GameObject row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {

                texts[0].text = number.ToString();
                texts[1].text = data.attempts.ToString();
                texts[2].text = $"{data.accuracy:F1}%";
            }

            totalAccuracy += data.accuracy;
        }

        // Calculate overall accuracy
        float overallAccuracy = totalNumbers > 0 ? totalAccuracy / totalNumbers : 0;

        // Create a final row for overall accuracy
        GameObject overallRow = Instantiate(rowPrefab, rowContainer);
        overallRow.SetActive(true);

        TMP_Text[] overallTexts = overallRow.GetComponentsInChildren<TMP_Text>();
        if (overallTexts.Length >= 3)
        {
            overallTexts[0].text = "Overall Accuracy";
            overallTexts[1].text = "-";
            overallTexts[2].text = $"{overallAccuracy:F1}%";

            overallTexts[0].color = Color.yellow;
            overallTexts[1].color = Color.yellow;
            overallTexts[2].color = Color.yellow;
        }

        // Save session accuracy only if it hasn't been saved
        if (!sessionSaved)
        {
            SaveSessionData(overallAccuracy);
            sessionSaved = true;
        }

        // Display past session records
        ShowRecentSessions();
    }
    private void ShowRecentSessions()
    {
        string filePath = Application.persistentDataPath + "/sessions.json";

        if (!File.Exists(filePath))
        {
            Debug.Log("No session data found.");
            return;
        }

        // Load session data from file
        string json = File.ReadAllText(filePath);
        SessionDataList sessionDataList = JsonUtility.FromJson<SessionDataList>(json);

        if (sessionDataList.sessions.Count == 0)
        {
            Debug.Log("No session data to display.");
            return;
        }

        // Add a separator row for clarity
        GameObject separatorRow = Instantiate(rowPrefab, rowContainer);
        separatorRow.SetActive(true);
        TMP_Text[] separatorTexts = separatorRow.GetComponentsInChildren<TMP_Text>();
        if (separatorTexts.Length >= 3)
        {
            separatorTexts[0].text = "Recent Sessions";
            separatorTexts[1].text = "Date";
            separatorTexts[2].text = "Accuracy";
        }

        // Display the last 5 sessions (or less if there aren't 5)
        int count = Mathf.Min(5, sessionDataList.sessions.Count);
        //List<SessionData> latestSessions = sessionDataList.sessions.GetRange(sessionDataList.sessions.Count - count, count);
        List<SessionData> latestSessions = sessionDataList.sessions.GetRange(sessionDataList.sessions.Count - count, count)
        .OrderByDescending(s => s.sessionID).ToList();

        foreach (SessionData session in latestSessions)
        {
            GameObject row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = $"Session {session.sessionID}";
                texts[1].text = session.date;
                texts[2].text = $"{session.accuracy:F1}%";
            }
        }
    }


    public void CloseProgressPanel()
    {
        progressPanel.SetActive(false);
    }
    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ExitGame()
    {
        Application.Quit();
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

// Class to store progress data
[System.Serializable]
public class ProgressData
{
    public int number, attempts;
    public float avgTime,accuracy;
    public ProgressData(int number, int attempts, float avgTime)
    {
        this.number = number;
        this.attempts = attempts;
        this.avgTime = avgTime;
        this.accuracy = 100;
    }
}

[System.Serializable]
public class SessionData
{
    public int sessionID;
    public string date;
    public float accuracy;
}

[System.Serializable]
public class SessionDataList
{
    public List<SessionData> sessions = new List<SessionData>();
}



