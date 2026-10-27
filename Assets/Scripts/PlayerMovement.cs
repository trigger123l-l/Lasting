
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Visual")]
    public Transform visual;
    public float stretchAmount = 0.2f;
    public float stretchSpeed = 10f;

    [Header("Dash Visual")]
    public float dashStretchAmount = 0.6f;
    public float dashStretchSpeed = 20f;

    [Header("Movement Particles")]
    public ParticleSystem movementParticles;
    public float particleOffset = 0.35f;
    public float emissionRate = 35f;

    [Header("Screen Boundaries")]
    public float screenMargin = 0.02f;

    private Rigidbody2D rb;
    private PlayerDash dash;
    private Vector2 movement;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Camera mainCamera;
    private float playerHalfWidth;
    private float playerHalfHeight;

    private ParticleSystem.EmissionModule emission;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dash = GetComponent<PlayerDash>();
        mainCamera = Camera.main;

        if (visual != null)
        {
            originalScale = visual.localScale;
            originalRotation = visual.localRotation;
        }

        SpriteRenderer spriteRenderer = visual != null
            ? visual.GetComponent<SpriteRenderer>()
            : GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            playerHalfWidth = spriteRenderer.bounds.extents.x;
            playerHalfHeight = spriteRenderer.bounds.extents.y;
        }
        else
        {
            Collider2D playerCollider = GetComponent<Collider2D>();

            if (playerCollider != null)
            {
                playerHalfWidth = playerCollider.bounds.extents.x;
                playerHalfHeight = playerCollider.bounds.extents.y;
            }
        }

        if (movementParticles != null)
        {
            emission = movementParticles.emission;
            emission.rateOverTime = 0f;

            movementParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        UpdateVisual();
        UpdateParticles();
    }

    private void UpdateVisual()
    {
        if (visual == null)
            return;

        Vector2 direction = Vector2.zero;
        float amount = 0f;
        float visualSpeed = stretchSpeed;

        if (dash != null && dash.IsDashing)
        {
            direction = dash.DashDirection.normalized;
            amount = dashStretchAmount;
            visualSpeed = dashStretchSpeed;
        }
        else if (movement.sqrMagnitude > 0.01f)
        {
            direction = movement.normalized;
            amount = stretchAmount;
        }

        Vector3 targetScale = originalScale;
        Quaternion targetRotation = originalRotation;

        if (direction.sqrMagnitude > 0.01f)
        {
            // Orientar el cubo hacia la dirección del movimiento.
            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

            targetRotation = Quaternion.Euler(0f, 0f, angle);

            // Estirar en la dirección del movimiento
            // y estrechar el eje perpendicular.
            targetScale.x *= 1f + amount;
            targetScale.y *= Mathf.Max(0.1f, 1f - amount * 0.6f);
        }

        visual.localRotation = Quaternion.Slerp(
            visual.localRotation,
            targetRotation,
            visualSpeed * Time.deltaTime
        );

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            targetScale,
            visualSpeed * Time.deltaTime
        );
    }

    private void UpdateParticles()
    {
        if (movementParticles == null)
            return;

        bool isMoving = movement.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            emission.rateOverTime = 0f;

            if (movementParticles.isPlaying)
            {
                movementParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );
            }

            return;
        }

        Vector3 oppositeDirection = new Vector3(
            -movement.x,
            -movement.y,
            0f
        );

        movementParticles.transform.localPosition =
            oppositeDirection * particleOffset;

        emission.rateOverTime = emissionRate;

        if (!movementParticles.isPlaying)
            movementParticles.Play();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (dash != null && dash.IsDashing)
            return;

        rb.linearVelocity = movement * speed;

        if (mainCamera == null || !mainCamera.orthographic)
            return;

        rb.position = ClampToScreen(rb.position);
    }

    private Vector2 ClampToScreen(Vector2 position)
    {
        if (mainCamera == null || !mainCamera.orthographic)
            return position;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float cameraX = mainCamera.transform.position.x;
        float cameraY = mainCamera.transform.position.y;

        float minX = cameraX - cameraWidth
            + playerHalfWidth + screenMargin;

        float maxX = cameraX + cameraWidth
            - playerHalfWidth - screenMargin;

        float minY = cameraY - cameraHeight
            + playerHalfHeight + screenMargin;

        float maxY = cameraY + cameraHeight
            - playerHalfHeight - screenMargin;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}
