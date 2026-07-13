using System.Collections;
using UnityEngine;

public class BossPhaseManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Атаки (перетащи сюда любой скрипт атаки)")]
    [Tooltip("Первая атака (работает до половины здоровья)")]
    [SerializeField] private MonoBehaviour firstAttack;

    [Tooltip("Вторая атака (включается после половины здоровья)")]
    [SerializeField] private MonoBehaviour secondAttack;

    [Header("Настройки")]
    [SerializeField] private float startDelay = 2f;

    private IBossAttack _currentAttack;
    private bool _isFirstPhase = true;
    private bool _hasStarted = false; // Флаг, чтобы не запустить атаки дважды

    private void Awake()
    {
        // Автопоиск игрока
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogError("[BossPhaseManager] Игрок не найден!");
            }
        }

        // Инициализация первой атаки
        if (firstAttack != null && firstAttack is IBossAttack attack1)
        {
            attack1.Setup(playerTransform);
        }
        else if (firstAttack != null)
        {
            Debug.LogError($"[BossPhaseManager] {firstAttack.GetType().Name} не реализует IBossAttack!");
        }

        // Инициализация второй атаки
        if (secondAttack != null && secondAttack is IBossAttack attack2)
        {
            attack2.Setup(playerTransform);
        }
        else if (secondAttack != null)
        {
            Debug.LogError($"[BossPhaseManager] {secondAttack.GetType().Name} не реализует IBossAttack!");
        }

        // Подписка на событие здоровья
        if (enemyHealth != null)
        {
            enemyHealth.OnHalfHealthReached += OnHalfHealthReached;
        }
    }

    private void OnEnable()
    {
        // Подписываемся на вход в комнату
        EventManager.OnRoomEntered += HandleRoomEntered;
    }

    private void OnDisable()
    {
        // Отписываемся от события
        EventManager.OnRoomEntered -= HandleRoomEntered;
    }

    private void HandleRoomEntered()
    {
        if (_hasStarted) return; // Если уже запустились, не реагируем повторно

        _hasStarted = true;
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);

        if (firstAttack != null && firstAttack is IBossAttack attack)
        {
            _currentAttack = attack;
            _currentAttack.Execute();
            //Debug.Log($"[BossPhaseManager] Запущена первая фаза: {firstAttack.GetType().Name}");
        }
    }

    private void OnHalfHealthReached()
    {
        if (!_isFirstPhase)
        {
            return;
        }

        _isFirstPhase = false;
        //Debug.Log("[BossPhaseManager] Получено событие о половине здоровья, переключаем атаку");

        if (_currentAttack != null)
        {
            _currentAttack.Cancel();
        }

        if (secondAttack != null && secondAttack is IBossAttack attack)
        {
            _currentAttack = attack;
            _currentAttack.Execute();
           // Debug.Log($"[BossPhaseManager] Переключена на вторую фазу: {secondAttack.GetType().Name}");
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHalfHealthReached -= OnHalfHealthReached;
        }

        if (_currentAttack != null)
        {
            _currentAttack.Cancel();
        }
    }

    // Для отладки
    [ContextMenu("Принудительно переключить на вторую фазу")]
    private void ForceSecondPhase()
    {
        OnHalfHealthReached();
    }

    [ContextMenu("Остановить все атаки")]
    private void StopAll()
    {
        if (_currentAttack != null)
        {
            _currentAttack.Cancel();
            _currentAttack = null;
        }
    }
}