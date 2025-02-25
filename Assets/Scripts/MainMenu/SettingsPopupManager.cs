using UnityEngine;

public class SettingsPopupManager : MonoBehaviour
{
    public GameObject settingsPopup; // Reference to the popup panel

    // Function to Show the Popup
    public void ShowPopup()
    {
        settingsPopup.SetActive(true);
    }

    // Function to Hide the Popup
    public void HidePopup()
    {
        settingsPopup.SetActive(false);
    }
}
