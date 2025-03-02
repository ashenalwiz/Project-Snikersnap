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

    public AudioSource audioSource;
    public Button replayButton;
    public Button option1Button;
    public Button option2Button;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public GameObject correctMessage;
    public GameObject wrongMessage;

    void Start()
    {
        LoadQuestion();
        replayButton.onClick.AddListener(PlayAudio);
        option1Button.onClick.AddListener(() => CheckAnswer(option1Text.text));
        option2Button.onClick.AddListener(() => CheckAnswer(option2Text.text));
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            Debug.Log("Game Over! No more questions.");
            return;
        }

        Question currentQuestion = questions[currentQuestionIndex];

        option1Text.text = currentQuestion.option1;
        option2Text.text = currentQuestion.option2;
        audioSource.clip = currentQuestion.audioClip;

        correctMessage.SetActive(false);
        wrongMessage.SetActive(false);
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
        correctMessage.SetActive(true);
        wrongMessage.SetActive(false);
        Debug.Log("Correct Answer! ✅"); // Debugging
    }
    else
    {
        correctMessage.SetActive(false);
        wrongMessage.SetActive(true);
        Debug.Log("Wrong Answer! ❌"); // Debugging
    }

    // Move to the next question after a short delay
    Invoke(nameof(NextQuestion), 1.5f);
}

void NextQuestion()
{
    currentQuestionIndex++;
    LoadQuestion();
}

}  