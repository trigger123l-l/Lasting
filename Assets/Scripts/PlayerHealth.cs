
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxLives = 4;
    public float invulnerabilityDuration = 0.35f;

    [Header("UI")]
    public TMP_Text livesText;

    private int currentLives;
    private float invulnerabilityTimer;
    private bool isDead;

    public bool IsDead => isDead;

    private void Start()
    {
        currentLives = maxLives;
        UpdateLivesUI();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    public bool TakeDamage()
    {
        if (isDead || invulnerabilityTimer > 0f)
            return false;

        PlayerDash dash = GetComponentInParent<PlayerDash>();

        if (dash != null && dash.IsDashProtected)
            return false;

        currentLives--;
        invulnerabilityTimer = invulnerabilityDuration;

        UpdateLivesUI();

        if (currentLives <= 0)
        {
            Die();
        }

        return true;
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = isDead
                ? "GAME OVER"
                : "LIVES: " + currentLives;
        }
    }

    private void Die()
    {
        isDead = true;
        UpdateLivesUI();

        Time.timeScale = 0f;

        Debug.Log("GAME OVER: el jugador se quedó sin vidas.");
    }
}
