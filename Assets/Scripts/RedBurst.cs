
using System.Collections;
using UnityEngine;

public class RedBurst : MonoBehaviour
{
    [Header("Warning")]
    public float warningDuration = 1.5f;
    public float activeDuration = 0.25f;

    [Header("Explosion")]
    public GameObject spikePrefab;
    public int minSpikeCount = 16;
    public int maxSpikeCount = 150;
    public float spikeSpeed = 5f;
    public float spikeLifetime = 2f;

    private SpriteRenderer spriteRenderer;
    private Collider2D obstacleCollider;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        obstacleCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null || obstacleCollider == null)
        {
            Debug.LogError(
                "RedBurst necesita un SpriteRenderer y un Collider2D.",
                this
            );
            enabled = false;
            return;
        }

        obstacleCollider.enabled = false;

        spriteRenderer.color = new Color(1f, 0f, 0f, 0.15f);

        StartCoroutine(ObstacleRoutine());
    }

    private IEnumerator ObstacleRoutine()
    {
        float timer = 0f;

        while (timer < warningDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / warningDuration);
            float alpha = Mathf.Lerp(0.15f, 1f, progress);

            spriteRenderer.color = new Color(1f, 0f, 0f, alpha);

            yield return null;
        }

        spriteRenderer.color = Color.red;
        obstacleCollider.enabled = true;

        yield return new WaitForSeconds(activeDuration);

        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (spikePrefab == null)
        {
            Debug.LogWarning(
                "RedBurst: No se ha asignado Spike Prefab.",
                this
            );
            return;
        }

        int spikeCount = Random.Range(
            minSpikeCount,
            maxSpikeCount + 1
        );

        float angleStep = 360f / spikeCount;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < spikeCount; i++)
        {
            float angle = startAngle + i * angleStep;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            Vector3 spawnPosition =
                transform.position + (Vector3)(direction * 0.3f);

            GameObject spike = Instantiate(
                spikePrefab,
                spawnPosition,
                Quaternion.identity
            );

            Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError(
                    "El prefab Spike necesita un Rigidbody2D en su objeto raíz.",
                    spike
                );

                Destroy(spike);
                continue;
            }

            rb.gravityScale = 0f;
            rb.angularVelocity = 0f;
            rb.linearVelocity = direction * spikeSpeed;

            Destroy(spike, spikeLifetime);
        }
    }
}
