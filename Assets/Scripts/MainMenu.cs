
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject howToPlayPanel;
    public GameObject settingsPanel;

    [Header("Settings")]
    public UnityEngine.UI.Slider volumeSlider;

    private void Start()
    {
        Time.timeScale = 1f;

        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);

        AudioListener.volume = Mathf.Clamp(
            AudioListener.volume, 0.01f, 1f
        );

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0.01f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = AudioListener.volume;
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void OpenHowToPlay()
    {
        howToPlayPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp(volume, 0.01f, 1f);
    }
}
