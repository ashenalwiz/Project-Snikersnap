using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject inGameMenuPanel; // Panel that pops up
    public Button settingsButton, yesButton, noButton, restartButton;
    public Slider musicVolumeSlider, taskVolumeSlider;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource taskSource;

    private void Start()
    {
        // Load saved volume settings
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        taskVolumeSlider.value = PlayerPrefs.GetFloat("TaskVolume", 1f);

        // Apply volumes at start
        ApplyVolume();

        // Assign button functions
        settingsButton.onClick.AddListener(ToggleMenu);
        yesButton.onClick.AddListener(QuitToMainMenu);
        noButton.onClick.AddListener(CloseMenu);
        restartButton.onClick.AddListener(RestartCurrentScene);

        // Assign slider functions
        musicVolumeSlider.onValueChanged.AddListener(delegate { UpdateMusicVolume(); });
        taskVolumeSlider.onValueChanged.AddListener(delegate { UpdateTaskVolume(); });

        // Hide menu at start
        inGameMenuPanel.SetActive(false);
    }

    public void ToggleMenu()
    {


        // If menu is hidden, show it; if it's shown, hide it
        bool isActive = inGameMenuPanel.activeSelf;
        inGameMenuPanel.SetActive(true);
        Debug.Log("Menu is active: " + !isActive);

        // Pause or resume game based on menu state
        Time.timeScale = inGameMenuPanel.activeSelf ? 0f : 1f;
    }

    public void CloseMenu()
    {
        Debug.Log("No button pressed, closing menu...");
        inGameMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void RestartCurrentScene()
    {
        Debug.Log("Restart button pressed, reloading scene...");

        Time.timeScale = 1f; // Reset game speed
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    public void QuitToMainMenu()
    {
        Debug.Log("Yes button pressed, going to TaskHolder...");
        Time.timeScale = 1f; // Reset game speed
        SceneManager.LoadScene("TaskHolder"); // Change "TaskHolder" to your actual Main Menu scene name
    }

    //create a quitmethod to go to the mainmenu
    public void QuitToMenu()
    {
        Debug.Log("Yes button pressed, going to TaskHolder...");
        Time.timeScale = 1f; // Reset game speed
        SceneManager.LoadScene("MainMenu"); // Change "TaskHolder" to your actual Main Menu scene name
    }

    public void UpdateMusicVolume()
    {
        musicSource.volume = musicVolumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
    }

    public void UpdateTaskVolume()
    {
        taskSource.volume = taskVolumeSlider.value;
        PlayerPrefs.SetFloat("TaskVolume", taskVolumeSlider.value);
    }

    void ApplyVolume()
    {
        musicSource.volume = musicVolumeSlider.value;
        taskSource.volume = taskVolumeSlider.value;
    }
}
