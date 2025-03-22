using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseGameManager : MonoBehaviour
{
    public static FirebaseGameManager instance;

    // Ensures that this GameObject persists across scenes and maintains a single instance (Singleton pattern).
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Loads a new scene asynchronously based on the given scene index.
    public void ChangeScene(int _sceneIndex)
    {
        SceneManager.LoadSceneAsync(_sceneIndex);
    }
}
