using TMPro;
using UnityEngine;
using System.Collections;

public class Balloon : MonoBehaviour
{
    public float speed = 2.5f;
    public TMP_Text numberText;
    private int number;
    public BalloonSpawner spawner;

    private bool isMoving = true;

    public void SetNumber(int num)
    {
        if (numberText == null)
        {
            Debug.LogError("numberText is null! Check if TMP_Text is assigned in Balloon prefab.");
            return;
        }
        number = num;
        numberText.text = num.ToString();
        numberText.transform.localScale = Vector3.one * 0.5f;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }

        if (transform.position.y > 6f)
        {
            spawner?.BalloonDestroyed();
            Destroy(gameObject);
        }
    }

    void OnMouseDown()
    {
        if (!gameObject.activeSelf) return;  // Ensure the GameObject is active before proceeding

        GameManagerThrishali gameManager = FindAnyObjectByType<GameManagerThrishali>();
        if (gameManager == null || gameManager.IsShowingMissedNumber()) return;

        if (number == gameManager.TargetNumber)
        {
            gameManager.CorrectNumberPopped();
            Destroy(gameObject);
        }
        else
        {
            gameManager.CheckNumber(number);

            // Only start the coroutine if the object is still active
            if (gameObject.activeSelf)
            {
                StartCoroutine(ShakeBalloon());
            }
        }
    }


    IEnumerator ShakeBalloon()
    {
        Vector3 originalPos = transform.position;
        for (int i = 0; i < 5; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        transform.position = originalPos;
    }

    public void StopMovement() => isMoving = false;
    public void ResumeMovement() => isMoving = true;
}
