
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class ExpandingCircle : MonoBehaviour
{
    [Header("Expansion")]
    public float initialRadius = 0.2f;
    public float minMaxRadius = 4f;
    public float maxMaxRadius = 12f;
    public float expansionSpeed = 2f;

    [Header("Appearance")]
    public float ringThickness = 0.35f;
    public Color ringColor = Color.red;
    public int segments = 64;

    private CircleCollider2D circleCollider;
    private LineRenderer lineRenderer;

    private float currentRadius;
    private float maxRadius;
    private bool initialized;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        maxRadius = Random.Range(
            minMaxRadius,
            maxMaxRadius
        );

        circleCollider.isTrigger = true;
        circleCollider.radius = initialRadius;

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = ringThickness;
        lineRenderer.endWidth = ringThickness;
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;
        lineRenderer.numCapVertices = 4;

        lineRenderer.material = new Material(
            Shader.Find("Sprites/Default")
        );

        currentRadius = initialRadius;
        initialized = true;

        UpdateVisual();
    }

    private void Update()
    {
        if (!initialized)
            return;

        currentRadius += expansionSpeed * Time.deltaTime;

        if (currentRadius >= maxRadius)
        {
            Destroy(gameObject);
            return;
        }

        circleCollider.radius = currentRadius;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        float visualRadius = Mathf.Max(
            0.01f,
            currentRadius - ringThickness / 2f
        );

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * visualRadius,
                Mathf.Sin(angle) * visualRadius,
                0f
            );

            lineRenderer.SetPosition(i, point);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            health.transform.position
        );

        float innerRadius = Mathf.Max(
            0f,
            currentRadius - ringThickness
        );

        if (distance >= innerRadius &&
            distance <= currentRadius)
        {
            health.TakeDamage();
        }
    }
}
