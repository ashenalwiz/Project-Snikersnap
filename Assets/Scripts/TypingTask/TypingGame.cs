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
    //========================================================================

    void PlayWordAudio()
    {
        if (currentWordIndex < wordAudioClips.Length)
        {
            audioSource.clip = wordAudioClips[currentWordIndex];
            audioSource.Play();
        }
    }

    public void CheckWord()
    {
        string userInput = wordInputField.text.Trim().ToLower(); // Sanitize input
        string correctWord = wordAudioClips[currentWordIndex].name.ToLower(); // Get current correct word

        Debug.Log($"User Input: '{userInput}' | Correct Word: '{correctWord}'");

        if (userInput == correctWord)
        {
            feedbackText.text = "Correct!";
            Invoke("NextWord", 1.5f); // Wait 1.5 seconds before moving to next word
        }
        else
        {
            feedbackText.text = "Try again.";
        }
    }

    void NextWord()
    {
        currentWordIndex++; // Move to next word

        if (currentWordIndex < wordAudioClips.Length) // Check if words are remaining
        {
            wordInputField.text = ""; // Clear input field
            feedbackText.text = "";   // Clear feedback text
            PlayWordAudio(); // Play next word audio automatically
        }
        else
        {
            feedbackText.text = "Task Complete!"; // Show message when all words are completed
            checkButton.interactable = false; // Disable check button
        }
    }

}
