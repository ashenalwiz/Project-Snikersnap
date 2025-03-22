using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadChatBotScene()
    {
        SceneManager.LoadScene("ChatBot");
    }
}
