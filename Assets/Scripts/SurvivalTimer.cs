
using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Settings")]
    public float survivalGoal = 55f * 60f + 30f;

    [Header("UI")]
    public TMP_Text survivalText;

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
                ? "YOU SURVIVED: " + FormatTime(elapsedTime)
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
