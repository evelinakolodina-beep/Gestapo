using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Ссылки на Канвасы")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject disclaimerCanvas;

    [Header("Настройки загрузки")]
    // ВПИШИ СЮДА ТОЧНОЕ ИМЯ твоей игровой сцены из Build Settings (в кавычках)
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (disclaimerCanvas != null) disclaimerCanvas.SetActive(false);
    }

    public void OnStartGameClicked()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (disclaimerCanvas != null) disclaimerCanvas.SetActive(true);
    }

    public void OnDisclaimerAccepted()
    {
        // Загружаем сцену по имени (это надёжнее, чем по индексу 0)
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnExitGameClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}