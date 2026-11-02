
using System.Collections;
using UnityEngine;

public class RaySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject rayPrefab;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 5f;
    public float minimumSpawnInterval = 1f;

    [Header("Difficulty")]
    public float intervalDecreasePerSecond = 0.03f;

    [Header("Spawn Position")]
    [Range(0f, 0.4f)]
    public float edgeMargin = 0.15f;

    private Camera mainCamera;
    private float elapsedTime;

    private void Start()
    {
        mainCamera = Camera.main;

        if (rayPrefab == null)
        {
            Debug.LogError(
                "RaySpawner necesita un Ray Prefab.",
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

            SpawnRay();
        }
    }

    private void SpawnRay()
    {
        if (mainCamera == null || rayPrefab == null)
            return;

        Vector2 startPosition;
        Vector2 targetPosition;

        GetRandomEntryAndExit(
            out startPosition,
            out targetPosition
        );

        Vector2 direction =
            (targetPosition - startPosition).normalized;

        float travelDistance =
            Vector2.Distance(startPosition, targetPosition);

        GameObject ray = Instantiate(
            rayPrefab,
            startPosition,
            Quaternion.identity
        );

        Ray rayScript = ray.GetComponent<Ray>();

        if (rayScript != null)
        {
            rayScript.Initialize(
                startPosition,
                direction,
                travelDistance
            );
        }
    }

    private void GetRandomEntryAndExit(
        out Vector2 start,
        out Vector2 target)
    {
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

        float left = bottomLeft.x;
        float right = topRight.x;
        float bottom = bottomLeft.y;
        float top = topRight.y;

        int entrySide = Random.Range(0, 4);

        int exitSide = Random.Range(0, 3);

        if (exitSide >= entrySide)
            exitSide++;

        start = GetRandomPointOnSide(
            entrySide, left, right, bottom, top
        );

        target = GetRandomPointOnSide(
            exitSide, left, right, bottom, top
        );
    }

    private Vector2 GetRandomPointOnSide(
        int side,
        float left,
        float right,
        float bottom,
        float top)
    {
        float marginX = (right - left) * edgeMargin;
        float marginY = (top - bottom) * edgeMargin;

        switch (side)
        {
            case 0: // Arriba
                return new Vector2(
                    Random.Range(left + marginX, right - marginX),
                    top
                );

            case 1: // Derecha
                return new Vector2(
                    right,
                    Random.Range(bottom + marginY, top - marginY)
                );

            case 2: // Abajo
                return new Vector2(
                    Random.Range(left + marginX, right - marginX),
                    bottom
                );

            default: // Izquierda
                return new Vector2(
                    left,
                    Random.Range(bottom + marginY, top - marginY)
                );
        }
    }
}
