using System.Collections;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System;

public class UserProfileManager : MonoBehaviour
{
    public static UserProfileManager instance;

    private DatabaseReference databaseReference;

    [System.Serializable]
    public class UserData
    {
        public string userId;
        public string username;
        public string email;
        public long lastLogin; // Unix timestamp
        public string preferredLanguage = "English";
        public bool notificationsEnabled = true;
        // Add more fields as needed

        public UserData() { } // Required for Firebase

        public UserData(FirebaseUser user)
        {
            userId = user.UserId;
            username = user.DisplayName;
            email = user.Email;
            lastLogin = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    private UserData currentUserData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        try
        {
            // Initialize Firebase Database
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

            if (databaseReference == null)
            {
                Debug.LogError("Firebase Database reference is null. Make sure Firebase Database is properly initialized.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing Firebase Database: " + e.Message);
        }
    }

    // Call this after user authentication
    public void InitializeUserData(FirebaseUser user)
    {
        if (user == null) return;

        StartCoroutine(LoadUserData(user));
    }

    private IEnumerator LoadUserData(FirebaseUser user)
    {
        // Reference to this user's data in the database
        DatabaseReference userRef = databaseReference.Child("users").Child(user.UserId);

        // Attempt to get existing data
        var dataTask = userRef.GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
        {
            Debug.LogError("Failed to load user data: " + dataTask.Exception.Message);
            // Create new user data if we couldn't load existing data
            currentUserData = new UserData(user);
            StartCoroutine(SaveUserData()); // Save the new data
        }
        else if (!dataTask.Result.Exists)
        {
            // User doesn't exist in database yet, create new data
            currentUserData = new UserData(user);
            StartCoroutine(SaveUserData());
        }
        else
        {
            try
            {
                // User exists, parse the data
                string rawJson = dataTask.Result.GetRawJsonValue();
                if (string.IsNullOrEmpty(rawJson))
                {
                    Debug.LogError("User data exists but raw JSON is null or empty");
                    currentUserData = new UserData(user);
                }
                else
                {
                    currentUserData = JsonUtility.FromJson<UserData>(rawJson);
                    if (currentUserData == null)
                    {
                        Debug.LogError("Failed to parse user data from JSON");
                        currentUserData = new UserData(user);
                    }
                }

                // Update last login time
                currentUserData.lastLogin = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                StartCoroutine(SaveUserData());
            }
            catch (Exception e)
            {
                Debug.LogError("Exception parsing user data: " + e.Message);
                currentUserData = new UserData(user);
                StartCoroutine(SaveUserData());
            }
        }
    }

    public IEnumerator SaveUserData()
    {
        if (currentUserData == null || string.IsNullOrEmpty(currentUserData.userId))
        {
            Debug.LogError("Cannot save null or invalid user data");
            yield break;
        }

        // Convert to JSON
        string json = JsonUtility.ToJson(currentUserData);

        // Save to Firebase
        DatabaseReference userRef = databaseReference.Child("users").Child(currentUserData.userId);
        var saveTask = userRef.SetRawJsonValueAsync(json);

        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            Debug.LogError("Failed to save user data: " + saveTask.Exception.Message);
        }
        else
        {
            Debug.Log("User data saved successfully");
        }
    }

    // Methods to update user preferences
    public void UpdateUsername(string newUsername)
    {
        if (currentUserData != null)
        {
            currentUserData.username = newUsername;
            StartCoroutine(SaveUserData());
        }
    }

    public void SetPreferredLanguage(string language)
    {
        if (currentUserData != null)
        {
            currentUserData.preferredLanguage = language;
            StartCoroutine(SaveUserData());
        }
    }

    public void SetNotificationsEnabled(bool enabled)
    {
        if (currentUserData != null)
        {
            currentUserData.notificationsEnabled = enabled;
            StartCoroutine(SaveUserData());
        }
    }

    // Get the current user data
    public UserData GetUserData()
    {
        return currentUserData;
    }
}