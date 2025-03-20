using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TaskSelectionManager : MonoBehaviour
{
    public Button task1Button;
    public Button task7Button; // Assign in Inspector


    public Button backButton;

    void Start()
    {

        if (task1Button != null)
        {
            task1Button.onClick.AddListener(() => LoadTaskScene("LetterSoundMatching"));
        }
        else
        {
            Debug.LogError("Task 1 Button is not assigned in the Inspector!");
        }
        // Ensure the button is assigned and add the listener
        if (task7Button != null)
        {
            task7Button.onClick.AddListener(() => LoadTaskScene("TypingTaskV2"));
        }
        else
        {
            Debug.LogError("Task 7 Button is not assigned in the Inspector!");
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => LoadTaskScene("MainMenu"));
        }
        else
        {
            Debug.LogError("Back Button is not assigned in the Inspector!");
        }
    }

    void LoadTaskScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
