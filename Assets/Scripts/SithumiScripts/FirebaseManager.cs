using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login References")]
    [SerializeField]
    private TMP_InputField loginEmail;
    [SerializeField]
    private TMP_InputField loginPassword;
    [SerializeField]
    private TMP_Text loginOutputText;
    [SerializeField]
    private Toggle rememberMeToggle;
    [Space(5f)]

    [Header("Password Reset")]
    [SerializeField]
    private GameObject passwordResetUI;
    [SerializeField]
    private TMP_InputField resetEmailField;
    [SerializeField]
    private TMP_Text resetEmailOutput;
    [Space(5f)]

    [Header("Email Verification")]
    [SerializeField]
    private float verificationCheckDelay = 5f; // Check every 5 seconds
    private Coroutine verificationCheckCoroutine;
    [Space(5f)]

    [Header("Loading")]
    [SerializeField]
    private GameObject loadingPanel;

    // for persistent login
    private const string RememberMeKey = "FirebaseRememberMe";
    private const string SavedEmailKey = "FirebaseSavedEmail";
    private const string SavedPasswordKey = "FirebaseSavedPassword";

    [Header("Register References")]
    [SerializeField]
    private TMP_InputField registerUsername;
    [SerializeField]
    private TMP_InputField RegisterEmail;
    [SerializeField]
    private TMP_InputField RegisterPassword;
    [SerializeField]
    private TMP_InputField RegisterConfirmPassword;
    [SerializeField]
    private TMP_Text registerOutputText;

    // Store registration info for potential "Remember Me" after verification
    private string lastRegisteredEmail;
    private string lastRegisteredPassword;
    private bool shouldRememberAfterRegistration = false;

    // List of game progress file names - synchronized with FirebaseProgressManager
    private readonly string[] progressFileNames = {
        "Task2UserProgress",
        "Task3UserProgress",
        "Task5UserProgress",
        "Task7UserProgress",
        "Task8UserProgress"
    };

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependancies());
    }

    // Password reset methods
    public void OpenPasswordResetUI()
    {
        AuthUIManager.instance.PasswordResetScreen();
    }

    public void PasswordResetButton()
    {
        StartCoroutine(SendPasswordResetEmail(resetEmailField.text));
    }

    private IEnumerator SendPasswordResetEmail(string email)
    {
        // Show loading indicator
        ShowLoading(true);

        // Validate email format
        if (!IsValidEmail(email))
        {
            resetEmailOutput.text = "Please enter a valid email address";
            ShowLoading(false);
            yield break;
        }

        var resetTask = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => resetTask.IsCompleted);

        ShowLoading(false);

        if (resetTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)resetTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please try again";

            switch (error)
            {
                case AuthError.InvalidEmail:
                    output = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    output = "User Not Found";
                    break;
                case AuthError.TooManyRequests:
                    output = "Too Many Requests, Try Later";
                    break;
            }

            resetEmailOutput.text = output;
        }
        else
        {
            resetEmailOutput.text = "Password reset email sent!";
            StartCoroutine(ReturnToLoginAfterDelay(3f));
        }
    }

    private IEnumerator ReturnToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AuthUIManager.instance.LoginScreen();
    }

    // Logout functionality
    public void LogoutUser()
    {
        StartCoroutine(LogoutLogic());
    }

    private IEnumerator LogoutLogic()
    {
        ShowLoading(true);

        // Sync any remaining data before logout
        if (FirebaseProgressManager.Instance != null)
        {
            FirebaseProgressManager.Instance.UploadProgressToFirebase();
        }

        // Clear remember me preference
        ClearSavedCredentials();

        auth.SignOut();

        // Wait for auth state to update
        yield return new WaitForSeconds(0.5f);

        ShowLoading(false);
        AuthUIManager.instance.LoginScreen();
    }

    // Remember Me functionality
    private void SaveCredentials(string email, string password)
    {
        PlayerPrefs.SetInt(RememberMeKey, 1);
        PlayerPrefs.SetString(SavedEmailKey, email);
        // Note: This is not secure for a production app. Consider using encryption.
        PlayerPrefs.SetString(SavedPasswordKey, password);
        PlayerPrefs.Save();
    }

    private void ClearSavedCredentials()
    {
        PlayerPrefs.SetInt(RememberMeKey, 0);
        PlayerPrefs.DeleteKey(SavedEmailKey);
        PlayerPrefs.DeleteKey(SavedPasswordKey);
        PlayerPrefs.Save();
    }

    // Public method to check the current Remember Me toggle state
    public bool IsRememberMeEnabled()
    {
        return rememberMeToggle != null && rememberMeToggle.isOn;
    }

    // Public method to resend verification email
    public void ResendVerificationEmail()
    {
        if (user != null)
        {
            StartCoroutine(sendVerificationEmail());
        }
    }

    // Method to start checking for verification
    public void StartVerificationCheck()
    {
        // Stop any existing verification check
        StopVerificationCheck();

        // Start a new verification check
        verificationCheckCoroutine = StartCoroutine(CheckEmailVerificationStatus());
    }

    // Method to stop checking for verification
    public void StopVerificationCheck()
    {
        if (verificationCheckCoroutine != null)
        {
            StopCoroutine(verificationCheckCoroutine);
            verificationCheckCoroutine = null;
        }
    }

    // Coroutine to periodically check verification status
    private IEnumerator CheckEmailVerificationStatus()
    {
        while (true)
        {
            if (user != null)
            {
                // Reload the user to get updated info
                var reloadTask = user.ReloadAsync();
                yield return new WaitUntil(() => reloadTask.IsCompleted);

                // Check if email is verified
                if (user.IsEmailVerified)
                {
                    Debug.Log("Email verified! Proceeding to game.");

                    // Handle Remember Me preference for newly registered users
                    if (shouldRememberAfterRegistration &&
                        !string.IsNullOrEmpty(lastRegisteredEmail) &&
                        !string.IsNullOrEmpty(lastRegisteredPassword))
                    {
                        SaveCredentials(lastRegisteredEmail, lastRegisteredPassword);
                    }
                    else
                    {
                        // Clear credentials if Remember Me is not checked
                        ClearSavedCredentials();
                    }

                    // Notify FirebaseProgressManager about user login
                    if (FirebaseProgressManager.Instance != null)
                    {
                        FirebaseProgressManager.Instance.OnUserLoggedIn(user.UserId);
                    }

                    FirebaseGameManager.instance.ChangeScene(1);
                    yield break;
                }
            }
            else
            {
                // No user logged in, stop checking
                yield break;
            }

            // Wait before checking again
            yield return new WaitForSeconds(verificationCheckDelay);
        }
    }

    // Validation methods
    private bool IsValidEmail(string email)
    {
        // Basic email validation
        if (string.IsNullOrEmpty(email))
            return false;

        // Use regex or simple check for @ and .
        return email.Contains("@") && email.Contains(".");
    }

    private bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return false;

        // Add more complex checks if desired
        // e.g., require numbers, special chars, etc.
        return true;
    }

    // Utility methods
    private void ShowLoading(bool isLoading)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);
    }

    private IEnumerator CheckAndFixDependancies()
    {
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(predicate: () => checkAndFixDependenciesTask.IsCompleted);

        var dependancyResult = checkAndFixDependenciesTask.Result;

        if (dependancyResult == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependancies: {dependancyResult}");
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();

        // Check if we have a user and Remember Me is enabled
        if (PlayerPrefs.GetInt(RememberMeKey, 0) == 1)
        {
            string savedEmail = PlayerPrefs.GetString(SavedEmailKey, "");
            string savedPassword = PlayerPrefs.GetString(SavedPasswordKey, "");

            if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
            {
                // Populate the login fields
                if (loginEmail != null) loginEmail.text = savedEmail;
                if (loginPassword != null) loginPassword.text = savedPassword;
                if (rememberMeToggle != null) rememberMeToggle.isOn = true;

                // Attempt auto-login
                yield return StartCoroutine(loginLogic(savedEmail, savedPassword, true));
            }
            else
            {
                AuthUIManager.instance.LoginScreen();
            }
        }
        else if (user != null)
        {
            // We have a user but Remember Me is not enabled
            // So we should log them out to prevent auto-login
            auth.SignOut();
            AuthUIManager.instance.LoginScreen();
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            // Only auto-login if Remember Me is enabled
            if (PlayerPrefs.GetInt(RememberMeKey, 0) == 1)
            {
                if (user.IsEmailVerified)
                {
                    // Notify FirebaseProgressManager about user login
                    if (FirebaseProgressManager.Instance != null)
                    {
                        FirebaseProgressManager.Instance.OnUserLoggedIn(user.UserId);
                    }
                    
                    FirebaseGameManager.instance.ChangeScene(1);
                }
                else
                {
                    StartCoroutine(sendVerificationEmail());
                }
            }
            else
            {
                // If Remember Me is not enabled, sign out the user
                auth.SignOut();
                AuthUIManager.instance.LoginScreen();
            }
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (signedIn && user != null)
            {
                Debug.Log("Signed Out");
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}");
            }
        }
    }

    public void ClearOutputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(loginLogic(loginEmail.text, loginPassword.text, rememberMeToggle.isOn));
    }

    public void RegisterButton()
    {
        // Before registration, check the Remember Me toggle state
        shouldRememberAfterRegistration = IsRememberMeEnabled();
        StartCoroutine(registerLogic(registerUsername.text, RegisterEmail.text, RegisterPassword.text, RegisterConfirmPassword.text));
    }

    // Updated login method with explicit Remember Me parameter
    private IEnumerator loginLogic(string email, string password, bool rememberMe)
    {
        // Show loading indicator
        ShowLoading(true);

        // Validate inputs
        if (!IsValidEmail(email))
        {
            loginOutputText.text = "Please enter a valid email address";
            ShowLoading(false);
            yield break;
        }

        Credential credential = EmailAuthProvider.GetCredential(email, password);

        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        ShowLoading(false);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please try again";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter your Email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please Enter your Password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid Email";
                    break;
                case AuthError.WrongPassword:
                    output = "Incorrect Password";
                    break;
                case AuthError.UserNotFound:
                    output = "Account Does Not Exist";
                    break;
                case AuthError.UserDisabled:
                    output = "Account has been disabled";
                    break;
                case AuthError.TooManyRequests:
                    output = "Too many login attempts, try again later";
                    break;
            }
            loginOutputText.text = output;
        }
        else
        {
            // Handle Remember Me preference
            if (rememberMe)
            {
                SaveCredentials(email, password);
            }
            else
            {
                ClearSavedCredentials();
            }

            if (user.IsEmailVerified)
            {
                // Load user progress data before changing scene with callback
                yield return StartCoroutine(LoadUserProgressData(user.UserId, () => {
                    // Now we can safely change scenes
                    FirebaseGameManager.instance.ChangeScene(1);
                }));
            }
            else
            {
                StartCoroutine(sendVerificationEmail());
            }
        }
    }

    private IEnumerator registerLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        ShowLoading(true);

        if (string.IsNullOrEmpty(_username) || _username.Length < 3)
        {
            registerOutputText.text = "Username must be at least 3 characters";
            ShowLoading(false);
            yield break;
        }
        else if (!IsValidEmail(_email))
        {
            registerOutputText.text = "Please enter a valid email address";
            ShowLoading(false);
            yield break;
        }
        else if (!IsPasswordStrong(_password))
        {
            registerOutputText.text = "Password must be at least 6 characters";
            ShowLoading(false);
            yield break;
        }
        else if (_password != _confirmPassword)
        {
            registerOutputText.text = "Passwords Do Not Match";
            ShowLoading(false);
            yield break;
        }
        else
        {
            // Store the registration info temporarily
            lastRegisteredEmail = _email;
            lastRegisteredPassword = _password;

            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please try again";

                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                }
                registerOutputText.text = output;
                ShowLoading(false);
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,
                    //TODO: give profile default photo
                };

                var DefaultUserTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => DefaultUserTask.IsCompleted);

                if (DefaultUserTask.Exception != null)
                {
                    user.DeleteAsync();
                    FirebaseException firebaseException = (FirebaseException)DefaultUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please try again";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session Expired";
                            break;
                    }
                    registerOutputText.text = output;
                }
                else
                {
                    Debug.Log($"Firebase User Created Successfully: {user.DisplayName} ({user.UserId})");
                    StartCoroutine(sendVerificationEmail());
                }
                ShowLoading(false);
            }
        }
    }

    private IEnumerator sendVerificationEmail()
    {
        if (user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown Error, Try Again!";

                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Verification Task was Cancelled";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too Many Requests";
                        break;
                }

                AuthUIManager.instance.AwaitVerification(false, user.Email, output);
            }
            else
            {
                AuthUIManager.instance.AwaitVerification(true, user.Email, null);
                // Start checking for verification
                StartVerificationCheck();
                Debug.Log("Email Sent Successfully");
            }
        }
    }

    private IEnumerator LoadUserProgressData(string userId, System.Action onComplete = null)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        int totalFiles = progressFileNames.Length;
        int completedFiles = 0;

        foreach (string progressFile in progressFileNames)
        {
            string localFilePath = Path.Combine(Application.persistentDataPath, progressFile + ".json");
            bool hasLocalFile = File.Exists(localFilePath);
            string localJson = null;

            // Read local file if it exists
            if (hasLocalFile)
            {
                try
                {
                    localJson = File.ReadAllText(localFilePath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error reading local file {progressFile}: {ex.Message}");
                }
            }

            // Get Firebase data
            var dataTask = reference.Child("users").Child(userId).Child("progress").Child(progressFile).GetValueAsync();
            yield return new WaitUntil(() => dataTask.IsCompleted);

            if (dataTask.Exception != null)
            {
                Debug.LogWarning($"Failed to load {progressFile}: {dataTask.Exception.Message}");
                
                // If we have local data but couldn't fetch Firebase data, upload local data
                if (hasLocalFile && !string.IsNullOrEmpty(localJson))
                {
                    var uploadTask = reference.Child("users").Child(userId).Child("progress")
                        .Child(progressFile).SetRawJsonValueAsync(localJson);
                    yield return new WaitUntil(() => uploadTask.IsCompleted);

                    if (uploadTask.Exception != null)
                    {
                        Debug.LogError($"Failed to upload local {progressFile}: {uploadTask.Exception.Message}");
                    }
                    else
                    {
                        Debug.Log($"Successfully uploaded local {progressFile} to Firebase");
                    }
                }
                
                completedFiles++;
                continue;
            }

            DataSnapshot snapshot = dataTask.Result;
            
            if (snapshot.Exists)
            {
                string firebaseJson = snapshot.GetRawJsonValue();
                
                // Compare timestamps or version numbers if included in the JSON
                // For this example, we'll use a simple string comparison
                if (hasLocalFile && localJson != firebaseJson)
                {
                    // TODO: Implement proper conflict resolution strategy
                    // For now, we'll keep the Firebase version
                    try
                    {
                        File.WriteAllText(localFilePath, firebaseJson);
                        Debug.Log($"Updated local {progressFile} with Firebase data");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error saving {progressFile} data: {ex.Message}");
                    }
                }
            }
            else if (hasLocalFile && !string.IsNullOrEmpty(localJson))
            {
                // No data in Firebase but we have local data, so upload it
                var uploadTask = reference.Child("users").Child(userId).Child("progress")
                    .Child(progressFile).SetRawJsonValueAsync(localJson);
                yield return new WaitUntil(() => uploadTask.IsCompleted);

                if (uploadTask.Exception != null)
                {
                    Debug.LogError($"Failed to upload local {progressFile}: {uploadTask.Exception.Message}");
                }
                else
                {
                    Debug.Log($"Successfully uploaded local {progressFile} to Firebase");
                }
            }
            else
            {
                Debug.Log($"No data found for {progressFile}");
            }

            completedFiles++;
        }

        Debug.Log($"All progress data processed ({completedFiles}/{totalFiles} files)");

        if (onComplete != null)
        {
            onComplete();
        }
    }
}