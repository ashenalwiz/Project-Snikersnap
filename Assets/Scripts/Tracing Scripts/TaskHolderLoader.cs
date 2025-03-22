using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskHolderLoader : MonoBehaviour
{
    public void LoadTaskHolderScene()
    {
        SceneManager.LoadScene("TaskHolder");
    }
}
