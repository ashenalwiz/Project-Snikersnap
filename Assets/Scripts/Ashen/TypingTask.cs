using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TypingTask : MonoBehaviour
{
    public Button audioButton, checkButton, skipButton;
    public InputField wordInput;
    public Text feedbackText;
    public AudioSource audioSource;

    public AudioClip[] wordAudioClips; // Assign 3 audio clips in Inspector
    public string[] words = { "Rock", "Cave", "Dark" }; // Words to type

    private int currentWordIndex = 0;
    private int wrongAttempts = 0;
    private int correctAnswers = 0;

    void Start()
    {
        skipButton.gameObject.SetActive(false);
        feedbackText.text = "";

        audioButton.onClick.AddListener(PlayWord);
        checkButton.onClick.AddListener(CheckAnswer);
        skipButton.onClick.AddListener(SkipWord);
    }

    void PlayWord()
    {
        if (audioSource && currentWordIndex < wordAudioClips.Length)
        {
            audioSource.clip = wordAudioClips[currentWordIndex];
            audioSource.Play();
        }
    }

    void CheckAnswer()
    {
        string userAnswer = wordInput.text.Trim().ToLower();
        if (userAnswer == words[currentWordIndex])
        {
            feedbackText.text = "That is Correct!";
            feedbackText.color = Color.green;
            correctAnswers++;
            NextWord();
        }
        else
        {
            feedbackText.text = "it's okay you can Try again!";
            feedbackText.color = Color.red;
            wrongAttempts++;

            if (wrongAttempts >= 5)
            {
                skipButton.gameObject.SetActive(true);
            }
        }
    }

    void SkipWord()
    {
        feedbackText.text = "The correct word was: " + words[currentWordIndex];
        feedbackText.color = Color.yellow;
        NextWord();
    }

    void NextWord()
    {
        wordInput.text = "";
        wrongAttempts = 0;
        skipButton.gameObject.SetActive(false);

        if (++currentWordIndex < words.Length)
        {
            feedbackText.text = "Next word!";
        }
        else
        {
            EndTask();
        }
    }

    void EndTask()
    {
        feedbackText.text = "Task Complete!";

        // Show stars & trophy based on performance
        if (correctAnswers == 3)
        {
            feedbackText.text = "⭐ ⭐ ⭐ Trophy Earned!";
        }
        else if (correctAnswers == 2)
        {
            feedbackText.text = "⭐ ⭐ Congratulations!";
        }
        else if (correctAnswers == 1)
        {
            feedbackText.text = "⭐ Keep trying!";
        }
    }
}
