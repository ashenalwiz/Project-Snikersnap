using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TaskSelectionManager : MonoBehaviour
{
    public Button task1Button;
    public Button task2Button;
    public Button task3Button;
    public Button task4Button;
    public Button task5Button;
    public Button task6Button;
    public Button task7Button;
    public Button task8Button;
    public Button task9Button;
    
    // Assign in Inspector


    public Button backButton;

    void Start()
    {

        if (task1Button != null)
        {
            task1Button.onClick.AddListener(() => LoadTaskScene("Task1"));
        }
        else
        {
            Debug.LogError("Task 1 Button is not assigned in the Inspector!");
        }
        if (task2Button != null)
        {
            task2Button.onClick.AddListener(() => LoadTaskScene("Task2"));
        }
        else
        {
            Debug.LogError("Task 2 Button is not assigned in the Inspector!");
        }
        if (task3Button != null)
        {
            task3Button.onClick.AddListener(() => LoadTaskScene("Task3"));
        }
        else
        {
            Debug.LogError("Task 3 Button is not assigned in the Inspector!");
        }
        if (task4Button != null)
        {
            task4Button.onClick.AddListener(() => LoadTaskScene("Task4"));
        }
        else
        {
            Debug.LogError("Task 4 Button is not assigned in the Inspector!");
        }
        if (task5Button != null)
        {
            task5Button.onClick.AddListener(() => LoadTaskScene("Task5"));
        }
        else
        {
            Debug.LogError("Task 5 Button is not assigned in the Inspector!");
        }
        if (task6Button != null)
        {
            task6Button.onClick.AddListener(() => LoadTaskScene("Task6"));
        }
        else
        {
            Debug.LogError("Task 6 Button is not assigned in the Inspector!");
        }
        // Ensure the button is assigned and add the listener
        if (task7Button != null)
        {
            task7Button.onClick.AddListener(() => LoadTaskScene("Task7"));
        }
        else
        {
            Debug.LogError("Task 7 Button is not assigned in the Inspector!");
        }
        if (task8Button != null)
        {
            task8Button.onClick.AddListener(() => LoadTaskScene("Task8"));
        }
        else
        {
            Debug.LogError("Task 8 Button is not assigned in the Inspector!");
        }
        if (task9Button != null)
        {
            task9Button.onClick.AddListener(() => LoadTaskScene("Task9"));
        }
        else
        {
            Debug.LogError("Task 1 Button is not assigned in the Inspector!");
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
