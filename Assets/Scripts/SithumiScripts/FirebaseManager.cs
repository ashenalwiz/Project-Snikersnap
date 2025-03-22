using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System.Net;
using UnityEngine.SocialPlatforms.Impl;

// Manages all Firebase authentication operations including login, registration,
// email verification, password reset, and persistent login.
public class FirebaseManager : MonoBehaviour
{
    // Singleton instance for global access
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;       // Firebase Authentication instance
    public FirebaseUser user;       // Current Firebase user
    [Space(5f)]

    [Header("Login References")]
    [SerializeField]
    private TMP_InputField loginEmail;      // Input field for login email
    [SerializeField]
    private TMP_InputField loginPassword;   // Input field for login password
    [SerializeField]
    private TMP_Text loginOutputText;       // Text to display login results/errors
    [SerializeField]
    private Toggle rememberMeToggle;        // Toggle for "Remember Me" functionality
    [Space(5f)]

    [Header("Password Reset")]
    [SerializeField]
    private GameObject passwordResetUI;     // UI panel for password reset
    [SerializeField]
    private TMP_InputField resetEmailField; // Input field for reset email
    [SerializeField]
    private TMP_Text resetEmailOutput;      // Text to display reset results/errors
    [Space(5f)]

    [Header("Email Verification")]
    [SerializeField]
    private float verificationCheckDelay = 5f; // Interval to check verification status
    private Coroutine verificationCheckCoroutine; // Reference to verification check coroutine
    [Space(5f)]

    [Header("Loading")]
    [SerializeField]
    private GameObject loadingPanel;        // Loading indicator panel

    // PlayerPrefs keys for persistent login
    private const string RememberMeKey = "FirebaseRememberMe";
    private const string SavedEmailKey = "FirebaseSavedEmail";
    private const string SavedPasswordKey = "FirebaseSavedPassword";

    [Header("Register References")]
    [SerializeField]
    private TMP_InputField registerUsername;        // Input field for registration username
    [SerializeField]
    private TMP_InputField RegisterEmail;           // Input field for registration email
    [SerializeField]
    private TMP_InputField RegisterPassword;        // Input field for registration password
    [SerializeField]
    private TMP_InputField RegisterConfirmPassword; // Input field to confirm password
    [SerializeField]
    private TMP_Text registerOutputText;            // Text to display registration results/errors

    // Cache registration info for potential "Remember Me" after verification
    private string lastRegisteredEmail;
    private string lastRegisteredPassword;
    private bool shouldRememberAfterRegistration = false;

    // Awake is called when the script instance is being loaded.
    // Sets up the singleton pattern.
    public void Awake()
    {
        // Make this object persistent across scenes
        DontDestroyOnLoad(gameObject);

        // Implement singleton pattern
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

    // Start is called before the first frame update.
    // Initializes Firebase and checks dependencies.
    private void Start()
    {
        StartCoroutine(CheckAndFixDependancies());
    }

    // Opens the password reset UI screen.
    public void OpenPasswordResetUI()
    {
        AuthUIManager.instance.PasswordResetScreen();
    }

    // Initiates the password reset process when button is clicked.
    public void PasswordResetButton()
    {
        StartCoroutine(SendPasswordResetEmail(resetEmailField.text));
    }

    // Sends a password reset email to the specified address.
    // email: Email address to send the reset link to
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

        // Send password reset email
        var resetTask = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => resetTask.IsCompleted);

        ShowLoading(false);

        // Handle any errors that occurred
        if (resetTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)resetTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please try again";

