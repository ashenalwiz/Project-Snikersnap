using TMPro;
using UnityEngine;
public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance;
    [Header("References")]
    [SerializeField]
    private GameObject checkingForAccountUI;
    [SerializeField]
    private GameObject loginUI;
    [SerializeField]
    private GameObject registerUI;
    [SerializeField]
    private GameObject verifyEmailUI;
    [SerializeField]
    private GameObject passwordResetUI;
    [SerializeField]
    private GameObject loadingUI;
    [SerializeField]
    private TMP_Text verifyEmailText;
    [Header("Verification Screen")]
    [SerializeField]
    private GameObject resendButton;
    [SerializeField]
    private GameObject backToLoginButton;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void ClearUI()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        FirebaseManager.instance.ClearOutputs();
        verifyEmailUI.SetActive(false);
        passwordResetUI.SetActive(false);
        loadingUI.SetActive(false);
        checkingForAccountUI.SetActive(false);
    }
    // Password Reset Screen
    public void PasswordResetScreen()
    {
        ClearUI();
        passwordResetUI.SetActive(true);
    }
    // Show/Hide Loading Screen
    public void ShowLoading(bool show)
    {
        if (loadingUI != null)
            loadingUI.SetActive(show);
    }
    public void LoginScreen()
    {
        // Stop checking for email verification when leaving the verification screen
        FirebaseManager.instance.StopVerificationCheck();
        ClearUI();
        loginUI.SetActive(true);
    }
    public void RegisterScreen()
    {
        ClearUI();
        registerUI.SetActive(true);
    }
    public void AwaitVerification(bool emailSent, string email, string output)
    {
        ClearUI();
        verifyEmailUI.SetActive(true);
        if (emailSent)
        {
            verifyEmailText.text = $"Sent Email!\nPlease Verify {email}\n\nChecking for verification...";
        }
        else
        {
            verifyEmailText.text = $"Email Not Sent: {output}\nPlease Verify {email}";
        }
        // Make the resend button visible after 30 seconds
        if (resendButton != null)
        {
            resendButton.SetActive(false);
            Invoke("ShowResendButton", 30f);
        }
    }
    private void ShowResendButton()
    {
        if (verifyEmailUI.activeSelf && resendButton != null)
        {
            resendButton.SetActive(true);
        }
    }
    // Method for Resend button
    public void ResendVerificationEmail()
    {
        FirebaseManager.instance.ResendVerificationEmail();
        // Hide resend button again
        if (resendButton != null)
        {
            resendButton.SetActive(false);
            Invoke("ShowResendButton", 30f);
        }
    }
}