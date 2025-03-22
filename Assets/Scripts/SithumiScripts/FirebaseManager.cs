using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Auth;
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

    [Header("Loading")]
    [SerializeField]
    private GameObject loadingPanel;

    // for persistent login
    private const string RememberMeKey = "FirebaseRememberMe";

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

        // Clear remember me preference
        PlayerPrefs.SetInt(RememberMeKey, 0);
        PlayerPrefs.Save();

        auth.SignOut();

        // Wait for auth state to update
        yield return new WaitForSeconds(0.5f);

        ShowLoading(false);
        AuthUIManager.instance.LoginScreen();
    }

    // Remember Me functionality
    private void UpdateRememberMePreference(bool remember)
    {
        PlayerPrefs.SetInt(RememberMeKey, remember ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RememberMeToggleChanged()
    {
        UpdateRememberMePreference(rememberMeToggle.isOn);
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
        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();
            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);
            AutoLogin();
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
        StartCoroutine(loginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(registerLogic(registerUsername.text, RegisterEmail.text, RegisterPassword.text, RegisterConfirmPassword.text));
    }

    // Login method with Remember Me
    private IEnumerator loginLogic(string email, string password)
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
            // Save remember me preference
            UpdateRememberMePreference(rememberMeToggle.isOn);

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
                            output = "Session Expire";
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

    public void ResendVerificationEmail()
    {
        StartCoroutine(sendVerificationEmail());
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
                Debug.Log("Email Send Successfully");
            }
        }
    }
}