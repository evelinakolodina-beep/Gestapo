using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Boss Health Bars")]
    [SerializeField] private List<BossHealthBar> bossHealthBars = new List<BossHealthBar>();
    [SerializeField] private bool isBoss = true;

    private float currentHealth;

    public event Action<float> OnDamaged;
    public event Action OnHalfHealthReached;
    public event Action OnDied; // <-- ДОБАВЛЕНО: Событие смерти

    private bool _halfHealthTriggered = false;

    public float CurrentHealthNormalized => currentHealth / maxHealth;

    private void Start()
    {
        currentHealth = maxHealth;

        if (isBoss && (bossHealthBars == null || bossHealthBars.Count == 0))
        {
            TryFindBossHealthBars();
        }

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
            BossHealthBar[] allBars = FindObjectsOfType<BossHealthBar>(includeInactive: true);
            if (allBars.Length > 0)
            {
                bossHealthBars.AddRange(allBars);
            }
        }
        catch (UnityException e)
        {
            Debug.LogError($"[Enemy] Ошибка при поиске BossHealthBar: {e.Message}");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnDamaged?.Invoke(damage);

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
        // 1. Сначала вызываем событие смерти, чтобы другие скрипты могли отреагировать
        OnDied?.Invoke(); // <-- ДОБАВЛЕНО

        // 2. Скрываем ВСЕ шкалы с анимацией
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