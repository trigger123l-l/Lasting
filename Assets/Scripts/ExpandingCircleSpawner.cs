
using System.Collections;
using UnityEngine;

public class ExpandingCircleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject expandingCirclePrefab;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 7f;
    public float minimumSpawnInterval = 2.5f;

    [Header("Difficulty")]
    public float intervalDecreasePerSecond = 0.015f;

    private float elapsedTime;

    private void Start()
    {
        if (expandingCirclePrefab == null)
        {
            Debug.LogError(
                "ExpandingCircleSpawner necesita un ExpandingCircle Prefab.",
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

            SpawnCircle();
        }
    }

    private void SpawnCircle()
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

        float margin = 5f;

        Vector2 spawnPosition = new Vector2(
            Random.Range(
                bottomLeft.x + margin,
                topRight.x - margin
            ),
            Random.Range(
                bottomLeft.y + margin,
                topRight.y - margin
            )
        );

        Instantiate(
            expandingCirclePrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}
