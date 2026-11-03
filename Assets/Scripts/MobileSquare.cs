
using UnityEngine;

public class MobileSquare : MonoBehaviour
{
    [Header("Size")]
    public float minSize = 3f;
    public float maxSize = 10f;

    [Header("Movement")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float directionChangeInterval = 3f;

    [Header("Bounces")]
    public int minBounces = 20;
    public int maxBounces = 50;

    private Camera mainCamera;
    private BoxCollider2D squareCollider;

    private Vector2 movementDirection;
    private float movementSpeed;
    private float directionTimer;

    private int remainingBounces;
    private bool initialized;
    private bool hasDamagedPlayer;

    private void Start()
    {
        mainCamera = Camera.main;
        squareCollider = GetComponent<BoxCollider2D>();

        float size = Random.Range(minSize, maxSize);

        transform.localScale = new Vector3(
            size, size, 1f
        );

        movementSpeed = Random.Range(minSpeed, maxSpeed);

        remainingBounces = Random.Range(
            minBounces, maxBounces + 1
        );

        ChangeDirection();

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || mainCamera == null)
            return;

        MoveSquare();
        CheckScreenBounds();
        ChangeDirectionOverTime();
    }

    private void MoveSquare()
    {
        transform.position +=
            (Vector3)(movementDirection *
            movementSpeed * Time.deltaTime);
    }

    private void CheckScreenBounds()
    {
        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z
        );

        Vector3 bottomLeft =
            mainCamera.ViewportToWorldPoint(
                new Vector3(0, 0, cameraDistance)
            );

        Vector3 topRight =
            mainCamera.ViewportToWorldPoint(
                new Vector3(1, 1, cameraDistance)
            );

        Vector3 position = transform.position;

        float halfSize = transform.localScale.x / 2f;

        bool bounced = false;

        if (position.x - halfSize < bottomLeft.x)
        {
            position.x = bottomLeft.x + halfSize;
            movementDirection.x = Mathf.Abs(movementDirection.x);
            bounced = true;
        }
        else if (position.x + halfSize > topRight.x)
        {
            position.x = topRight.x - halfSize;
            movementDirection.x = -Mathf.Abs(movementDirection.x);
            bounced = true;
        }

        if (position.y - halfSize < bottomLeft.y)
        {
            position.y = bottomLeft.y + halfSize;
            movementDirection.y = Mathf.Abs(movementDirection.y);
            bounced = true;
        }
        else if (position.y + halfSize > topRight.y)
        {
            position.y = topRight.y - halfSize;
            movementDirection.y = -Mathf.Abs(movementDirection.y);
            bounced = true;
        }

        transform.position = position;

        if (bounced)
        {
            remainingBounces--;

            if (remainingBounces <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ChangeDirectionOverTime()
    {
        directionTimer += Time.deltaTime;

        if (directionTimer >= directionChangeInterval)
        {
            directionTimer = 0f;
            ChangeDirection();
        }
    }

    private void ChangeDirection()
    {
        float angle = Random.Range(0f, 360f);

        movementDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && health.TakeDamage())
        {
            hasDamagedPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasDamagedPlayer = false;
        }
    }
}
