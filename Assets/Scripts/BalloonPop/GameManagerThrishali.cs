using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerThrishali : MonoBehaviour
{
    public TMP_Text instructionText, chancesText, scoreText, missedNumberText, gameOverText;
    public Button replayButton, progressButton, exitButton;
    public AudioSource audioSource;
    public AudioClip balloonPopSound, wrongChoiceSound, missedItSound, gameOverSound;
    public AudioClip[] numberAudioClips;
    public GameObject progressPanel, rowPrefab, headerRow;
    public Transform rowContainer; // Parent where rows will be instantiated
    public GameObject backButton; // Back button


    private int remainingChances = 5, score = 0;
    private List<int> availableNumbers = new(), missedNumbers = new();
    private Dictionary<int, int> incorrectAttemptsPerNumber = new();
    private int incorrectAttempts = 0, totalMissedNumbers = 0;
    private float startTime;
    private List<float> timeTakenList = new();
    private bool gameOver = false, showingMissedNumber = false;
    public int TargetNumber { get; private set; }
    [SerializeField]private List<ProgressData> progressList = new List<ProgressData>();
    private Dictionary<int, ProgressData> progressDict = new Dictionary<int, ProgressData>();


    private BalloonSpawner balloonSpawner;

    private object texts;

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
        //backButton.onClick.AddListener(GoBack);
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
    /*private void RecordAttempt(int number, int usedAttempts, float timeTaken)
    {
        if (!progressDict.ContainsKey(number))
        {
            progressDict[number] = new ProgressData(number, 0, 0f);
        }

        ProgressData data = progressDict[number];

        // Accumulate the total attempts
        data.attempts += usedAttempts;

        // Update average time
        data.avgTime = ((data.avgTime * (data.attempts - usedAttempts)) + timeTaken) / data.attempts;
    }*/
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

        StartCoroutine(ShowFinalMessage());
        SaveProgress(); // save the progress after the game ends
    }

    private IEnumerator ShowFinalMessage()
    // no private
    {
        gameOverText.text = $"Congratulations!\nYou earned {score} Points";
        gameOverText.gameObject.SetActive(true);
        audioSource.PlayOneShot(gameOverSound);
        ShowEndGameButtons();
        yield break;
    }

    private void ShowEndGameButtons()
    // no private
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

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetFloat("AvgTime", timeTakenList.Count > 0 ? timeTakenList.Average() : 0);
        PlayerPrefs.SetString("MissedNumbers", string.Join(",", missedNumbers));
        PlayerPrefs.Save();
    }

    /*private void ShowProgressDetails()
    {
        progressPanel.SetActive(true);

        // Clear previous entries
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // Loop through all numbers (1-10) and display progress
        for (int i = 1; i <= 10; i++)
        {
            if (!progressDict.ContainsKey(i))
            {
                progressDict[i] = new ProgressData(i, 0, 0f);
            }

            ProgressData data = progressDict[i];

            GameObject row = Instantiate(rowPrefab, scrollContent);
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 3)
            {
                texts[0].text = data.number.ToString(); // Number
                texts[1].text = data.attempts.ToString(); // Total attempts across all rounds
                texts[2].text = data.attempts > 0 ? data.avgTime.ToString("F2") + "s" : "-"; // Avg Time
            }
        }

        backButton.gameObject.SetActive(true);
        scrollContent.gameObject.SetActive(true);
    }*/
    public void ShowProgressDetails()
    {
        progressPanel.SetActive(true);
        backButton.SetActive(true);

        // Clear existing rows except for header
        foreach (Transform child in rowContainer)
        {
            if (child == headerRow.transform) continue;
            {
                Destroy(child.gameObject);
            }
        }

        // Generate table rows
        foreach (var entry in progressDict)
        {
            int number = entry.Key;
            ProgressData data = entry.Value;

            GameObject row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);

            // Assign text values
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = number.ToString();
                texts[1].text = data.attempts.ToString();
                texts[2].text = $"{data.accuracy:F1}%";
            }
        }
        backButton.gameObject.SetActive(true);
    }

    public void HideProgressPanel()
    {
        progressPanel.SetActive(false);
        backButton.SetActive(false);
    }






