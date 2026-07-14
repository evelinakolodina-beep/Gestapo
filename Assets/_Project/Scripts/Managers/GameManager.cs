using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Настройки Паузы")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Настройки Концовки")]
    [SerializeField] private VideoPlayer endCinematicVideo;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float endGameDelay = 3f; // Задержка в секундах перед показом ролика

    private bool isPaused = false;

    private void Start()
    {
        // Подписываемся на событие завершения игры из EventManager
        EventManager.OnGameEnded += HandleGameEnded;

        // Подписываемся на событие завершения видео
        if (endCinematicVideo != null)
        {
            endCinematicVideo.loopPointReached += OnEndCinematicFinished;
        }
    }

    // Обязательно отписываемся при уничтожении объекта, чтобы избежать утечек памяти
    private void OnDestroy()
    {
        EventManager.OnGameEnded -= HandleGameEnded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region Пауза и Управление Временем
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(isPaused);
        }
    }
    #endregion

    #region Перезапуск и Выход
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Завершение игры и Ролик
    // Этот метод вызывается, когда срабатывает EventManager.OnGameEnded
    private void HandleGameEnded()
    {
        StartCoroutine(EndGameSequence());
    }

    // Корутина для задержки и последовательного запуска
    private IEnumerator EndGameSequence()
    {
        // 1. Снимаем паузу, если она была, чтобы время шло
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        // 2. Ждем указанное количество секунд
        yield return new WaitForSeconds(endGameDelay);

        // 3. Запускаем ролик
        if (endCinematicVideo != null)
        {
            endCinematicVideo.Play();
        }
        else
        {
            Debug.LogWarning("VideoPlayer для концовки не назначен в GameManager!");
            LoadMainMenu();
        }
    }

    private void OnEndCinematicFinished(VideoPlayer vp)
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    #endregion
}