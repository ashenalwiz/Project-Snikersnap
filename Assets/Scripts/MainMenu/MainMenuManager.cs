using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startButton; // Assign in Inspector

    void Start()
    {
        // Ensure the button is assigned before adding listener
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => LoadScene("TaskHolder"));
        }
        else
        {
            Debug.LogError("Start Button is not assigned in the Inspector!");
        }
    }

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
