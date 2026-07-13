using System;
using UnityEngine;

public class ThirdBossManager : MonoBehaviour, IBossAttack
{
    [Header("Компоненты")]
    [SerializeField] private LetterGridArranger gridArranger;
    [SerializeField] private WordController wordController;
    [SerializeField] private LetterCombatManager combatManager;

    [Header("Настройки")]
    [SerializeField] private KeyCode arrangeKey = KeyCode.Space;
    [SerializeField] private KeyCode toggleAttackKey = KeyCode.F;

    private bool isGridArranged = false;
    private bool isAttacking = false;

    private Transform _playerTransform;

    // Событие из интерфейса IBossAttack
    public event Action OnCompleted;

    private void Awake()
    {
        // Автоматический поиск компонентов, если не назначены
        if (gridArranger == null) gridArranger = GetComponentInChildren<LetterGridArranger>();

        if (wordController == null)
        {
            GameObject titleObj = GameObject.FindGameObjectWithTag("Title");
            if (titleObj != null) wordController = titleObj.GetComponent<WordController>();
        }

        if (combatManager == null) combatManager = GetComponentInChildren<LetterCombatManager>();

        if (gridArranger == null)
            Debug.LogError("[ThirdBossManager] LetterGridArranger не найден!");
        if (wordController == null)
            Debug.LogError("[ThirdBossManager] WordController не найден!");
        if (combatManager == null)
            Debug.LogError("[ThirdBossManager] LetterCombatManager не найден!");
    }

   

    #region IBossAttack Implementation

    /// <summary>
    /// Вызывается BossPhaseManager для передачи ссылки на игрока
    /// </summary>
    public void Setup(Transform playerTransform)
    {
        _playerTransform = playerTransform;

        // Если вашему LetterCombatManager нужно знать цель (игрока), передайте её сюда:
        // if (combatManager != null) combatManager.SetTarget(playerTransform);
    }

    /// <summary>
    /// Вызывается BossPhaseManager для начала атаки (с учетом задержки startDelay)
    /// </summary>
    public void Execute()
    {
        Debug.Log("[ThirdBossManager] Execute() - Начало атаки");
        ArrangeGrid();
    }

    /// <summary>
    /// Вызывается BossPhaseManager для остановки атаки (при смене фазы или уничтожении)
    /// </summary>
    public void Cancel()
    {
        Debug.Log("[ThirdBossManager] Cancel() - Остановка атаки");

        if (isAttacking && combatManager != null)
        {
            combatManager.StopContinuousAttack();
            isAttacking = false;
        }

        if (wordController != null)
        {
            wordController.StopMoving();
        }

        isGridArranged = false;

        // Вызываем событие завершения
        OnCompleted?.Invoke();
    }

    #endregion

    private void ArrangeGrid()
    {
        Debug.Log("[ThirdBossManager] Запуск сборки букв в квадрат");

        // Останавливаем движение заголовка
        if (wordController != null)
        {
            wordController.StopMoving();
        }

        // Запускаем анимацию сборки
        if (gridArranger != null)
        {
            gridArranger.ArrangeIntoGrid();
        }
    }

    /// <summary>
    /// Вызывается из LetterGridArranger после завершения анимации
    /// </summary>
    public void OnGridArrangementComplete()
    {
        isGridArranged = true;
        Debug.Log("[ThirdBossManager] Сборка завершена, запускаем движение и атаку");

        // Запускаем движение заголовка
        if (wordController != null)
        {
            wordController.StartMoving();
        }

        // Запускаем постоянную атаку
        if (combatManager != null)
        {
            combatManager.StartContinuousAttack();
            isAttacking = true;
        }
    }

    private void ToggleAttack()
    {
        if (combatManager == null) return;

        if (isAttacking)
        {
            combatManager.StopContinuousAttack();
            isAttacking = false;
            Debug.Log("[ThirdBossManager] Атака выключена");
        }
        else
        {
            combatManager.StartContinuousAttack();
            isAttacking = true;
            Debug.Log("[ThirdBossManager] Атака включена");
        }
    }

    [ContextMenu("Тест: Собрать в квадрат")]
    private void TestArrange()
    {
        ArrangeGrid();
    }

    [ContextMenu("Тест: Переключить атаку")]
    private void TestToggleAttack()
    {
        ToggleAttack();
    }

    [ContextMenu("Тест: Запустить движение заголовка")]
    private void TestStartMoving()
    {
        if (wordController != null)
        {
            wordController.StartMoving();
        }
    }

    [ContextMenu("Тест: Остановить движение заголовка")]
    private void TestStopMoving()
    {
        if (wordController != null)
        {
            wordController.StopMoving();
        }
    }
}