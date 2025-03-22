
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class WordGameManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string word;
        public string option1;
        public string option2;
        public AudioClip audioClip;
        public string correctAnswer;
    }

    [System.Serializable]
    public class GameRound
    {
        public int questionsAttempted;
        public int correctAnswers;
        public int soundsPlayed;
    }

    public Question[] questions;
    private int currentQuestionIndex = 0;
    private int score = 0;

    public AudioSource audioSource;
    public AudioSource messageAudioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    public Button replayButton;
    public Button option1Button;
    public Button option2Button;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverImage;

    private GameRound currentRound = new GameRound();
    private string saveFilePath;
    private List<GameRound> roundsHistory = new List<GameRound>();

    void Start()
    {
        saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, "progress.json");

        Debug.Log("Progress file path: " + saveFilePath);

        ResetGame();

        gameOverImage.SetActive(false);
        scoreText.text = "Score: " + score;

        replayButton.onClick.AddListener(PlayAudio);
        option1Button.onClick.AddListener(() => CheckAnswer(option1Text.text));
        option2Button.onClick.AddListener(() => CheckAnswer(option2Text.text));

        LoadPreviousRounds();
        LoadQuestion();
    }

    public void ResetGame()
    {
        currentRound = new GameRound();  // Reset the round data
        score = 0;
        currentQuestionIndex = 0;
        gameOverImage.SetActive(false);
        scoreText.text = "Score: " + score;
    }

    void LoadQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("Questions array is empty!");
            return;
        }

        if (currentQuestionIndex >= questions.Length)
        {
            GameOver();
            return;
        }

        Question currentQuestion = questions[currentQuestionIndex];
        option1Text.text = currentQuestion.option1;
        option2Text.text = currentQuestion.option2;
        audioSource.clip = currentQuestion.audioClip;

        messageText.text = "";
        currentRound.soundsPlayed++;  // Increment the sound played counter
    }

    void PlayAudio()
    {
        audioSource.Play();
        currentRound.soundsPlayed++;  // Increment sound played each time replay is clicked
    }

    void CheckAnswer(string selectedAnswer)
    {
        Question currentQuestion = questions[currentQuestionIndex];
        currentRound.questionsAttempted++;  // Increment question attempted counter

        if (selectedAnswer == currentQuestion.correctAnswer)
        {
            messageText.text = "Correct Answer!";
            messageText.color = Color.black;
            messageAudioSource.PlayOneShot(correctSound);
            score++;
            currentRound.correctAnswers++;  // Increment correct answers counter
        }
        else
        {
            messageText.text = "Wrong Answer!";
            messageText.color = Color.red;
            messageAudioSource.PlayOneShot(wrongSound);
        }

        scoreText.text = "Score: " + score;
        Invoke(nameof(NextQuestion), 1.5f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Length)
        {
            GameOver();
        }
        else
        {
            LoadQuestion();
        }
    }

    void GameOver()
    {
        // Save the round progress at the end of the game
        roundsHistory.Add(currentRound);  // Add current round to the history

        // Save all rounds' progress into the JSON file
        SaveProgress();

        gameOverImage.SetActive(true);
        Debug.Log("Game Over! Stats: " +
                  "\nQuestions Attempted: " + currentRound.questionsAttempted +
                  "\nCorrect Answers: " + currentRound.correctAnswers +
                  "\nSounds Played: " + currentRound.soundsPlayed);
    }

    void SaveProgress()
    {
        // Serialize the entire list of rounds to JSON and save it to a file
        string json = JsonUtility.ToJson(new RoundHistory { rounds = roundsHistory }, true);
        File.WriteAllText(saveFilePath, json);
    }

    void LoadPreviousRounds()
    {
        // Load the rounds history from the JSON file if it exists
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            RoundHistory loadedData = JsonUtility.FromJson<RoundHistory>(json);
            roundsHistory = loadedData.rounds;
        }
    }

    [System.Serializable]
    public class RoundHistory
    {
        public List<GameRound> rounds;  // List of all rounds
    }
}
