using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgLoader : MonoBehaviour
{
    // Load a scene by its name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Load the first scene
    public void LoadFirstScene()
    {
        string firstSceneName = "Task2Progress"; // Replace with the actual name of your first scene
        SceneManager.LoadScene(firstSceneName);
    }

    // Load the second scene
    public void LoadSecondScene()
    {
        
        string secondSceneName = "Task3Progress"; // Replace with the actual name of your second scene
        SceneManager.LoadScene(secondSceneName);
    }

    // Load the third scene
    public void LoadThirdScene()
    {
        string thirdSceneName = "Task5Progress"; // Replace with the actual name of your third scene
        SceneManager.LoadScene(thirdSceneName);
    }
}
