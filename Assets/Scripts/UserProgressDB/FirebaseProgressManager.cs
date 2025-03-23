using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using System;

public class FirebaseProgressManager : MonoBehaviour
{
    public static FirebaseProgressManager Instance { get; private set; }
    
    private DatabaseReference dbReference;
    private string userId;
    private bool isSyncing = false;
    
    // List of game progress file names - ensure this matches with FirebaseManager
    private readonly string[] progressFileNames = {
        "Task2UserProgress.json",
        "Task3UserProgress.json",
        "Task5UserProgress.json",
        "Task6UserProgress.json",
        "Task7UserProgress.json",
        "Task8UserProgress.json"
    };

    private float autoSaveInterval = 300f; // 5 minutes
    private float lastSaveTime;
    
    // Dictionary to store the last known timestamps from Firebase
    private Dictionary<string, long> fileTimestamps = new Dictionary<string, long>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        // Check if user is already logged in
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DownloadProgressFromFirebase();
        }
        
        lastSaveTime = Time.time;
    }

    private void Update()
    {
        // Check if it's time for auto-save
        if (Time.time - lastSaveTime >= autoSaveInterval)
        {
            PeriodicSync();
            lastSaveTime = Time.time;
        }
    }
    
    // Call this when user logs in
    public void OnUserLoggedIn(string uid)
    {
        userId = uid;
        // First download progress, then handle the merge with local data
        DownloadProgressFromFirebase(MergeLocalAndRemoteData);
    }
    
    // Merge local and remote data based on timestamps
    private void MergeLocalAndRemoteData()
    {
        foreach (string fileName in progressFileNames)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            string gameKey = fileName.Replace(".json", "");
            
            if (File.Exists(filePath))
            {
                // Check if we should upload based on timestamps
                if (fileTimestamps.ContainsKey(gameKey))
                {
                    long remoteTimestamp = fileTimestamps[gameKey];
                    if (ShouldUploadFile(filePath, remoteTimestamp))
                    {
                        UploadSingleFile(filePath, gameKey);
                    }
                }
                else
                {
                    // No remote data exists, upload local
                    UploadSingleFile(filePath, gameKey);
                }
            }
        }
    }
    
    // Helper method to determine if local file is newer
    private bool ShouldUploadFile(string localPath, long firebaseTimestamp)
    {
        if (File.Exists(localPath))
        {
            // Convert local file time to comparable format
            long localTimestamp = new FileInfo(localPath).LastWriteTimeUtc.Ticks;
            return localTimestamp > firebaseTimestamp;
        }
        return false;
    }
    
    // Upload a single file to Firebase
    private void UploadSingleFile(string filePath, string gameKey)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            
            // Create a JSON object that includes the current timestamp
            string jsonWithTimestamp = $"{{\"data\":{json},\"timestamp\":{DateTime.UtcNow.Ticks}}}";
            
            dbReference.Child("users").Child(userId).Child("progress").Child(gameKey)
                .SetRawJsonValueAsync(jsonWithTimestamp).ContinueWith(task => {
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Failed to upload {gameKey}: {task.Exception}");
                    }
                    else if (task.IsCompleted)
                    {
                        Debug.Log($"Successfully uploaded {gameKey}");
                    }
                });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error uploading {gameKey}: {ex.Message}");
        }
    }
    
    // Upload all local progress files to Firebase
    public void UploadProgressToFirebase()
    {
        if (string.IsNullOrEmpty(userId) || isSyncing) return;
        
        isSyncing = true;
        
        foreach (string fileName in progressFileNames)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(filePath))
            {
                string gameKey = fileName.Replace(".json", "");
                UploadSingleFile(filePath, gameKey);
            }
        }
        
        isSyncing = false;
    }
    
    // Download all progress files from Firebase with an optional callback
    public void DownloadProgressFromFirebase(Action onComplete = null)
    {
        if (string.IsNullOrEmpty(userId)) 
        {
            onComplete?.Invoke();
            return;
        }
        
        if (isSyncing)
        {
            // If already syncing, wait a bit and try again
            Invoke(nameof(RetryDownload), 1f);
            return;
        }
        
        isSyncing = true;
        fileTimestamps.Clear();
        
        dbReference.Child("users").Child(userId).Child("progress").GetValueAsync().ContinueWith(task => {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to download progress: " + task.Exception);
                    isSyncing = false;
                    onComplete?.Invoke();
                    return;
                }
                
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    
                    if (snapshot.Exists)
                    {
                        foreach (DataSnapshot gameSnapshot in snapshot.Children)
                        {
                            string gameKey = gameSnapshot.Key;
                            
                            // Try to get timestamp
                            if (gameSnapshot.Child("timestamp").Exists)
                            {
                                long timestamp = long.Parse(gameSnapshot.Child("timestamp").Value.ToString());
                                fileTimestamps[gameKey] = timestamp;
                            }
                            
                            // Get the actual data
                            if (gameSnapshot.Child("data").Exists)
                            {
                                string jsonData = gameSnapshot.Child("data").GetRawJsonValue();
                                
                                if (!string.IsNullOrEmpty(jsonData))
                                {
                                    string fileName = gameKey + ".json";
                                    string filePath = Path.Combine(Application.persistentDataPath, fileName);
                                    
                                    try
                                    {
                                        // Save to local file
                                        File.WriteAllText(filePath, jsonData);
                                        Debug.Log($"Downloaded and saved {fileName}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Error saving file {fileName}: {ex.Message}");
                                    }
                                }
                            }
                            else
                            {
                                // Legacy format - direct JSON
                                string jsonData = gameSnapshot.GetRawJsonValue();
                                
                                if (!string.IsNullOrEmpty(jsonData))
                                {
                                    string fileName = gameKey + ".json";
                                    string filePath = Path.Combine(Application.persistentDataPath, fileName);
                                    
                                    try
                                    {
                                        // Save to local file
                                        File.WriteAllText(filePath, jsonData);
                                        Debug.Log($"Downloaded and saved {fileName} (legacy format)");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Error saving file {fileName}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    
                    isSyncing = false;
                    Debug.Log("Progress download completed");
                    onComplete?.Invoke();
                }
            });
        });
    }
    
    // Retry download helper
    private void RetryDownload()
    {
        DownloadProgressFromFirebase();
    }
    
    // Call this when the app is closing or when user logs out
    public void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(userId))
        {
            // Force immediate upload
            UploadProgressToFirebase();
        }
    }
    
    // You can also call this periodically to ensure progress is saved
    public void PeriodicSync()
    {
        if (!string.IsNullOrEmpty(userId) && !isSyncing)
        {
            UploadProgressToFirebase();
        }
    }
}

// Helper class to run code on the main thread
// Add this class if you don't have it already
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<Action> _executionQueue = new Queue<Action>();
    
    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }
    
    private void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
    
    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}