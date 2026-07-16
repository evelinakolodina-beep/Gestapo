using UnityEngine;

public class EndGameOnDestroy : MonoBehaviour // Рекомендуется переименовать класс и файл
{
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        // Получаем компонент здоровья при инициализации
        enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogWarning($"[EndGameOnDeath] Компонент EnemyHealth не найден на объекте {gameObject.name}!", this);
        }
    }

    private void OnEnable()
    {
        // Подписываемся на событие смерти, когда объект активен
        if (enemyHealth != null)
        {
            enemyHealth.OnDied += HandleEnemyDied;
        }
    }

    private void OnDisable()
    {
        // Обязательно отписываемся, чтобы избежать утечек памяти и ошибок
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= HandleEnemyDied;
        }
    }

    private void HandleEnemyDied()
    {
        // Этот метод вызовется ТОЛЬКО когда здоровье упадет до 0 и сработает метод Die()
        EventManager.TriggerGameEnded();
    }
}