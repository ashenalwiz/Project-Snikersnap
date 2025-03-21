using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressTableManager : MonoBehaviour
{
    // File settings 
    private const string PROGRESS_FILENAME = "SithumiProgress.json";
    private string saveFilePath;

    // UI References
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject headerPrefab;
    [SerializeField] private GameObject sessionHeaderPrefab;

    // Use the shared data classes
    private SithumiProgress.ProgressData progressData;

    private void Awake()
    {
        saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, PROGRESS_FILENAME);
        Debug.Log("JSON file path: " + saveFilePath);
    }

    private void Start()
    {
        LoadProgressData();
        DisplayProgressData();
    }

    public void LoadProgressData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                progressData = JsonUtility.FromJson<SithumiProgress.ProgressData>(jsonData);
                Debug.Log($"Data loaded successfully with {progressData.sessions.Count} sessions");
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                progressData = new SithumiProgress.ProgressData { sessions = new List<SithumiProgress.Session>() };
            }
        }
        else
        {
            Debug.LogWarning("No progress file found at: " + saveFilePath);
            progressData = new SithumiProgress.ProgressData { sessions = new List<SithumiProgress.Session>() };
        }
    }

    public void DisplayProgressData()
    {
        // Clear existing content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create ONLY ONE column header
        GameObject header = Instantiate(headerPrefab, contentParent);
        // Don't call SetHeaderText here - the header prefab should already have text set correctly

        if (progressData.sessions == null || progressData.sessions.Count == 0)
        {
            GameObject emptyRow = Instantiate(rowPrefab, contentParent);
            TMP_Text[] texts = emptyRow.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0)
            {
                texts[0].text = "No data available";
                // Center the text across all columns
                RectTransform rectTransform = texts[0].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(800, rectTransform.sizeDelta.y);
                }
            }
            return;
        }

        // Display data for each session
        foreach (SithumiProgress.Session session in progressData.sessions)
        {
            // Create session header
            GameObject sessionHeader = Instantiate(
                sessionHeaderPrefab != null ? sessionHeaderPrefab : rowPrefab, contentParent);

            TMP_Text[] sessionTexts = sessionHeader.GetComponentsInChildren<TMP_Text>();
            if (sessionTexts.Length > 0)
            {
                // Use the first text component for the session date
                sessionTexts[0].text = "Session: " + session.date;
                sessionTexts[0].fontStyle = FontStyles.Bold;
                sessionTexts[0].fontSize += 2;
                sessionTexts[0].color = Color.blue;

                // Clear other text fields in the session header
                for (int i = 1; i < sessionTexts.Length; i++)
                {
                    sessionTexts[i].text = "";
                }
            }

            // Create rows for each round IN THE ORIGINAL ORDER from the JSON
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

                    // Apply color coding for better readability
                    if (round.skipped)
                    {
                        texts[4].color = Color.red;
                    }
                    else
                    {
                        texts[4].color = Color.green;
                    }

                    // Color-code accuracy
                    if (round.accuracy >= 80)
                        texts[2].color = new Color(0, 0.7f, 0);  // Green
                    else if (round.accuracy >= 50)
                        texts[2].color = new Color(0.9f, 0.6f, 0);  // Orange
                    else if (round.accuracy > 0)
                        texts[2].color = new Color(0.8f, 0, 0);  // Red
                    else
                        texts[2].color = Color.red;  // For 0.0%

                }

            }
        }
    }

    // Helper function to create sample data (for testing)
    public void CreateSampleData()
    {
        SithumiProgress.ProgressData sample = new SithumiProgress.ProgressData
        {
            sessions = new List<SithumiProgress.Session>
            {
                new SithumiProgress.Session
                {
                    date = DateTime.Now.ToString("dd-MM-yyyy"),
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

    // Add a method to clear all data (for testing purposes)
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