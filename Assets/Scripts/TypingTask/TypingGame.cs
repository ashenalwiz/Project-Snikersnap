using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;



public class TypingGame : MonoBehaviour
{
    public Button playAudioButton;
    public TMP_InputField wordInputField;
    public Button checkButton;
    public TMP_Text feedbackText;
    public AudioSource audioSource;
    public AudioClip[] wordAudioClips; // Array to hold 5 audio clips
    private string[] words = { "Rock", "Cave", "Flag", "Dark", "Shark","Cat","Cup","Nap","Hello","Yellow","Lava","Jump","Run" }; // List of words
    private int currentWordIndex = 0; // Track current word


    //----------------New Updates--------------------------
    public UnityEngine.UI.Image hintImage;
    public Button skipButton;
    //public Image hintImage;
    private Sprite[] wordHintImages;
    private Dictionary<string, Sprite> wordImageMap;
    private int attempts = 0;
    //private int incorrectAttempts = 0;
    //-----------------------------------------------------



    void Start()
    {
        

        //hintImage = GameObject.Find("HintImage").GetComponent<Image>();
        playAudioButton.onClick.AddListener(PlayWordAudio);
        checkButton.onClick.AddListener(CheckWord);
        skipButton.onClick.AddListener(SkipWord);

        //-------------New Updates-------------------------

        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        wordImageMap = new Dictionary<string, Sprite>
        {
            { "Rock", Resources.Load<Sprite>("HintImages/Rock") },
            { "Cave", Resources.Load<Sprite>("HintImages/Cave") },
            { "Flag", Resources.Load<Sprite>("HintImages/Flag") },
            { "Dark", Resources.Load<Sprite>("HintImages/Dark") },
            { "Shark", Resources.Load<Sprite>("HintImages/Shark") },
            { "Cat", Resources.Load<Sprite>("HintImages/Cat") },
            { "Cup", Resources.Load<Sprite>("HintImages/Cup") },
            { "Nap", Resources.Load<Sprite>("HintImages/Nap") },
            { "Hello", Resources.Load<Sprite>("HintImages/Hello") },
            { "Yellow", Resources.Load<Sprite>("HintImages/Yellow") },
            { "Lava", Resources.Load<Sprite>("HintImages/Lava") },
            { "Jump", Resources.Load<Sprite>("HintImages/Jump") },
            { "Run", Resources.Load<Sprite>("HintImages/Run") }
        };

        //-------------------------------------------------


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

            //----------------Updated----------------

            attempts = 0;

            //incorrectAttempts = 0; // Reset wrong attempts
            hintImage.gameObject.SetActive(false); // Hide image if it was displayed
            skipButton.gameObject.SetActive(false);
            //-----------------------------------------

            Invoke("NextWord", 1.5f); // Wait 1.5 seconds before moving to next word

        }
        else
        {
            attempts++;
            //incorrectAttempts++;
            feedbackText.text = "Try again.";

            //-------Update----------------------------------------------------

            if (attempts == 1)
            {
                skipButton.gameObject.SetActive(true); // Show skip button after first mistake
            }

            if (attempts == 2)
            {
                ShowHintImage();
            }

            //------------------------------------------------------------------

            //if (incorrectAttempts >= 3) { 
             //   hintImage.gameObject.SetActive(true);
            //}
        }
    }
    //------------Update----------------------------------------------------------------

    void ShowHintImage()
    {
        string currentWord = words[currentWordIndex];

        if (wordImageMap.ContainsKey(currentWord))
        {
            hintImage.sprite = wordImageMap[currentWord]; // Set correct image
            hintImage.gameObject.SetActive(true); // Show image
        }
    }

    //-------------Update-SkipButton---------------------------------------------------------------------

    void SkipWord()
    {
        feedbackText.text = "Skipped!";
        attempts = 0;
        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        Invoke("NextWord", 1.0f);
    }


    //----------------------------------------------------------------------------------
    void NextWord()
    {
        currentWordIndex++; // Move to next word

        if (currentWordIndex < wordAudioClips.Length) // Check if words are remaining
        {
            wordInputField.text = ""; // Clear input field
            feedbackText.text = "";   // Clear feedback text
            attempts = 0;
            //incorrectAttempts = 0;
            hintImage.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            PlayWordAudio(); // Play next word audio automatically
        }
        else
        {
            feedbackText.text = "Task Complete!"; // Show message when all words are completed
            checkButton.interactable = false; // Disable check button
            skipButton.interactable = false;
        }
    }

}
