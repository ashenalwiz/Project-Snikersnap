using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TypingGame : MonoBehaviour
{
    public Button playAudioButton;
    public TMP_InputField wordInputField;
    public Button checkButton;
    public TMP_Text feedbackText;
    public AudioSource audioSource;
    public AudioClip[] wordAudioClips; // Array to hold 5 audio clips
    private string[] words = { "Rock", "Cave", "Mark", "Dark", "Shark" }; // List of words
    private int currentWordIndex = 0; // Track current word

    void Start()
    {
        playAudioButton.onClick.AddListener(PlayWordAudio);
        checkButton.onClick.AddListener(CheckWord);
        feedbackText.text = "";
    }

    void PlayWordAudio()
    {
        if (currentWordIndex < wordAudioClips.Length)
        {
            audioSource.clip = wordAudioClips[currentWordIndex];
            audioSource.Play();
        }
    }

    void CheckWord()
    {
        string userInput = wordInputField.text.Trim().ToLower(); // Ensure it's trimmed and lowercase
        string correctWord = wordAudioClips[currentWordIndex].name.ToLower(); // Ensure correct word matches input format

        Debug.Log($"User Input: '{userInput}' | Correct Word: '{correctWord}'");

        if (userInput == correctWord)
        {
            feedbackText.text = "Correct!";
        }
        else
        {
            feedbackText.text = "Try again.";
        }
    }
}
