using UnityEngine;
using UnityEngine.SceneManagement; // 1. Подключаем работу со сценами
using System.Collections;         // 2. Подключаем для IEnumerator
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Настройки перезапуска")]
    [SerializeField, Tooltip("Задержка в секундах перед перезагрузкой сцены")]
    private float restartDelay = 2f; // 3. Добавляем переменную задержки

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
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб!");
        OnDie?.Invoke();
        
        // 4. Запускаем корутину перезагрузки вместо мгновенного Destroy
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // ПРИМЕЧАНИЕ: Destroy(gameObject) убран отсюда. 
        // Если уничтожить объект сейчас, корутина не успеет выполниться.
        // При перезагрузке сцены объект и так будет уничтожен.
    }

    // 5. Новый метод корутины для ожидания и перезагрузки
   

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth / maxHealth);
    }
}