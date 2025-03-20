using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variables
    private FirebaseAuth auth;

    // Add references to your UI elements here
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TMP_InputField confirmPasswordField;

    void Start()
    {
        Debug.Log("Starting Firebase initialization...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase initialized successfully. Auth instance: " + (auth != null ? "Valid" : "NULL"));
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + dependencyStatus);
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    if (statusText != null)
                        statusText.text = "Firebase initialization failed!";
                });
            }
        });
    }




    public void LoginUser()
    {
        statusText.text = "Logging in...";
        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle errors
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    statusText.text = "Login failed";
                });
                return;
            }

            // Login successful
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                statusText.text = "Login successful!";
                // Load your main game scene here
            });
        });
    }

    public void RegisterUser()
    {
        if (passwordField.text != confirmPasswordField.text)
        {
            statusText.text = "Passwords don't match";
            return;
        }

        statusText.text = "Registering...";

        // Check if auth is initialized
        if (auth == null)
        {
            Debug.LogError("Firebase Auth is null");
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                statusText.text = "Firebase is not initialized! Try again later.";
            });
            return;
        }

        // Add debug logging
        Debug.Log("Attempting to register user: " + emailField.text);

        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Registration was canceled");
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    statusText.text = "Registration canceled";
                });
                return;
            }

            if (task.IsFaulted)
            {
                // Improved error logging
                string errorMessage = "Unknown error";
                if (task.Exception != null)
                {
                    Debug.LogError("Registration error full exception: " + task.Exception.ToString());

                    var exceptions = task.Exception.Flatten().InnerExceptions;
                    if (exceptions.Count > 0)
                    {
                        errorMessage = exceptions[0].Message;
                    }
                }

                Debug.LogError("Registration failed: " + errorMessage);

                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    statusText.text = "Registration failed: " + errorMessage;
                });
                return;
            }

            Debug.Log("Registration completed successfully");

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                statusText.text = "Registration successful!";
            });
        });
    }


    public void GoToRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }

    public void GoToLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
