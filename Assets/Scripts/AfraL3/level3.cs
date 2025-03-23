using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

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
    public class RoundData
    {
        public int roundID;
        public int questionsAttempted;
        public int correctAnswers;
        public int soundsPlayed;
    }

    [System.Serializable]
    public class GameData
    {
        public List<RoundData> rounds = new List<RoundData>();
    }

    public Question[] questions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int soundsPlayed = 0;
    private GameData gameData = new GameData();

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

    private RoundData currentRound;
    private string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "Task7UserProgress.json"); // Updated path
        LoadProgress();
        gameOverImage.SetActive(false);
        scoreText.text = "Score: " + score;

        replayButton.onClick.AddListener(PlayAudio);
        option1Button.onClick.AddListener(() => CheckAnswer(option1Text.text));
        option2Button.onClick.AddListener(() => CheckAnswer(option2Text.text));

        StartNewRound();
        LoadQuestion();
    }

    void StartNewRound()
    {
        currentRound = new RoundData
        {
            roundID = gameData.rounds.Count + 1,
            questionsAttempted = 0,
            correctAnswers = 0,
            soundsPlayed = 0
        };
    }

    void LoadQuestion()
    {
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
    }

    void PlayAudio()
    {
        audioSource.Play();
        currentRound.soundsPlayed++;
        Debug.Log("Sound played. Total count: " + currentRound.soundsPlayed);
    }

    void CheckAnswer(string selectedAnswer)
    {
        Question currentQuestion = questions[currentQuestionIndex];
        currentRound.questionsAttempted++;

        if (selectedAnswer == currentQuestion.correctAnswer)
        {
            messageText.text = "Correct Answer!";
            messageText.color = Color.black;
            messageAudioSource.PlayOneShot(correctSound);
            score++;
            currentRound.correctAnswers++;
            scoreText.text = "Score: " + score;
        }
        else
        {
            messageText.text = "Wrong Answer";
            messageText.color = Color.red;
            messageAudioSource.PlayOneShot(wrongSound);
        }

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
        gameOverImage.SetActive(true);
        gameData.rounds.Add(currentRound);
        SaveProgress();
        Debug.Log("Game Over! Progress saved.");
    }

    void SaveProgress()
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(savePath, json); // Save to persistentDataPath
        Debug.Log("Progress saved to: " + savePath);
    }

    void LoadProgress()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            if (gameData == null)
            {
                gameData = new GameData();
            }
            Debug.Log("Progress loaded successfully from: " + savePath);
        }
        else
        {
            Debug.Log("No progress file found at: " + savePath);
        }
    }
}