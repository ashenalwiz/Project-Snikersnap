using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

public class ProgressTask8 : MonoBehaviour
{
    [Header("Progress Tracking")]
    public GameObject progressPanel;
    public Transform progressContentParent;
    public GameObject rowPrefab;
    public GameObject headerRow;
    public GameObject backButton;

    private string saveFileName = "Task8UserProgress.json";
    private SessionData2 sessionData;

    [System.Serializable]
    public class WordAttempt
    {
        public string word;
        public int attempts;
        public bool correct;
        public bool skipped;
    }

    [System.Serializable]
    public class SessionData2
    {
        public List<WordAttempt> wordAttempts = new List<WordAttempt>();
    }

    void Start()
    {
        LoadProgressData();
        ShowProgressDetails();
    }

    void LoadProgressData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            sessionData = JsonUtility.FromJson<SessionData2>(jsonData);
            Debug.Log($"Loaded progress data with {sessionData.wordAttempts.Count} word attempts");
        }
        else
        {
            Debug.LogWarning($"No progress file found at {filePath}");
            sessionData = new SessionData2();
        }
    }

    public void ShowProgressDetails()
    {
        progressPanel.SetActive(true);
        backButton.SetActive(true);

        // Clear existing rows except for the header
        foreach (Transform child in progressContentParent)
        {
            if (child == headerRow.transform) continue;
            Destroy(child.gameObject);
        }

        // Display word attempts
        foreach (var attempt in sessionData.wordAttempts)
        {
            GameObject row = Instantiate(rowPrefab, progressContentParent);
            row.SetActive(true);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 4)
            {
                texts[0].text = attempt.word;
                texts[1].text = attempt.attempts.ToString();
                texts[2].text = attempt.correct ? "Yes" : "No";
                texts[3].text = attempt.skipped ? "Yes" : "No";
            }
        }
    }

    public void CloseProgressPanel()
    {
        progressPanel.SetActive(false);
    }
}