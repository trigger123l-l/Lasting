
using System.Collections;
using UnityEngine;

public class Ray : MonoBehaviour
{
    [Header("Ray Settings")]
    public float rayLength = 20f;
    public float rayWidth = 0.15f;
    public float movementSpeed = 3f;

    [Header("Appearance")]
    public float initialAlpha = 0.05f;
    public float activeAlpha = 1f;
    public float activeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D rayCollider;

    private Vector2 movementDirection;
    private Vector2 targetPosition;

    private bool initialized;
    private bool isActive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rayCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null || rayCollider == null)
        {
            Debug.LogError(
                "Ray necesita SpriteRenderer y BoxCollider2D.",
                this
            );

            enabled = false;
            return;
        }

        rayCollider.isTrigger = true;
        rayCollider.enabled = true;
    }

    public void Initialize(
        Vector2 startPosition,
        Vector2 direction,
        float travelDistance)
    {
        if (!enabled)
            return;

        transform.position = startPosition;

        movementDirection = direction.normalized;

        targetPosition =
            startPosition + movementDirection * travelDistance;

        float angle = Mathf.Atan2(
            movementDirection.y,
            movementDirection.x
        ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle - 90f);

        transform.localScale = new Vector3(
            rayWidth,
            rayLength,
            1f
        );

        spriteRenderer.color = new Color(
            1f, 0f, 0f, initialAlpha
        );

        initialized = true;

        StartCoroutine(RayRoutine());
    }

    private IEnumerator RayRoutine()
    {
        isActive = true;

        float travelDuration = Vector2.Distance(
            transform.position,
            targetPosition
        ) / movementSpeed;

        float elapsed = 0f;

        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / travelDuration
            );

            float alpha = Mathf.Lerp(
                initialAlpha,
                activeAlpha,
                progress
            );

            spriteRenderer.color = new Color(
                1f, 0f, 0f, alpha
            );

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;

        spriteRenderer.color = new Color(
            1f, 0f, 0f, activeAlpha
        );

        yield return new WaitForSeconds(activeDuration);

        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!initialized || !isActive)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.TakeDamage();
    }
}
