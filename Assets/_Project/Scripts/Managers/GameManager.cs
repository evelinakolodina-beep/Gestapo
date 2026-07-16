using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Настройки Паузы")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Настройки Концовки")]
    [SerializeField] private GameObject endGameCanvas; // <-- Новый канвас окончания игры
    [SerializeField] private VideoPlayer endCinematicVideo;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float endGameDelay = 3f; // Задержка в секундах перед показом концовки

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

        // Убеждаемся, что канвас концовки выключен при старте
        if (endGameCanvas != null)
        {
            endGameCanvas.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        EventManager.OnGameEnded -= HandleGameEnded;

        if (endCinematicVideo != null)
        {
            endCinematicVideo.loopPointReached -= OnEndCinematicFinished;
        }
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
    private void HandleGameEnded()
    {
        Debug.Log("Конец игры. Запуск последовательности завершения.");
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // 1. Снимаем паузу, если она была, чтобы время шло
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }

        // 2. Ждем указанное количество секунд
        yield return new WaitForSeconds(endGameDelay);

        // 4. Запускаем ролик (если он есть)
        if (endCinematicVideo != null)
        {
            endCinematicVideo.Play();
            yield return new WaitForSeconds((float) endCinematicVideo.length);
        }

        // 3. Активируем канвас окончания игры
        if (endGameCanvas != null)
        {
            endGameCanvas.SetActive(true);
        }
    }

    private void OnEndCinematicFinished(VideoPlayer vp)
    {
        //LoadMainMenu();
    }

    // Этот метод теперь можно повесить на кнопку "В главное меню" на вашем endGameCanvas
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    #endregion
}