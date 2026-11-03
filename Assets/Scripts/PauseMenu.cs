
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public AudioSource musicSource;

    private bool isPaused;

    private void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
