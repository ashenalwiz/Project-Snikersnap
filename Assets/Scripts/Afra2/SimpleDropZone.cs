using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string wordWithBlank; // Word with missing letter (e.g., "C_T")
    public string correctLetter; // Correct missing letter (e.g., "A")
    public Sprite wordImage; // Image for incorrect attempt
    public string[] letterOptions = new string[3]; // Three letter choices
}

public class SimpleDropZone : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI wordText; // Displays word with missing letter
    public TextMeshProUGUI feedbackText; // Feedback message
    public RawImage wordImageUI; // UI Image for word
    public Button nextButton; // Button to move to next question

    public QuestionData[] questions; // Array of questions
    public Button[] letterButtons; // Letter buttons (Ensure full Button GameObjects are assigned)

    private int currentQuestionIndex = 0; // Track current question
    private int currentAttempts = 0; // Attempts for current question

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>(); // Store original positions
    private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>(); // Store original parents

    private void Start()
    {
        if (wordImageUI != null) wordImageUI.gameObject.SetActive(false); // Hide image initially
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false); // Hide Next button initially
            nextButton.onClick.AddListener(NextQuestion);
        }

        // Store the original positions and parents of all letter buttons
        foreach (var letterButton in letterButtons)
        {
            GameObject parentObject = letterButton.gameObject; // Get the button GameObject itself
            originalPositions[parentObject] = parentObject.transform.position;
            originalParents[parentObject] = parentObject.transform.parent;
        }

        LoadQuestion();
    }

    private void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            Debug.Log("Game Over! All questions completed.");
            feedbackText.text = "Game Over!";
            nextButton.gameObject.SetActive(false);
            return;
        }

        QuestionData question = questions[currentQuestionIndex];

        wordText.text = question.wordWithBlank; // Set word text
        
        // Update the image
        if (question.wordImage != null && wordImageUI != null)
        {
            wordImageUI.texture = question.wordImage.texture; // Update image
            wordImageUI.gameObject.SetActive(false); // Hide image at start
        }

        // Reset all letter buttons to their original positions
        foreach (var letterButton in letterButtons)
        {
            GameObject parentObject = letterButton.gameObject;
            
            if (originalPositions.ContainsKey(parentObject) && originalParents.ContainsKey(parentObject))
            {
                // Reset parent
                parentObject.transform.SetParent(originalParents[parentObject]);
                
                // Reset position
                parentObject.transform.position = originalPositions[parentObject];
            }
        }

        // Assign letters to buttons
        for (int i = 0; i < letterButtons.Length; i++)
        {
            if (i < question.letterOptions.Length)
            {
                TextMeshProUGUI tmpText = letterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = question.letterOptions[i]; // Set letter text
                }

                // Assign correct letter to the draggable object script
                SimpleDragAndDrop dragScript = letterButtons[i].GetComponent<SimpleDragAndDrop>();
                if (dragScript != null)
                {
                    dragScript.correctLetter = question.correctLetter;
                }
            }
        }

        currentAttempts = 0; // Reset attempts
        feedbackText.text = ""; // Clear feedback
        nextButton.gameObject.SetActive(false); // Hide Next button
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject != null)
        {
            SimpleDragAndDrop dragAndDropScript = draggedObject.GetComponent<SimpleDragAndDrop>();

            if (dragAndDropScript != null)
            {
                TextMeshProUGUI draggedLetterText = draggedObject.GetComponentInChildren<TextMeshProUGUI>();

                if (draggedLetterText != null && draggedLetterText.text == dragAndDropScript.correctLetter)
                {
                    ShowFeedback("Correct!");
                    draggedObject.transform.SetParent(transform);
                    draggedObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    nextButton.gameObject.SetActive(true); // Show Next button after correct answer
                }
                else
                {
                    currentAttempts++;

                    if (currentAttempts == 1)
                    {
                        ShowFeedback("Try Again!");
                        dragAndDropScript.ResetPosition();
                        StartCoroutine(ShowImageAfterDelay());
                    }
                    else
                    {
                        ShowFeedback("Incorrect! No more attempts.");
                        nextButton.gameObject.SetActive(true); // Show Next button after two wrong attempts
                    }
                }
            }
        }
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private IEnumerator ShowImageAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (wordImageUI != null)
        {
            wordImageUI.gameObject.SetActive(true);
        }
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            LoadQuestion();
        }
        else
        {
            feedbackText.text = "Game Over!";
            nextButton.gameObject.SetActive(false); // Hide Next button when all questions are done
        }
    }
}
