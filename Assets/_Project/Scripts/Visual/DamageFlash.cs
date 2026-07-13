using System.Collections;
using UnityEngine;

// Убрали RequireComponent, так как теперь скрипт универсальный
public class DamageFlash : MonoBehaviour
{
    [Header("Настройки вспышки")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Ссылки")]
    [SerializeField, Tooltip("Если пусто - возьмет Renderer с этого же объекта")]
    private Renderer meshRenderer;

    private Color originalColor;

    // Ссылки на возможные варианты здоровья
    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        // Если рендерер не назначен руками в инспекторе - ищем сами
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();

        if (meshRenderer != null)
        {
            // Запоминаем изначальный цвет материала
            originalColor = meshRenderer.material.color;
        }

        // Пытаемся найти здоровье (ищем на себе или на родителях)
        playerHealth = GetComponentInParent<PlayerHealth>();
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    private void OnEnable()
    {
        // Подписываемся на то здоровье, которое смогли найти
        if (playerHealth != null) playerHealth.OnDamaged += Flash;
        if (enemyHealth != null) enemyHealth.OnDamaged += Flash;
    }

    private void OnDisable()
    {
        // Отписываемся, чтобы избежать утечек памяти
        if (playerHealth != null) playerHealth.OnDamaged -= Flash;
        if (enemyHealth != null) enemyHealth.OnDamaged -= Flash;
    }

    // Метод принимает float, так как событие OnDamaged передает количество урона
    private void Flash(float damage)
    {
        if (meshRenderer == null) return;

        // Обязательно останавливаем предыдущую корутину, если бьют очень быстро!
        // Иначе старая корутина вернет оригинальный цвет посреди новой вспышки.
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        meshRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        meshRenderer.material.color = originalColor;
    }
}