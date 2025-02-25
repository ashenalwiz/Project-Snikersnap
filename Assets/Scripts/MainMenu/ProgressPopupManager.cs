using UnityEngine;

public class ProgressPopupManager : MonoBehaviour
{
    public GameObject progressPopup; // Reference to the popup panel

    // Function to Show the Popup
    public void ShowPopup()
    {
        progressPopup.SetActive(true);
    }

    // Function to Hide the Popup
    public void HidePopup()
    {
        progressPopup.SetActive(false);
    }
}
