
using UnityEngine;

public class HazardDamage : MonoBehaviour
{
    private bool hasHitPlayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void TryHitPlayer(Collider2D other)
    {
        if (hasHitPlayer || other == null)
            return;

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        PlayerDash dash =
            health.GetComponent<PlayerDash>();

        // Durante el Dash o la protección posterior,
        // el jugador atraviesa los pinchos sin destruirlos.
        if (dash != null && dash.IsDashProtected)
            return;

        // El jugador ya no está protegido.
        hasHitPlayer = true;

        health.TakeDamage();

        // Los pinchos desaparecen al tocar al jugador
        // sin protección.
        if (CompareTag("Spike"))
        {
            Destroy(gameObject);
        }
    }
}
