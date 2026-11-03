
using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Settings")]
    public float survivalGoal = 3330f;

    [Header("UI")]
    public TMP_Text survivalText;

    [Header("References")]
    public PlayerHealth playerHealth;

    private float elapsedTime;
    private bool isFinished;

    public float ElapsedTime => elapsedTime;

    private void Start()
    {
        if (survivalText != null)
        {
            survivalText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isFinished)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= survivalGoal)
        {
            elapsedTime = survivalGoal;
            FinishTimer(true);

            if (playerHealth != null)
            {
                playerHealth.WinGame();
            }
        }
    }

    public void FinishTimer(bool won)
    {
        if (isFinished)
            return;

        isFinished = true;

        if (survivalText != null)
        {
            survivalText.gameObject.SetActive(true);

            survivalText.text = won
                ? "YOU WIN\nSURVIVED: " + FormatTime(elapsedTime)
                : "SURVIVED: " + FormatTime(elapsedTime);
        }

        Debug.Log(
            "Survival Time: " + FormatTime(elapsedTime)
        );
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return minutes.ToString("00") + ":" +
               seconds.ToString("00");
    }
}