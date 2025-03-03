using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackButtonManager : MonoBehaviour
{
    public Button backButton; // Back button in the corner
    public GameObject confirmationPanel; // The confirmation pop-up
    public Button yesButton; // Button to confirm going back
    public Button noButton; // Button to cancel going back

    void Start()
    {
        // Ensure the confirmation panel is hidden at the start
        confirmationPanel.SetActive(false);

        // Add event listeners
        backButton.onClick.AddListener(ShowConfirmationPanel);
        yesButton.onClick.AddListener(GoToMainMenu);
        noButton.onClick.AddListener(CloseConfirmationPanel);
    }

    void ShowConfirmationPanel()
    {
        confirmationPanel.SetActive(true); // Show panel when back button is clicked
    }

    void CloseConfirmationPanel()
    {
        confirmationPanel.SetActive(false); // Hide panel when "No" is clicked
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the Main Menu scene
    }
}