public void CloseProgressPanel()
    {
        progressPanel.SetActive(false);
    }


    private void GoBack()
    {
        progressPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void ReplayGame()
    {
        SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        SaveProgress();
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
    public int number;
    public int attempts;
    public float avgTime;
    public float accuracy;

    public ProgressData(int number, int attempts, float avgTime)
    {
        this.number = number;
        this.attempts = attempts;
        this.avgTime = avgTime;
        this.accuracy = 100;
    }
}
/*using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerThrishali : MonoBehaviour
{
    public TMP_Text instructionText, chancesText, scoreText, missedNumberText, gameOverText;
    public Button replayButton, progressButton, exitButton, backButton;
    public AudioSource audioSource;
    public AudioClip balloonPopSound, wrongChoiceSound, missedItSound, gameOverSound;
    public AudioClip[] numberAudioClips;
    public GameObject progressPanel, rowPrefab;
    public Transform scrollContent; // The content inside the Scroll View

    private int remainingChances = 5, score = 0;
    private List<int> availableNumbers = new(), missedNumbers = new();
    private Dictionary<int, int> incorrectAttemptsPerNumber = new();
    private int incorrectAttempts = 0, totalMissedNumbers = 0;
    private float startTime;
    private List<float> timeTakenList = new();
    private bool gameOver = false, showingMissedNumber = false;
    public int TargetNumber { get; private set; }
    //private List<ProgressData> progressList = new List<ProgressData>();
    private Dictionary<int, ProgressData> progressDict = new Dictionary<int, ProgressData>();


    private BalloonSpawner balloonSpawner;

    private object texts;

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
        backButton.onClick.AddListener(GoBack);
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
        availableNumbers.Remove(TargetNumber);
        score++;
        scoreText.text = $"Points: {score}";
        showingMissedNumber = false;
        audioSource.PlayOneShot(balloonPopSound);
        timeTakenList.Add(timeTaken);

        // Record progress
        RecordAttempt(TargetNumber, timeTaken);

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
    private void RecordAttempt(int number, float timeTaken)
    {
        if (progressDict.ContainsKey(number))
        {
            ProgressData data = progressDict[number];
            data.attempts++;
            data.avgTime = ((data.avgTime * (data.attempts - 1)) + timeTaken) / data.attempts;
        }
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
        SaveProgress(); // save the progress after the game ends
    }

    private IEnumerator ShowFinalMessage()
        // no private
    {
        gameOverText.text = $"Congratulations!\nYou earned {score} Points";
        gameOverText.gameObject.SetActive(true);
        audioSource.PlayOneShot(gameOverSound);
        ShowEndGameButtons();
        yield break;
    }

    private void ShowEndGameButtons()
        // no private
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

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetFloat("AvgTime", timeTakenList.Count > 0 ? timeTakenList.Average() : 0);
        PlayerPrefs.SetString("MissedNumbers", string.Join(",", missedNumbers));
        PlayerPrefs.Save();
    }

    private void ShowProgressDetails()
    {
        progressPanel.SetActive(true);

        // Clear previous entries
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // Populate scroll view with missed numbers
        foreach (var number in missedNumbers)
        {
            if (progressDict.TryGetValue(number, out ProgressData data))
            {
                GameObject row = Instantiate(rowPrefab, scrollContent);
                TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

                if (texts.Length >= 3)
                {
                    texts[0].text = data.number.ToString(); // Number
                    texts[1].text = data.attempts > 0 ? data.attempts.ToString() : "-"; // Attempts
                    texts[2].text = data.attempts > 0 ? data.avgTime.ToString("F2") + "s" : "-"; // Avg Time
                }
            }
        }

        progressPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
        scrollContent.gameObject.SetActive(true);
    }
    public void CloseProgressPanel()
    {
        progressPanel.SetActive(false);
    }
    

    private void GoBack()
    {
        progressPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void ReplayGame()
    {
        SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        SaveProgress();
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
    public int number;
    public int attempts;
    public float avgTime;

    public ProgressData(int number, int attempts, float avgTime)
    {
        this.number = number;
        this.attempts = attempts;
        this.avgTime = avgTime;
    }
}*/

