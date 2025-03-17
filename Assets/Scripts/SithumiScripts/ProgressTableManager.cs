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

    // Data classes defined within this script to avoid conflicts
    [System.Serializable]
    private class Round
    {
        public int round;
        public string targetLetter;
        public float accuracy;
        public int attempts;
        public bool skipped;
    }

    [System.Serializable]
    private class Session
    {
        public string date;
        public List<Round> rounds;
    }

    [System.Serializable]
    private class ProgressData
    {
        public List<Session> sessions;
    }

    private ProgressData progressData;

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/" + PROGRESS_FILENAME;
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
                progressData = JsonUtility.FromJson<ProgressData>(jsonData);
                Debug.Log("Data loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                progressData = new ProgressData { sessions = new List<Session>() };
            }
        }
        else
        {
            Debug.LogWarning("No progress file found at: " + saveFilePath);
            progressData = new ProgressData { sessions = new List<Session>() };
        }
    }

    public void DisplayProgressData()
    {
        // Clear existing content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create column headers
        GameObject header = Instantiate(headerPrefab, contentParent);

        if (progressData.sessions == null || progressData.sessions.Count == 0)
        {
            GameObject emptyRow = Instantiate(rowPrefab, contentParent);
            TMP_Text[] texts = emptyRow.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0)
            {
                texts[0].text = "No data available";
            }
            return;
        }

        // Display data
        foreach (Session session in progressData.sessions)
        {
            // Create session header
            GameObject sessionHeader = Instantiate(headerPrefab, contentParent);
            TMP_Text sessionText = sessionHeader.GetComponentInChildren<TMP_Text>();
            if (sessionText != null)
            {
                sessionText.text = "Session: " + session.date;
            }

            // Create rows for each round
            foreach (Round round in session.rounds)
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
                }
            }
        }
    }

    // Helper function to create sample data (for testing)
    public void CreateSampleData()
    {
        ProgressData sample = new ProgressData
        {
            sessions = new List<Session>
            {
                new Session
                {
                    date = "17-03-2025",
                    rounds = new List<Round>
                    {
                        new Round { round = 1, targetLetter = "Q-q", accuracy = 0.0f, attempts = 0, skipped = true },
                        new Round { round = 2, targetLetter = "T-t", accuracy = 0.0f, attempts = 0, skipped = true }
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
}