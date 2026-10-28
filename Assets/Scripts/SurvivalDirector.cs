
using UnityEngine;

public class SurvivalDirector : MonoBehaviour
{
    [Header("References")]
    public GameObject redBurstPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 5f;
    public float minimumSpawnInterval = 1.5f;
    public float safeDistanceFromPlayer = 2.5f;
    public int maxAttempts = 20;

    private float elapsedTime;
    private float spawnTimer;

    public float ElapsedTime => elapsedTime;
    public float Difficulty => Mathf.Clamp01(elapsedTime / 1200f);

    private void Start()
    {
        Time.timeScale = 1f;
        spawnTimer = initialSpawnInterval;
    }

    private void Update()
    {
        if (player == null || redBurstPrefab == null)
            return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null && health.IsDead)
            return;

        elapsedTime += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        float difficulty = Difficulty;

        float currentInterval = Mathf.Lerp(
            initialSpawnInterval,
            minimumSpawnInterval,
            difficulty
        );

        if (spawnTimer <= 0f)
        {
            SpawnRedBurst();
            spawnTimer = currentInterval;
        }
    }

    private void SpawnRedBurst()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning(
                "SurvivalDirector necesita una cámara principal ortográfica."
            );
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector2 cameraCenter = cam.transform.position;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 position = new Vector2(
                Random.Range(
                    cameraCenter.x - halfWidth + 0.75f,
                    cameraCenter.x + halfWidth - 0.75f
                ),
                Random.Range(
                    cameraCenter.y - halfHeight + 0.75f,
                    cameraCenter.y + halfHeight - 0.75f
                )
            );

            if (Vector2.Distance(position, player.position)
                < safeDistanceFromPlayer)
            {
                continue;
            }

            Instantiate(
                redBurstPrefab,
                position,
                Quaternion.identity
            );

            return;
        }

        Debug.Log("No se encontró una posición segura para RedBurst.");
    }
}