            // Convert Firebase error codes to user-friendly messages
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
            // Success message and return to login screen after delay
            resetEmailOutput.text = "Password reset email sent!";
            StartCoroutine(ReturnToLoginAfterDelay(3f));
        }
    }

    // Returns to login screen after specified delay.
    // delay: Time in seconds to wait before switching screens
    private IEnumerator ReturnToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AuthUIManager.instance.LoginScreen();
    }

    // Logs out the current user.
    public void LogoutUser()
    {
        StartCoroutine(LogoutLogic());
    }

    // Handles the logout process.
    private IEnumerator LogoutLogic()
    {
        ShowLoading(true);

        // Clear saved credentials when logging out
        ClearSavedCredentials();

        // Sign out from Firebase
        auth.SignOut();

        // Wait for auth state to update
        yield return new WaitForSeconds(0.5f);

        ShowLoading(false);
        AuthUIManager.instance.LoginScreen();
    }

    // Saves user credentials for "Remember Me" functionality.
    // email: User's email
    // password: User's password
    private void SaveCredentials(string email, string password)
    {
        PlayerPrefs.SetInt(RememberMeKey, 1);
        PlayerPrefs.SetString(SavedEmailKey, email);
        // Note: This is not secure for a production app. Consider using encryption.
        PlayerPrefs.SetString(SavedPasswordKey, password);
        PlayerPrefs.Save();
    }

    // Clears saved credentials from PlayerPrefs.
    private void ClearSavedCredentials()
    {
        PlayerPrefs.SetInt(RememberMeKey, 0);
        PlayerPrefs.DeleteKey(SavedEmailKey);
        PlayerPrefs.DeleteKey(SavedPasswordKey);
        PlayerPrefs.Save();
    }

    // Checks if "Remember Me" is currently enabled.
    // Returns: True if Remember Me is enabled, otherwise false
    public bool IsRememberMeEnabled()
    {
        return rememberMeToggle != null && rememberMeToggle.isOn;
    }

    // Resends the verification email to the current user.
    public void ResendVerificationEmail()
    {
        if (user != null)
        {
            StartCoroutine(sendVerificationEmail());
        }
    }

    // Starts the verification check process.
    public void StartVerificationCheck()
    {
        // Stop any existing verification check
        StopVerificationCheck();

        // Start a new verification check
        verificationCheckCoroutine = StartCoroutine(CheckEmailVerificationStatus());
    }

    // Stops the verification check process.
    public void StopVerificationCheck()
    {
        if (verificationCheckCoroutine != null)
        {
            StopCoroutine(verificationCheckCoroutine);
            verificationCheckCoroutine = null;
        }
    }

    // Periodically checks if the user's email has been verified.
    private IEnumerator CheckEmailVerificationStatus()
    {
        while (true)
        {
            if (user != null)
            {
                // Reload the user to get updated verification info
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

                    // Load the game scene
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

    // Validates an email address format.
    // email: Email to validate
    // Returns: True if email is valid, otherwise false
    private bool IsValidEmail(string email)
    {
        // Basic email validation
        if (string.IsNullOrEmpty(email))
            return false;

        // Use regex or simple check for @ and .
        return email.Contains("@") && email.Contains(".");
    }

    // Checks if a password meets strength requirements.
    // password: Password to check
    // Returns: True if password is strong enough, otherwise false
    private bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return false;

        // Add more complex checks if desired
        // e.g., require numbers, special chars, etc.
        return true;
    }

    // Shows or hides the loading panel.
    // isLoading: True to show loading panel, false to hide
    private void ShowLoading(bool isLoading)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);
    }

    // Checks and fixes Firebase dependencies.
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

    // Initializes Firebase Authentication and sets up event handlers.
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());

        // Set up auth state changed event
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    // Checks if auto-login is possible and attempts it if enabled.
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

    // Handles auto-login based on user verification status.
    private void AutoLogin()
    {
        if (user != null)
        {
            // Only auto-login if Remember Me is enabled
            if (PlayerPrefs.GetInt(RememberMeKey, 0) == 1)
            {
                if (user.IsEmailVerified)
                {
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

    // Handles Firebase authentication state changes.
    // sender: Event sender
    // eventArgs: Event arguments
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

    // Clears all output text fields.
    public void ClearOutputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    // Initiates login process when login button is clicked.
    public void LoginButton()
    {
        StartCoroutine(loginLogic(loginEmail.text, loginPassword.text, rememberMeToggle.isOn));
    }

    // Initiates registration process when register button is clicked.
    public void RegisterButton()
    {
        // Before registration, check the Remember Me toggle state
        shouldRememberAfterRegistration = IsRememberMeEnabled();
        StartCoroutine(registerLogic(registerUsername.text, RegisterEmail.text, RegisterPassword.text, RegisterConfirmPassword.text));
    }

    // Handles the login process.
    // email: User email
    // password: User password
    // rememberMe: Whether to remember login credentials
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

        // Create credential from email and password
        Credential credential = EmailAuthProvider.GetCredential(email, password);

        // Attempt to sign in
        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        ShowLoading(false);

        // Handle login errors
        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please try again";

            // Convert Firebase error codes to user-friendly messages
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

            // Check if email is verified
            if (user.IsEmailVerified)
            {
                yield return new WaitForSeconds(1f);
                FirebaseGameManager.instance.ChangeScene(1);
            }
            else
            {
                StartCoroutine(sendVerificationEmail());
            }
        }
    }

    // Handles the registration process.
    // _username: User's chosen username
    // _email: User's email
    // _password: User's password
    // _confirmPassword: Confirmation of password
    private IEnumerator registerLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        ShowLoading(true);

        // Validate registration inputs
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

            // Create new user in Firebase
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            // Handle registration errors
            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please try again";

                // Convert Firebase error codes to user-friendly messages
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
                // Setup user profile with username
                Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
                {
                    DisplayName = _username,
                    //TODO: give profile default photo
                };

                // Update user profile with username
                var DefaultUserTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => DefaultUserTask.IsCompleted);

                // Handle profile update errors
                if (DefaultUserTask.Exception != null)
                {
                    user.DeleteAsync();
                    FirebaseException firebaseException = (FirebaseException)DefaultUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please try again";

                    // Convert Firebase error codes to user-friendly messages
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

    // Sends a verification email to the current user.
    private IEnumerator sendVerificationEmail()
    {
        if (user != null)
        {
            // Send verification email
            var emailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

            // Handle verification email errors
            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown Error, Try Again!";

                // Convert Firebase error codes to user-friendly messages
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
}