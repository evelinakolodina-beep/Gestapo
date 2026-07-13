using UnityEngine;
using System.Collections; // Добавлено для корутин

public class CorridorSegment : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private CorridorTrigger trigger;
    [SerializeField] private CorridorAnimator animator;

    [Header("Что включить")]
    [SerializeField] private GameObject restCorridor;
    [SerializeField] private Transform endAnchor;

    [Header("Комната (назначается динамически)")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Vector3 roomOffset = Vector3.zero;

    private bool hasAnimated;

    private void OnEnable()
    {
        EventManager.OnCorridorAnimation += HandleTrigger;
        // Подписываемся на событие входа в комнату
        EventManager.OnRoomEntered += HideCorridor;
    }

    private void OnDisable()
    {
        EventManager.OnCorridorAnimation -= HandleTrigger;
        // Отписываемся от события
        EventManager.OnRoomEntered -= HideCorridor;
    }

    private void Start()
    {
        //if (roomPrefab != null) roomPrefab.SetActive(false);
    }

    /// <summary>
    /// Метод для CorridorManager, чтобы подставить нужную комнату в этот коридор.
    /// Заодно подготавливает коридор к использованию: сбрасывает анимацию и включает триггер.
    /// </summary>
    public void AssignNextRoom(GameObject roomObj)
    {
        roomPrefab = roomObj;
        if (roomPrefab != null) roomPrefab.SetActive(false);

        // Сбрасываем флаг, чтобы анимация могла проиграться заново (если коридор переиспользуется)
        hasAnimated = false;

        // Сбрасываем аниматор в начальное состояние (высота возвращается к изначальной)
        if (animator != null) animator.ResetState();

        // Включаем триггер, чтобы игрок мог его активировать
        if (trigger != null) trigger.gameObject.SetActive(true);

        // Возвращаем видимость самому сегменту (на случай, если он был скрыт при входе в комнату)
        gameObject.SetActive(true);
    }

    private void HandleTrigger(CorridorTrigger t)
    {
        if (t != trigger) return;

        Animate();
    }

    /// <summary>
    /// Скрывает обе части коридора при входе в комнату. (ИЗМЕНЕНО НА ПЛАВНОЕ)
    /// </summary>
    private void HideCorridor()
    {
        // Если объект активен, запускаем плавное исчезновение
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(HideRoutine());
        }
    }

    // НОВАЯ КОРУТИНА
    private IEnumerator HideRoutine()
    {
        float duration = 0.4f; // Длительность исчезновения
        float elapsed = 0f;

        // Запоминаем текущие масштабы, чтобы вернуть их потом
        Vector3 startScale = transform.localScale;
        Vector3 restStartScale = restCorridor != null ? restCorridor.transform.localScale : Vector3.one;

        // Плавно уменьшаем масштаб до 0
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Если restCorridor это отдельный объект, уменьшаем и его
            if (restCorridor != null)
            {
                restCorridor.transform.localScale = Vector3.Lerp(restStartScale, Vector3.zero, t);
            }

            yield return null;
        }

        // --- Твоя оригинальная логика скрытия ---
        if (restCorridor != null)
        {
            restCorridor.SetActive(false);
        }
        gameObject.SetActive(false);

        // ВАЖНО: Возвращаем масштабы в норму ПОСЛЕ выключения.
        // Когда коридор снова включат, он будет нормального размера.
        transform.localScale = startScale;
        if (restCorridor != null)
        {
            restCorridor.transform.localScale = restStartScale;
        }
    }

    public void Animate()
    {
        if (hasAnimated) return;
        hasAnimated = true;

        if (animator != null) animator.StartGrowth();
        if (restCorridor != null) restCorridor.SetActive(true);

        if (roomPrefab != null && endAnchor != null)
        {
            roomPrefab.transform.position = endAnchor.position + roomOffset;
            roomPrefab.SetActive(true);
        }
    }
}