using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackButtonManager : MonoBehaviour
{
    public Button backButton; // Back button in the corner
    public GameObject menuPanel; // Reference to the MenuPanel in your scene

    // Buttons inside the MenuPanel
    public Button quitYesButton;
    public Button quitNoButton;

    void Start()
    {
        // Ensure the menu panel is hidden at start
        menuPanel.SetActive(false);

        // Add event listener to the back button
        backButton.onClick.AddListener(ShowMenuPanel);

        // Add event listeners to the buttons
        quitYesButton.onClick.AddListener(GoToMainMenu);
        quitNoButton.onClick.AddListener(CloseMenuPanel);
    }

    void ShowMenuPanel()
    {
        menuPanel.SetActive(true); // Show MenuPanel when back button is clicked
    }

    void CloseMenuPanel()
    {
        menuPanel.SetActive(false); // Hide MenuPanel when "No" is clicked
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the Main Menu scene
    }
}