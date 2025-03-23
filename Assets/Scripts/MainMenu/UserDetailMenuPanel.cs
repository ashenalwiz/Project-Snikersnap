using UnityEngine;
using UnityEngine.UI;

public class UserDetailManuPanel : MonoBehaviour
{
    [Header("User Detail Panel")]
    public GameObject userDetailPanel; // Reference to the panel

    [Header("Input Fields")]
    public InputField nameInputField;      // Editable Name Field
    public InputField usernameInputField;  // Editable Username Field
    public Text emailText;                 // Non-editable Email Text

    private void Start()
    {
        // Ensure the panel is hidden initially
        userDetailPanel.SetActive(false);

        // Load saved user details (if any)
        LoadUserDetails();
    }

    // Open the User Panel
    public void OpenUserDetailPanel()
    {
        userDetailPanel.SetActive(true);
    }

    // Close the User Panel
    public void CloseUserDetailPanel()
    {
        userDetailPanel.SetActive(false);
        SaveUserDetails(); // Save when closing
    }

    // Save the user inputs
    public void SaveUserDetails()
    {
        PlayerPrefs.SetString("UserName", nameInputField.text);
        PlayerPrefs.SetString("UserUsername", usernameInputField.text);
        PlayerPrefs.Save();
    }

    // Load the user details when opening
    public void LoadUserDetails()
    {
        nameInputField.text = PlayerPrefs.GetString("UserName", "Enter Name");
        usernameInputField.text = PlayerPrefs.GetString("UserUsername", "Enter Username");

        // Assuming email is set during registration
        emailText.text = PlayerPrefs.GetString("UserEmail", "user@example.com");
    }
}
