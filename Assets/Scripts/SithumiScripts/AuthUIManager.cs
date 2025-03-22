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
            verifyEmailText.text = $"Sent Email!\nPlease Verify {email}";
        }
        else
        {
            verifyEmailText.text = $"Email Not Sent: {output}\nPlease Verify {email}";
        }
    }
}