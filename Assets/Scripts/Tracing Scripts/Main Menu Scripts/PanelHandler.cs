using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TracingScripts
{
    public class PanelHandler : MonoBehaviour
    {
        public string myPanelName;
        public bool PauseTheGame;

        private void LateUpdate()
        {
            HandlePanelVisibility();
            HandleGamePause();
        }

        private void HandlePanelVisibility()
        {
            if (myPanelName == "main")  // Assuming "main" is the main panel name
            {
                this.transform.GetChild(0).gameObject.SetActive(false); // Ensure main panel stays off
            }

            if (myPanelName == MainMenuHandler.Instance.panelName)
            {
                this.transform.GetChild(0).gameObject.SetActive(true);
            }
            else if (MainMenuHandler.Instance.panelName != "loading")
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        private void HandleGamePause()
        {
            if (PauseTheGame)
            {
                if (this.transform.GetChild(0).gameObject.activeSelf)
                {
                    Time.timeScale = 0f;
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }
        }
    }
}