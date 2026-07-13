using System;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("Общие параметры")]
    [SerializeField] private Transform playerTransform;

    [Header("Ссылки на атаки (перетащи сюда)")]
    [SerializeField] private BossDash dashAttack;
    [SerializeField] private BossHelicopterAttack heliAttack;

    [Header("Настройки последовательности")]
    [SerializeField] private bool loopSequence = true;

    private List<IBossAttack> _attacks = new List<IBossAttack>();
    private int _currentIndex = 0;
    private IBossAttack _currentAttack;
    private bool _isAttacking = false;

    private void Awake()
    {
        // 1. Если Transform игрока не назначен в инспекторе, ищем его по тегу
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogError("[BossAttackManager] playerTransform не назначен в инспекторе, и объект с тегом 'P' не найден на сцене!");
                return; // Прерываем выполнение, чтобы избежать ошибок дальше
            }
        }

        // 2. Подтягиваем компоненты атак, если они не назначены вручную
        if (dashAttack == null) dashAttack = GetComponent<BossDash>();
        if (heliAttack == null) heliAttack = GetComponent<BossHelicopterAttack>();

        if (dashAttack != null) _attacks.Add(dashAttack);
        if (heliAttack != null) _attacks.Add(heliAttack);

        // 3. Инициализируем атаки
        foreach (var attack in _attacks)
        {
            attack.Setup(playerTransform);
            attack.OnCompleted += HandleAttackCompleted;
        }

        Debug.Log($"[BossAttackManager] Инициализировано {_attacks.Count} атак(и)");
    }

    private void OnDestroy()
    {
        foreach (var attack in _attacks)
        {
            attack.OnCompleted -= HandleAttackCompleted;
        }
    }

    // ЭТА КНОПКА ПОЯВИТСЯ В ИНСПЕКТОРЕ
    [ContextMenu("Переключить атаку (Тест)")]
    private void TriggerAttackFromInspector()
    {
        if (!_isAttacking)
        {
            StartAttack();
        }
        else
        {
            SwitchToNextAttack();
        }
    }

    private void StartAttack()
    {
        if (_attacks.Count == 0)
        {
            Debug.LogWarning("[BossAttackManager] Нет доступных атак!");
            return;
        }

        _currentAttack = _attacks[_currentIndex];
        _isAttacking = true;
        _currentAttack.Execute();
        Debug.Log($"[BossAttackManager] Запущена атака: {_currentAttack.GetType().Name}");
    }

    private void SwitchToNextAttack()
    {
        _currentAttack?.Cancel();

        if (loopSequence)
        {
            _currentIndex = (_currentIndex + 1) % _attacks.Count;
        }
        else
        {
            _currentIndex++;
        }

        if (_currentIndex < _attacks.Count)
        {
            _currentAttack = _attacks[_currentIndex];
            _currentAttack.Execute();
            Debug.Log($"[BossAttackManager] Переключена на атаку: {_currentAttack.GetType().Name}");
        }
        else
        {
            _isAttacking = false;
            _currentAttack = null;
            _currentIndex = 0;
            Debug.Log("[BossAttackManager] Все атаки выполнены. Ожидание нового запуска.");
        }
    }

    private void HandleAttackCompleted()
    {
        _isAttacking = false;
        _currentAttack = null;
        Debug.Log("[BossAttackManager] Атака завершилась самостоятельно.");
    }
}