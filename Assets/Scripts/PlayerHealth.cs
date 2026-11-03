
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxLives = 5;
    public float invulnerabilityDuration = 0.35f;

    [Header("UI")]
    public TMP_Text livesText;

    [Header("Survival Timer")]
    public SurvivalTimer survivalTimer;

    [Header("Music")]
    public AudioSource musicSource;

    private int currentLives;
    private float invulnerabilityTimer;
    private bool isDead;
    private bool hasWon;

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
        if (isDead || hasWon || invulnerabilityTimer > 0f)
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
            if (hasWon)
                livesText.text = "YOU WIN";
            else if (isDead)
                livesText.text = "GAME OVER";
            else
                livesText.text = "LIVES: " + currentLives;
        }
    }

    private void Die()
    {
        isDead = true;
        UpdateLivesUI();

        if (survivalTimer != null)
        {
            survivalTimer.FinishTimer(false);
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        Time.timeScale = 0f;

        Debug.Log("GAME OVER: el jugador se quedó sin vidas.");
    }

    public void WinGame()
    {
        if (isDead || hasWon)
            return;

        hasWon = true;
        UpdateLivesUI();

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        Time.timeScale = 0f;

        Debug.Log("YOU WIN: el jugador sobrevivió 55:30.");
    }
}
