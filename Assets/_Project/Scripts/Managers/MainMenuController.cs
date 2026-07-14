using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Ссылки на Канвасы")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject disclaimerCanvas;
    [SerializeField] private GameObject prefaceCanvas; // <-- Новый канвас с предисловием

    [Header("Настройки загрузки")]
    // ВПИШИ СЮДА ТОЧНОЕ ИМЯ твоей игровой сцены из Build Settings (в кавычках)
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (disclaimerCanvas != null) disclaimerCanvas.SetActive(false);
        if (prefaceCanvas != null) prefaceCanvas.SetActive(false); // Изначально скрыт
    }

    // 1. Кнопка "Начать игру" в главном меню
    public void OnStartGameClicked()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (disclaimerCanvas != null) disclaimerCanvas.SetActive(true);
    }

    // 2. Кнопка принятия дисклеймера (например, "Принять" или "Понятно")
    public void OnDisclaimerAccepted()
    {
        if (disclaimerCanvas != null) disclaimerCanvas.SetActive(false);
        if (prefaceCanvas != null) prefaceCanvas.SetActive(true); // Показываем предисловие
    }

    // 3. Кнопка старта игры из канваса предисловия (например, "Играть" или "Далее")
    public void OnPrefaceAccepted()
    {
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