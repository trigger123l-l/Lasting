
using System.Collections;
using UnityEngine;

public class MobileSquareSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject mobileSquarePrefab;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 6f;
    public float minimumSpawnInterval = 2f;

    [Header("Difficulty")]
    public float intervalDecreasePerSecond = 0.02f;

    private float elapsedTime;

    private void Start()
    {
        if (mobileSquarePrefab == null)
        {
            Debug.LogError(
                "MobileSquareSpawner necesita un MobileSquare Prefab.",
                this
            );

            enabled = false;
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float currentInterval = Mathf.Max(
                minimumSpawnInterval,
                initialSpawnInterval -
                elapsedTime * intervalDecreasePerSecond
            );

            float waitTime = Random.Range(
                currentInterval * 0.75f,
                currentInterval * 1.25f
            );

            waitTime = Mathf.Max(
                minimumSpawnInterval,
                waitTime
            );

            yield return new WaitForSeconds(waitTime);

            elapsedTime += waitTime;

            SpawnSquare();
        }
    }

    private void SpawnSquare()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z
        );

        Vector3 bottomLeft =
            mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0f, cameraDistance)
            );

        Vector3 topRight =
            mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 1f, cameraDistance)
            );

        float safeSize = 4f;

        for (int i = 0; i < 30; i++)
        {
            Vector2 spawnPosition = new Vector2(
                Random.Range(
                    bottomLeft.x + safeSize / 2f,
                    topRight.x - safeSize / 2f
                ),
                Random.Range(
                    bottomLeft.y + safeSize / 2f,
                    topRight.y - safeSize / 2f
                )
            );

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(
                spawnPosition,
                Vector2.one * safeSize,
                0f
            );

            bool occupiedByPlayer = false;

            foreach (Collider2D hit in overlaps)
            {
                if (hit.CompareTag("Player"))
                {
                    occupiedByPlayer = true;
                    break;
                }
            }

            if (!occupiedByPlayer)
            {
                Instantiate(
                    mobileSquarePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                return;
            }
        }

        Debug.Log("No se encontró una posición segura para MobileSquare.");
    }
}
