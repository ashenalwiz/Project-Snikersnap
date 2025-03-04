using UnityEngine;
using System.Collections.Generic;

public class BalloonSpawner : MonoBehaviour
{
    public GameObject balloonPrefab;
    public Transform spawnPoint;
    private int activeBalloons = 0;
    private bool canSpawnBalloons = true;

    private readonly List<Vector2> availablePositions = new()
    {
        new Vector2(-6f, -6f),
        new Vector2(-3f, -7f),
        new Vector2(0f, -5.5f),
        new Vector2(3f, -6.5f),
        new Vector2(6f, -7f)
    };

    void Start()
    {
        InvokeRepeating(nameof(CheckAndRespawn), 1f, 1f);
        SpawnBalloons();
    }

    public void HideAllBalloons()
    {
        foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
        {
            balloon.SetActive(false);
        }
    }

    public void ShowAllBalloons()
    {
        foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
        {
            balloon.SetActive(true);
        }
    }

    public void SpawnBalloons()
    {
        if (!canSpawnBalloons) return;

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null || gameManager.IsGameOver()) return;

        foreach (GameObject balloon in GameObject.FindGameObjectsWithTag("Balloon"))
        {
            Destroy(balloon);
        }

        int correctNumber = gameManager.TargetNumber;
        bool missedLastRound = gameManager.IsShowingMissedNumber();
        int totalBalloons = missedLastRound ? 1 : 4;

        List<Vector2> shuffledPositions = new List<Vector2>(availablePositions);
        ShuffleList(shuffledPositions);

        activeBalloons = totalBalloons;

        for (int i = 0; i < totalBalloons; i++)
        {
            Vector2 spawnPos = shuffledPositions[i];
            GameObject balloon = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);
            balloon.tag = "Balloon";
            Balloon balloonScript = balloon.GetComponent<Balloon>();
            if (balloonScript != null)
            {
                balloonScript.spawner = this;
                balloonScript.SetNumber(i == 0 ? correctNumber : Random.Range(1, 11));
            }
        }
    }

    public void BalloonDestroyed()
    {
        activeBalloons--;
        if (activeBalloons <= 0)
        {
            SpawnBalloons();
        }
    }

    void CheckAndRespawn()
    {
        if (Object.FindObjectsByType<Balloon>(FindObjectsSortMode.None).Length == 0 && canSpawnBalloons)
        {
            SpawnBalloons();
        }
    }


    void ShuffleList(List<Vector2> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void StopSpawningBalloons() => canSpawnBalloons = false;
    public void ResumeSpawningBalloons() => canSpawnBalloons = true;
}

