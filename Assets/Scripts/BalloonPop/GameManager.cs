using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text chancesText;
    public TMP_Text scoreText;
    public TMP_Text missedNumberText;
    public TMP_Text gameOverText;  // Add Game Over UI text

    public int TargetNumber { get; private set; }

    private int remainingChances = 5;
    private int score = 0;
    private readonly List<int> availableNumbers = new();
    private bool showingMissedNumber = false;
    private bool gameOver = false;
    private BalloonSpawner balloonSpawner;

    void Start()
    {
        if (!ValidateUI()) return;

        balloonSpawner = FindAnyObjectByType<BalloonSpawner>();
        if (balloonSpawner == null)
        {
            Debug.LogError("BalloonSpawner not found!");
            return;
        }

        gameOverText.gameObject.SetActive(false); // Hide game over text at the start

        for (int i = 1; i <= 10; i++)
            availableNumbers.Add(i);

        SetNewNumber();
    }

    bool ValidateUI()
    {
        if (instructionText == null || chancesText == null || scoreText == null || missedNumberText == null || gameOverText == null)
        {
            Debug.LogError("UI elements are not assigned! Assign them in the Inspector.");
            return false;
        }
        return true;
    }

    public void SetNewNumber()
    {
        if (!ValidateUI() || gameOver) return;

        if (score >= 10) // Check if score reaches 10
        {
            GameOver();
            return;
        }

        if (availableNumbers.Count == 0)
        {
            GameOver();
            return;
        }

        TargetNumber = availableNumbers[Random.Range(0, availableNumbers.Count)];
        instructionText.text = $"Pop number {TargetNumber}!";
        remainingChances = 5;
        chancesText.text = $"Chances: {remainingChances}";
        missedNumberText.gameObject.SetActive(false);

        if (!gameOver)
            balloonSpawner.SpawnBalloons();
    }

    public void CorrectNumberPopped()
    {
        availableNumbers.Remove(TargetNumber);
        score++;
        scoreText.text = $"Points: {score}";
        showingMissedNumber = false;

        if (score >= 10) // Check if the game should end
        {
            GameOver();
        }
        else
        {
            SetNewNumber();
        }
    }

    public void CheckNumber(int number)
    {
        if (showingMissedNumber || gameOver) return;

        remainingChances--;
        chancesText.text = $"Chances: {remainingChances}";

        if (remainingChances <= 0)
        {
            StartCoroutine(ShowMissedMessage());
        }
    }

    IEnumerator ShowMissedMessage()
    {
        showingMissedNumber = true;
        instructionText.text = "You missed!";
        missedNumberText.text = $"The number was ({TargetNumber})";
        missedNumberText.gameObject.SetActive(true);

        balloonSpawner.StopSpawningBalloons();
        balloonSpawner.HideAllBalloons();

        yield return new WaitForSeconds(7f);

        missedNumberText.gameObject.SetActive(false);
        showingMissedNumber = false;
        SetNewNumber();
        balloonSpawner.ShowAllBalloons();
        balloonSpawner.ResumeSpawningBalloons();
    }

    void GameOver()
    {
        gameOver = true;
        balloonSpawner.StopSpawningBalloons();
        balloonSpawner.HideAllBalloons(); // Hide balloons immediately

        // Hide other UI elements
        instructionText.gameObject.SetActive(false);
        chancesText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        missedNumberText.gameObject.SetActive(false);

        // Show Game Over message
        gameOverText.text = $"Game Over! Total Points: {score}";
        gameOverText.gameObject.SetActive(true);
    }

    public bool IsShowingMissedNumber() => showingMissedNumber;
    public bool IsGameOver() => gameOver;
}

