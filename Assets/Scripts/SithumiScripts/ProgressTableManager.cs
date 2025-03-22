using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressTableManager : MonoBehaviour
{
    // Define the filename for storing progress data
    private const string PROGRESS_FILENAME = "SithumiProgress.json";
    private string saveFilePath;

    // UI References: Assign in Unity Inspector
    [SerializeField] private Transform contentParent;   // Parent for dynamically created UI elements
    [SerializeField] private GameObject rowPrefab;      // Prefab for individual data rows
    [SerializeField] private GameObject headerPrefab;   // Prefab for table headers
    [SerializeField] private GameObject sessionHeaderPrefab;  // Prefab for session headers

    // Progress data object (stores all session and round details)
    private SithumiProgress.ProgressData progressData;

    private void Awake()
    {
        // Define the path where progress data will be saved
        saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, PROGRESS_FILENAME);
        Debug.Log("JSON file path: " + saveFilePath);
    }

    private void Start()
    {
        // Load and display the progress data on startup
        LoadProgressData();
        DisplayProgressData();
    }

    /// Loads progress data from a JSON file.
    public void LoadProgressData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                Debug.Log("Raw JSON: " + jsonData);

                // Attempt to deserialize the JSON data into the ProgressData class
                progressData = JsonUtility.FromJson<SithumiProgress.ProgressData>(jsonData);
                Debug.Log($"Data loaded successfully with {progressData.sessions.Count} sessions");

                // Validate session data if necessary
                if (progressData.sessions.Count > 0)
                {
                    bool needsValidation = true;
                    foreach (var session in progressData.sessions)
                    {
                        if (!string.IsNullOrEmpty(session.Date))
                        {
                            needsValidation = false;
                            break;
                        }
                    }

                    if (needsValidation)
                    {
                        Debug.LogWarning("Date field appears empty, attempting to handle possible format issues...");
                        ValidateAndRepairSessionData();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                // Initialize an empty progress data object to avoid null references
                progressData = new SithumiProgress.ProgressData { sessions = new List<SithumiProgress.Session>() };
            }
        }
        else
        {
            Debug.LogWarning("No progress file found at: " + saveFilePath);
            // If no file exists, create an empty data structure
            progressData = new SithumiProgress.ProgressData { sessions = new List<SithumiProgress.Session>() };
        }
    }

    /// Validates and repairs missing session data fields.
    private void ValidateAndRepairSessionData()
    {
        for (int i = 0; i < progressData.sessions.Count; i++)
        {
            var session = progressData.sessions[i];

            // If the Date field is missing but rounds exist, generate a placeholder date
            if (string.IsNullOrEmpty(session.Date) && session.rounds.Count > 0)
            {
                session.Date = "Session " + (i + 1);
                Debug.LogWarning($"Repaired missing date for session {i + 1}");
            }
        }
    }

    /// Displays progress data in the UI.
    public void DisplayProgressData()
    {
        // Clear existing UI elements before adding new ones
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create a single column header row
        GameObject header = Instantiate(headerPrefab, contentParent);

        // If no data exists, show a placeholder message
        if (progressData.sessions == null || progressData.sessions.Count == 0)
        {
            GameObject emptyRow = Instantiate(rowPrefab, contentParent);
            TMP_Text[] texts = emptyRow.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0)
            {
                texts[0].text = "No data available";
                // Center the text
                RectTransform rectTransform = texts[0].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(800, rectTransform.sizeDelta.y);
                }
            }
            return;
        }

        // Iterate through each session and display its data
        foreach (SithumiProgress.Session session in progressData.sessions)
        {
            // Create a session header row
            GameObject sessionHeader = Instantiate(
                sessionHeaderPrefab != null ? sessionHeaderPrefab : rowPrefab, contentParent);

            TMP_Text[] sessionTexts = sessionHeader.GetComponentsInChildren<TMP_Text>();
            if (sessionTexts.Length > 0)
            {
                sessionTexts[0].text = "Session: " + session.Date;
                sessionTexts[0].fontStyle = FontStyles.Bold;
                sessionTexts[0].fontSize += 2;
                sessionTexts[0].color = Color.blue;

                // Clear any additional text fields in the session header
                for (int i = 1; i < sessionTexts.Length; i++)
                {
                    sessionTexts[i].text = "";
                }
            }

            // Display round data in order
            foreach (SithumiProgress.Round round in session.rounds)
            {
                GameObject row = Instantiate(rowPrefab, contentParent);
                TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

                if (texts.Length >= 5)
                {
                    texts[0].text = round.round.ToString();
                    texts[1].text = round.targetLetter;
                    texts[2].text = round.accuracy.ToString("F1") + "%";
                    texts[3].text = round.attempts.ToString();
                    texts[4].text = round.skipped ? "Yes" : "No";

                    // Apply color coding for readability
                    texts[4].color = round.skipped ? Color.red : Color.green;

                    // Color-code accuracy percentage
                    if (round.accuracy >= 80)
                        texts[2].color = new Color(0, 0.7f, 0);  // Green
                    else if (round.accuracy >= 50)
                        texts[2].color = new Color(0.9f, 0.6f, 0);  // Orange
                    else
                        texts[2].color = new Color(0.8f, 0, 0);  // Red
                }
            }
        }
    }

    /// Creates sample progress data for testing.
    public void CreateSampleData()
    {
        SithumiProgress.ProgressData sample = new SithumiProgress.ProgressData
        {
            sessions = new List<SithumiProgress.Session>
            {
                new SithumiProgress.Session
                {
                    Date = DateTime.Now.ToString("dd-MM-yyyy"),
                    rounds = new List<SithumiProgress.Round>
                    {
                        new SithumiProgress.Round { round = 1, targetLetter = "A-a", accuracy = 85.5f, attempts = 10, skipped = false },
                        new SithumiProgress.Round { round = 2, targetLetter = "B-b", accuracy = 70.0f, attempts = 8, skipped = false },
                        new SithumiProgress.Round { round = 3, targetLetter = "C-c", accuracy = 55.5f, attempts = 12, skipped = false },
                        new SithumiProgress.Round { round = 4, targetLetter = "D-d", accuracy = 90.0f, attempts = 5, skipped = false },
                        new SithumiProgress.Round { round = 5, targetLetter = "E-e", accuracy = 100.0f, attempts = 3, skipped = false }
                    }
                }
            }
        };

        string json = JsonUtility.ToJson(sample, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Sample data created at: " + saveFilePath);

        LoadProgressData();
        DisplayProgressData();
    }

    /// Clears all progress data.
    public void ClearAllData()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Progress data file deleted");
        }

        progressData = new SithumiProgress.ProgressData { sessions = new List<SithumiProgress.Session>() };
        DisplayProgressData();
    }
}
