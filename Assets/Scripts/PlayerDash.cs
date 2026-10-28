
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDistance = 2.5f;
    public float dashCooldown = 0.1f;
    public float dashProtectionAfterDash = 0f;

    [Header("References")]
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public LayerMask obstacleLayer;

    private Vector2 dashDirection;
    private Vector2 dashTarget;

    private float dashRemainingDistance;
    private float nextDashTime;
    private bool isDashing;

    private bool dashProtection;
    private float dashProtectionTimer;

    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;

    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    private readonly List<Collider2D> ignoredObstacles =
        new List<Collider2D>();

    public bool IsDashing => isDashing;
    public Vector2 DashDirection => dashDirection;
    public bool IsDashProtected => isDashing || dashProtection;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();

        mainCamera = Camera.main;

        if (rb != null && playerCollider != null)
        {
            colliderSize = playerCollider.bounds.size;
            colliderOffset =
                (Vector2)playerCollider.bounds.center - rb.position;
        }

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            halfWidth = spriteRenderer.bounds.extents.x;
            halfHeight = spriteRenderer.bounds.extents.y;
        }
        else if (playerCollider != null)
        {
            halfWidth = playerCollider.bounds.extents.x;
            halfHeight = playerCollider.bounds.extents.y;
        }
    }

    private void Update()
    {
        if (rb == null)
            return;

        if (dashProtection && !isDashing)
        {
            dashProtectionTimer -= Time.deltaTime;

            if (dashProtectionTimer <= 0f)
                dashProtection = false;
        }

        Vector2 inputDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
            TryStartDash(inputDirection);
    }

    private void TryStartDash(Vector2 inputDirection)
    {
        if (isDashing || rb == null || playerCollider == null)
            return;

        if (inputDirection.sqrMagnitude < 0.01f)
            return;

        if (Time.time < nextDashTime)
            return;

        // No iniciar el Dash dentro de un obstáculo.
        if (IsInsideObstacle(rb.position))
            return;

        dashDirection = inputDirection.normalized;

        Vector2 startPosition = rb.position;

        dashTarget = ClampToScreen(
            startPosition + dashDirection * dashDistance
        );

        if (IsInsideObstacle(dashTarget))
            return;

        dashRemainingDistance =
            Vector2.Distance(startPosition, dashTarget);

        if (dashRemainingDistance <= 0.01f)
            return;

        isDashing = true;
        dashProtection = true;
        dashProtectionTimer = dashProtectionAfterDash;

        nextDashTime = Time.time + dashCooldown;

        rb.linearVelocity = Vector2.zero;

        // Ignorar las colisiones desde el comienzo.
        IgnoreObstaclesAlongDash();
    }

    private void IgnoreObstaclesAlongDash()
    {
        if (!isDashing || playerCollider == null)
            return;

        Collider2D[] obstacles = Physics2D.OverlapBoxAll(
            rb.position + colliderOffset +
                dashDirection * (dashRemainingDistance * 0.5f),
            new Vector2(
                colliderSize.x +
                    Mathf.Abs(dashDirection.x) * dashRemainingDistance,
                colliderSize.y +
                    Mathf.Abs(dashDirection.y) * dashRemainingDistance
            ),
            0f
        );

        foreach (Collider2D obstacle in obstacles)
        {
            if (obstacle == null ||
                obstacle == playerCollider ||
                !IsInObstacleLayer(obstacle))
                continue;

            IgnoreObstacle(obstacle);
        }
    }

    private void IgnoreObstacle(Collider2D obstacle)
    {
        if (obstacle == null ||
            playerCollider == null ||
            ignoredObstacles.Contains(obstacle))
            return;

        Physics2D.IgnoreCollision(
            playerCollider,
            obstacle,
            true
        );

        ignoredObstacles.Add(obstacle);
    }

    private bool IsInObstacleLayer(Collider2D obstacle)
    {
        return (obstacleLayer.value &
            (1 << obstacle.gameObject.layer)) != 0;
    }

    private void FixedUpdate()
    {
        if (!isDashing || rb == null)
            return;

        // Detectar también obstáculos nuevos o activados durante el Dash.
        IgnoreObstaclesAlongDash();

        float step = Mathf.Min(
            dashSpeed * Time.fixedDeltaTime,
            dashRemainingDistance
        );

        Vector2 nextPosition =
            ClampToScreen(rb.position + dashDirection * step);

        float movedDistance = Vector2.Distance(
            rb.position,
            nextPosition
        );

        rb.MovePosition(nextPosition);
        dashRemainingDistance -= movedDistance;

        if (dashRemainingDistance <= 0.01f)
        {
            rb.position = dashTarget;
            FinishDash();
        }
    }

    private bool IsInsideObstacle(Vector2 position)
    {
        if (obstacleLayer.value == 0 || playerCollider == null)
            return false;

        Collider2D[] obstacles = Physics2D.OverlapBoxAll(
            position + colliderOffset,
            colliderSize,
            0f,
            obstacleLayer
        );

        foreach (Collider2D obstacle in obstacles)
        {
            if (obstacle != null && obstacle != playerCollider)
                return true;
        }

        return false;
    }

    private Vector2 ClampToScreen(Vector2 position)
    {
        if (mainCamera == null || !mainCamera.orthographic)
            return position;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float minX = mainCamera.transform.position.x
            - cameraWidth + halfWidth;

        float maxX = mainCamera.transform.position.x
            + cameraWidth - halfWidth;

        float minY = mainCamera.transform.position.y
            - cameraHeight + halfHeight;

        float maxY = mainCamera.transform.position.y
            + cameraHeight - halfHeight;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void FinishDash()
    {
        isDashing = false;
        dashRemainingDistance = 0f;

        dashProtection = true;
        dashProtectionTimer = dashProtectionAfterDash;

        RestoreObstacleCollisions();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void RestoreObstacleCollisions()
    {
        foreach (Collider2D obstacle in ignoredObstacles)
        {
            if (obstacle != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(
                    playerCollider,
                    obstacle,
                    false
                );
            }
        }

        ignoredObstacles.Clear();
    }

    private void OnDisable()
    {
        isDashing = false;
        dashProtection = false;
        dashProtectionTimer = 0f;

        RestoreObstacleCollisions();
    }
}
