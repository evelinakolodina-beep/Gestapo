using System;
using System.Collections.Generic; // Добавлено для работы с List
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Boss Health Bars")]
    // Изменено на список, чтобы можно было назначить несколько шкал
    [SerializeField] private List<BossHealthBar> bossHealthBars = new List<BossHealthBar>();
    [SerializeField] private bool isBoss = true;

    private float currentHealth;

    public event Action<float> OnDamaged;
    public event Action OnHalfHealthReached;

    private bool _halfHealthTriggered = false;

    public float CurrentHealthNormalized => currentHealth / maxHealth;

    private void Start()
    {
        currentHealth = maxHealth;

        // Автоматический поиск шкал, если список пуст
        if (isBoss && (bossHealthBars == null || bossHealthBars.Count == 0))
        {
            TryFindBossHealthBars();
        }

        // Показываем шкалу при спавне для всех назначенных/найденных объектов
        if (isBoss && bossHealthBars != null)
        {
            foreach (var bar in bossHealthBars)
            {
                if (bar != null)
                {
                    bar.Show(this);
                }
            }
        }
    }

    private void TryFindBossHealthBars()
    {
        try
        {
            // Ищем среди всех объектов, включая неактивные
            BossHealthBar[] allBars = FindObjectsOfType<BossHealthBar>(includeInactive: true);

            if (allBars.Length > 0)
            {
                bossHealthBars.AddRange(allBars);
                // Debug.Log($"[Enemy] Найдено {bossHealthBars.Count} объектов BossHealthBar");
            }
            else
            {
                // Debug.LogError("[Enemy] Не найден объект с компонентом BossHealthBar в сцене!");
            }
        }
        catch (UnityException e)
        {
            // Debug.LogError($"[Enemy] Ошибка при поиске BossHealthBar: {e.Message}");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        OnDamaged?.Invoke(damage);

        // Проверяем порог здоровья
        if (!_halfHealthTriggered && currentHealth <= maxHealth * 0.5f)
        {
            _halfHealthTriggered = true;
            OnHalfHealthReached?.Invoke();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Скрываем ВСЕ шкалы с анимацией
        if (isBoss && bossHealthBars != null)
        {
            foreach (var bar in bossHealthBars)
            {
                if (bar != null)
                {
                    bar.Hide();
                }
            }
        }

        EventManager.TriggerRoomCleared();

        if (isBoss)
            Destroy(gameObject, 0.5f);
        else
            Destroy(gameObject);
    }
}