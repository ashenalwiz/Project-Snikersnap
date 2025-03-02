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
    public UnityEngine.UI.Image hintImage;
    public Button skipButton;
    private Sprite[] wordHintImages;
    private Dictionary<string, Sprite> wordImageMap;
    private int attempts = 0;

    //----------------New Updates--------------------------
    
    public GameObject resultPanel;  // UI panel to show stars
    public List<Image> starImages = new List<Image>(); // Array of star images to display results
    public TMP_Text finalScoreText;



    private int correctAnswers = 0;
    private int skippedWords = 0;
    //-----------------------------------------------------



    void Start()
    {
        starImages = new List<Image>
    {
        GameObject.Find("Star1").GetComponent<Image>(),
        GameObject.Find("Star2").GetComponent<Image>(),
        GameObject.Find("Star3").GetComponent<Image>()
    };

        //--------Update---------------------------
        resultPanel.SetActive(false);
        //-----------------------------------------

        playAudioButton.onClick.AddListener(PlayWordAudio);
        checkButton.onClick.AddListener(CheckWord);
        skipButton.onClick.AddListener(SkipWord);

        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        //starImages = new List<Image>();  // Initialize the list
        //starImages.Add(GameObject.Find("star1").GetComponent<Image>());
        //starImages.Add(GameObject.Find("star2").GetComponent<Image>());
        //starImages.Add(GameObject.Find("star3").GetComponent<Image>());

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
            correctAnswers++;
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
            feedbackText.text = "Let's Try again.";

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
        //string currentWord = words[currentWordIndex];

        string currentWord = wordAudioClips[currentWordIndex].name;

        if (wordImageMap.ContainsKey(currentWord))
        {
            hintImage.sprite = wordImageMap[currentWord]; // Set correct image
            hintImage.gameObject.SetActive(true); // Show image
            Debug.Log("Hint Image Displayed for: " + currentWord);
        }
        else
        {
            Debug.LogWarning("No Hint Image found for: " + currentWord);
        }
    }

    //-------------Update-SkipButton---------------------------------------------------------------------

    void SkipWord()
    {
        feedbackText.text = "Skipped!";

        skippedWords++;

        attempts = 0;
        hintImage.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        Invoke("NextWord", 1.3f);
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
            CalculateFinalScore();

            feedbackText.text = "Task Complete!"; // Show message when all words are completed
            checkButton.interactable = false; // Disable check button
            skipButton.interactable = false;

            //ShowResults();
        }
    }

    //------------------------New Features : Score & Stars-------------------------------------------------------------------------
    void ShowResults()
    {
        // --- Calculate Score ---
        int totalWords = words.Length;
        float score = ((float)correctAnswers / totalWords) * 100f;

        // --- Determine Stars ---
        int starsEarned = 0;
        if (score >= 90) starsEarned = 3;
        else if (score >= 70) starsEarned = 2;
        else if (score >= 50) starsEarned = 1;
        else starsEarned = 0;

        // --- Show Stars ---
        //for (int i = 0; i < starImages.Length; i++)
        //{
        //    starImages[i].gameObject.SetActive(i < starsEarned); // Show stars earned
        //}

        // --- Show Result Panel ---
        resultPanel.SetActive(true);
        Debug.Log("Final Score: " + score + "% | Stars Earned: " + starsEarned);
    }

    void CalculateFinalScore()
    {
        int totalWords = words.Length;
        float score = ((float)correctAnswers / totalWords) * 100; // Score formula

        feedbackText.text = "Task Complete!";
        checkButton.interactable = false;
        skipButton.interactable = false;
        wordInputField.gameObject.SetActive(false);

        ShowStars(score);
    }
    void ShowStars(float score)
    {
        resultPanel.SetActive(true); // Show result UI
        finalScoreText.text = "Final Score: " + score.ToString("F0") + "%"; // Show final percentage

        // Hide all stars initially
        foreach (Image star in starImages)
        {
            star.gameObject.SetActive(false);
        }

        // Determine star rating based on score
        if (score >= 90) // 3 stars
        {
            for (int i = 0; i < 3; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 70) // 2 stars
        {
            for (int i = 0; i < 2; i++) starImages[i].gameObject.SetActive(true);
        }
        else if (score >= 50) // 1 star
        {
            starImages[0].gameObject.SetActive(true);
        }
        // If below 50%, no stars are shown
    }


}
