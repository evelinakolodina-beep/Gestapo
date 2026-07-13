using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Эффекты")]
    [SerializeField, Tooltip("Перетащите сюда скрипт PlayerFlinch")]
    private PlayerFlinch flinchEffect;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;

    // Событие для обновления полоски ХП
    public event Action<float> OnHealthChanged;

    // ВЕРНУЛ: Событие для вспышки DamageFlash (передает количество урона)
    public event Action<float> OnDamaged;

    public event Action OnDie;

    private void Start()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
    }

    public void TakeDamage(float damage, Vector3 attackerPosition = default)
    {
        if (damage <= 0) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        // 1. Обновляем UI
        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);

        // 2. ЗАПУСКАЕМ ВСПЫШКУ (через событие, которое слушает DamageFlash)
        OnDamaged?.Invoke(damage);

        // 3. ЗАПУСКАЕМ ОТСКОК (напрямую по ссылке в инспекторе)
        if (flinchEffect != null)
        {
            flinchEffect.PlayFlinch(attackerPosition);
        }

        if (CurrentHealth <= 0)
        {
            //Die();
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб!");
        OnDie?.Invoke();
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
    }
}