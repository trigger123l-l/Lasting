
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDistance = 2.5f;
    public float dashCooldown = 0.1f;

    [Header("References")]
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public LayerMask obstacleLayer;

    private Vector2 dashDirection;
    private Vector2 dashTarget;

    private float dashRemainingDistance;
    private float nextDashTime;
    private bool isDashing;

    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;

    private Vector2 colliderSize;
    private Vector2 colliderOffset;

    private readonly List<Collider2D> ignoredObstacles =
        new List<Collider2D>();

    public bool IsDashing => isDashing;
    public Vector2 DashDirection => dashDirection;

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

        Vector2 inputDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryStartDash(inputDirection);
        }
    }

    private void TryStartDash(Vector2 inputDirection)
    {
        if (isDashing || rb == null || playerCollider == null)
            return;

        // No permitir el dash si no se está pulsando una dirección.
        if (inputDirection.sqrMagnitude < 0.01f)
            return;

        if (Time.time < nextDashTime)
            return;

        if (IsInsideObstacle(rb.position))
            return;

        // Utilizar únicamente la dirección actual.
        dashDirection = inputDirection.normalized;

        Vector2 startPosition = rb.position;

        dashTarget = ClampToScreen(
            startPosition + dashDirection * dashDistance
        );

        // No comenzar si el destino está ocupado.
        if (IsInsideObstacle(dashTarget))
            return;

        dashRemainingDistance =
            Vector2.Distance(startPosition, dashTarget);

        if (dashRemainingDistance <= 0.01f)
            return;

        // Ignorar temporalmente los obstáculos atravesados.
        if (obstacleLayer.value != 0)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                startPosition + colliderOffset,
                colliderSize,
                0f,
                dashDirection,
                dashRemainingDistance,
                obstacleLayer
            );

            foreach (RaycastHit2D hit in hits)
            {
                Collider2D obstacle = hit.collider;

                if (obstacle == null || obstacle == playerCollider)
                    continue;

                Physics2D.IgnoreCollision(
                    playerCollider,
                    obstacle,
                    true
                );

                if (!ignoredObstacles.Contains(obstacle))
                    ignoredObstacles.Add(obstacle);
            }
        }

        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!isDashing || rb == null)
            return;

        float step = Mathf.Min(
            dashSpeed * Time.fixedDeltaTime,
            dashRemainingDistance
        );

        Vector2 nextPosition =
            rb.position + dashDirection * step;

        nextPosition = ClampToScreen(nextPosition);

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

        Collider2D obstacle = Physics2D.OverlapBox(
            position + colliderOffset,
            colliderSize,
            0f,
            obstacleLayer
        );

        return obstacle != null && obstacle != playerCollider;
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

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        isDashing = false;

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
}
