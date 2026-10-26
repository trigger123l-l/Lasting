
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Visual")]
    public Transform visual;
    public float stretchAmount = 0.2f;
    public float stretchSpeed = 10f;

    [Header("Movement Particles")]
    public ParticleSystem movementParticles;
    public float particleOffset = 0.35f;
    public float emissionRate = 35f;

    [Header("Screen Boundaries")]
    public float screenMargin = 0.02f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector3 originalScale;

    private Camera mainCamera;
    private float playerHalfWidth;
    private float playerHalfHeight;

    private ParticleSystem.EmissionModule emission;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (visual != null)
        {
            originalScale = visual.localScale;
        }

        // Obtener el tamaño visible del cuadrado.
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

        // Preparar las partículas sin emitir al iniciar.
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
        // Leer el movimiento horizontal y vertical.
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Evitar mayor velocidad al moverse en diagonal.
        movement = movement.normalized;

        UpdateVisual();
        UpdateParticles();
    }

    private void UpdateVisual()
    {
        if (visual == null)
            return;

        Vector3 targetScale = originalScale;

        // Deformar el cuadrado según la dirección.
        if (movement.sqrMagnitude > 0.01f)
        {
            float horizontal = Mathf.Abs(movement.x);
            float vertical = Mathf.Abs(movement.y);

            targetScale.x *=
                1f + stretchAmount * (horizontal - vertical);

            targetScale.y *=
                1f + stretchAmount * (vertical - horizontal);
        }

        // Suavizar la deformación.
        visual.localScale = Vector3.Lerp(
            visual.localScale,
            targetScale,
            stretchSpeed * Time.deltaTime
        );
    }

    private void UpdateParticles()
    {
        if (movementParticles == null)
            return;

        bool isMoving = movement.sqrMagnitude > 0.01f;

        // Detener la emisión cuando el jugador está quieto.
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

        // Situar el emisor en el lado opuesto al movimiento.
        Vector3 oppositeDirection = new Vector3(
            -movement.x,
            -movement.y,
            0f
        );

        movementParticles.transform.localPosition =
            oppositeDirection * particleOffset;

        emission.rateOverTime = emissionRate;

        if (!movementParticles.isPlaying)
        {
            movementParticles.Play();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Aplicar el movimiento.
        rb.linearVelocity = movement * speed;

        if (mainCamera == null || !mainCamera.orthographic)
            return;

        // Calcular el área visible de la cámara.
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float cameraX = mainCamera.transform.position.x;
        float cameraY = mainCamera.transform.position.y;

        // Dejar todo el cuadrado dentro de la pantalla.
        float minX = cameraX - cameraWidth
            + playerHalfWidth + screenMargin;

        float maxX = cameraX + cameraWidth
            - playerHalfWidth - screenMargin;

        float minY = cameraY - cameraHeight
            + playerHalfHeight + screenMargin;

        float maxY = cameraY + cameraHeight
            - playerHalfHeight - screenMargin;

        // Limitar la posición del jugador.
        Vector2 position = rb.position;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        rb.position = position;
    }
}

