using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;  // Menu UI Panel
    public Slider volumeSlider;   // Volume control
    private string taskHolderScene = "TaskHolder"; // Name of the TaskHolder scene

    void Start()
    {
        menuPanel.SetActive(false); // Hide menu at start

        // Set volume slider to match the current volume level
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Toggle the menu on/off
    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    // Adjust game volume
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // Restart the current game scene
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Quit the current game and go to TaskHolder scene
    public void QuitGame()
    {
        SceneManager.LoadScene(taskHolderScene);
    }

    // Close the menu and return to the game
    public void QuitNo()
    {
        menuPanel.SetActive(false);
    }
}
