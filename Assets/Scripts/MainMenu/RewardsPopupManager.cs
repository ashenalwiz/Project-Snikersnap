using UnityEngine;

public class RewardsPopupManager : MonoBehaviour
{
    public GameObject rewardsPopup; // Reference to the popup panel

    // Function to Show the Popup
    public void ShowPopup()
    {
        rewardsPopup.SetActive(true);
    }

    // Function to Hide the Popup
    public void HidePopup()
    {
        rewardsPopup.SetActive(false);
    }
}
