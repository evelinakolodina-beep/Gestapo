using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Image fillImage; // Картинка, которая будет заполняться
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Настройки анимации")]
    [SerializeField] private float smoothTime = 0.2f; // Время анимации в секундах
    [SerializeField] private Ease animationEase = Ease.OutQuad; // Тип сглаживания (можно поставить OutBounce для эффекта "пружинки")

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("Не назначен PlayerHealth в HealthBarUI!", this);
            return;
        }

        // Подписываемся на событие изменения здоровья
        playerHealth.OnHealthChanged += UpdateHealthBar;

        // Сразу устанавливаем корректное значение
        UpdateHealthBar(playerHealth.CurrentHealth / playerHealth.MaxHealth);
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся, чтобы избежать ошибок при уничтожении объекта
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float normalizedHealth)
    {
        // Убиваем предыдущую анимацию, если игрок получает удары быстро, чтобы твины не накладывались друг на друга
        fillImage.DOKill();

        // Анимируем заполнение картинки
        fillImage.DOFillAmount(normalizedHealth, smoothTime)
                 .SetEase(animationEase);
    }
}