using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TracingScripts
{
    public class ButtonHandler : MonoBehaviour
    {
        [HideInInspector] public string myItemToTace;
        public bool isNextScene, isPrevScene;

        public void ActivatePanel(string _panelName)
        {
            SoundManager.Instance.ClickFX.Play();
            MainMenuHandler.Instance.panelName = _panelName;
        }

        public void GoToTracingMainGame()
        {
            if (!isNextScene && !isPrevScene)
            {
                if (SceneManager.GetActiveScene().name != "TracingMainGame") // Updated Scene Name
                {
                    PanelSceneHandler.itemToTrace = myItemToTace;
                    PanelSceneHandler.currentItemTOTrace = myItemToTace;
                }
                else
                {
                    PanelSceneHandler.itemToTrace = myItemToTace;
                }
            }
            else
            {
                for (int i = 0; i < PanelSceneHandler.listToTrace.Count; i++)
                {
                    if (PanelSceneHandler.itemToTrace == PanelSceneHandler.listToTrace[i])
                    {
                        if (isNextScene && !isPrevScene)
                        {
                            if (i < PanelSceneHandler.listToTrace.Count - 1)
                            {
                                myItemToTace = PanelSceneHandler.listToTrace[i + 1];
                            }
                            else
                            {
                                myItemToTace = PanelSceneHandler.listToTrace[0];
                            }
                        }
                        else if (!isNextScene && isPrevScene)
                        {
                            if (i > 0)
                            {
                                myItemToTace = PanelSceneHandler.listToTrace[i - 1];
                            }
                            else
                            {
                                myItemToTace = PanelSceneHandler.listToTrace[PanelSceneHandler.listToTrace.Count - 1];
                            }
                        }
                    }
                }
                PanelSceneHandler.currentItemTOTrace = myItemToTace;
                PanelSceneHandler.itemToTrace = myItemToTace;
            }
            MainMenuHandler.Instance.panelName = "loading";
            MovingToNextScene("TracingMainGame"); // Updated Scene Name
        }

        public void GoToTracingMainMenu()
        {
            MainMenuHandler.Instance.panelName = "loading";
            PanelSceneHandler.panelToOpen = "choose";
            MovingToNextScene("TracingMainMenu"); // Updated Scene Name
        }

        void MovingToNextScene(string _sceneName)
        {
            SoundManager.Instance.ClickFX.Play();
            Debug.Log("Loading Scene: " + _sceneName);
            SceneManager.LoadScene(_sceneName);
        }
    }
}
