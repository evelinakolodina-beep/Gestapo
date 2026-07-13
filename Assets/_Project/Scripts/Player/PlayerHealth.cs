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

    public event Action<float> OnHealthChanged;
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

        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
        OnDamaged?.Invoke(damage);

        AudioManager.PlayPlayerDamage();

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