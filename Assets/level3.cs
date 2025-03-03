using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public Question[] questions;
    private int currentQuestionIndex = 0;
    private int score = 0; // Score variable to keep track of the correct answers

    public AudioSource audioSource;
    public AudioSource messageAudioSource;  // New AudioSource for messages
    public AudioClip correctSound; // Correct answer sound
    public AudioClip wrongSound;   // Wrong answer sound

    public Button replayButton;
    public Button option1Button;
    public Button option2Button;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public TextMeshProUGUI messageText;  // TextMeshProUGUI for messages
    public TextMeshProUGUI scoreText;    // TextMeshProUGUI to display the score
    public GameObject gameOverImage; // Game Over Image

    void Start()
    {
        if (messageText == null)
        {
            Debug.LogError("MessageText UI element is not assigned in the Inspector!");
            return;
        }

        if (scoreText == null)
        {
            Debug.LogError("ScoreText UI element is not assigned in the Inspector!");
            return;
        }

        if (gameOverImage == null)
        {
            Debug.LogError("GameOverImage UI element is not assigned in the Inspector!");
            return;
        }

        gameOverImage.SetActive(false); // Hide Game Over image at the start
        scoreText.text = "Score: " + score;  // Initialize the score display

        LoadQuestion();
        replayButton.onClick.AddListener(PlayAudio);
        option1Button.onClick.AddListener(() => CheckAnswer(option1Text.text));
        option2Button.onClick.AddListener(() => CheckAnswer(option2Text.text));
    }

    void LoadQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("Questions array is empty or not assigned in the Inspector!");
            return;
        }

        if (currentQuestionIndex >= questions.Length)
        {
            Debug.Log("Game Over! No more questions.");
            GameOver();
            return;
        }

        Question currentQuestion = questions[currentQuestionIndex];

        option1Text.text = currentQuestion.option1;
        option2Text.text = currentQuestion.option2;
        audioSource.clip = currentQuestion.audioClip;

        messageText.text = ""; // Clear message at the start of each question
    }

    void PlayAudio()
    {
        audioSource.Play();
    }

    void CheckAnswer(string selectedAnswer)
    {
        Question currentQuestion = questions[currentQuestionIndex];

        if (selectedAnswer == currentQuestion.correctAnswer)
        {
            messageText.text = "✅ Correct Answer!";
            messageText.color = Color.green;
            messageAudioSource.PlayOneShot(correctSound); // Play correct answer sound
            score++;  // Increment the score for the correct answer
            scoreText.text = "Score: " + score;  // Update the score display
            Debug.Log("Correct Answer!");
        }
        else
        {
            messageText.text = "❌ Wrong Answer! Try Again.";
            messageText.color = Color.red;
            messageAudioSource.PlayOneShot(wrongSound); // Play wrong answer sound
            Debug.Log("Wrong Answer!");
        }

        // Move to the next question after a short delay
        Invoke(nameof(NextQuestion), 1.5f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Length)
        {
            GameOver(); // Call GameOver when all questions are completed
        }
        else
        {
            LoadQuestion();
        }
    }

    void GameOver()
    {
        gameOverImage.SetActive(true); // Show Game Over image
        Debug.Log("Game Over! Displaying Game Over Image.");
    }
}
