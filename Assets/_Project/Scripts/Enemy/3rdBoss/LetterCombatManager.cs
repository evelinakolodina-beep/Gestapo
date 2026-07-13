using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LetterCombatManager : MonoBehaviour, IBossAttack
{
    [Header("Обнаружение и атака")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float returnDelay = 0.5f;
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private float delayBetweenAttacks = 0.3f;

    [Header("Предсказание движения")]
    [SerializeField] private float predictionTime = 0.5f;

    [Header("Мульти-атака")]
    [SerializeField] private int minGroupsToAttack = 2;
    [SerializeField] private int maxGroupsToAttack = 3;
    [SerializeField] private float delayBetweenGroups = 0.15f;

    [Header("Автономный режим")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool enableManualTest = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private KeyCode continuousAttackKey = KeyCode.F;

    private List<LetterGroup> groups = new List<LetterGroup>();
    private Coroutine continuousAttackCoroutine;
    private bool isContinuousAttackActive = false;
    private bool isAttacking = false;

    private HashSet<Transform> playersInTrigger = new HashSet<Transform>();

    public event Action OnCompleted;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("[LetterCombatManager] Игрок найден по тегу 'Player'");
            }
        }
    }

    private void Update()
    {
        if (enableManualTest)
        {
            if (Input.GetKeyDown(testKey))
            {
                Debug.Log("[LetterCombatManager] Ручной запуск атаки");
                Execute();
            }

            if (Input.GetKeyDown(continuousAttackKey))
            {
                ToggleContinuousAttack();
            }
        }
    }

    private void ToggleContinuousAttack()
    {
        if (isContinuousAttackActive) StopContinuousAttack();
        else StartContinuousAttack();
    }

    public void StartContinuousAttack()
    {
        if (isContinuousAttackActive) return;

        isContinuousAttackActive = true;
        continuousAttackCoroutine = StartCoroutine(ContinuousAttackLoop());
        Debug.Log("[LetterCombatManager] Постоянная атака ВКЛЮЧЕНА");
    }

    public void StopContinuousAttack()
    {
        if (!isContinuousAttackActive) return;

        isContinuousAttackActive = false;
        if (continuousAttackCoroutine != null)
        {
            StopCoroutine(continuousAttackCoroutine);
            continuousAttackCoroutine = null;
        }
        Debug.Log("[LetterCombatManager] Постоянная атака ВЫКЛЮЧЕНА");
    }

    private IEnumerator ContinuousAttackLoop()
    {
        while (isContinuousAttackActive)
        {
            yield return StartCoroutine(ExecuteAndWait());
            yield return new WaitForSeconds(delayBetweenAttacks);
        }
    }

    private IEnumerator ExecuteAndWait()
    {
        if (playerTransform == null || groups.Count == 0) yield break;

        int count = Random.Range(minGroupsToAttack, maxGroupsToAttack + 1);
        List<LetterGroup> targets = FindNearestCompatibleGroups(count, playerTransform.position);
        if (targets.Count == 0) yield break;

        isAttacking = true;
        List<Coroutine> activeAttacks = new List<Coroutine>();

        foreach (var group in targets)
        {
            Coroutine c = StartCoroutine(AttackSequence(group, playerTransform));
            activeAttacks.Add(c);

            if (delayBetweenGroups > 0f && group != targets[targets.Count - 1])
            {
                yield return new WaitForSeconds(delayBetweenGroups);
            }
        }

        foreach (var c in activeAttacks)
        {
            if (c != null) yield return c;
        }

        isAttacking = false;
        OnCompleted?.Invoke();
    }

    private IEnumerator AttackSequence(LetterGroup group, Transform target)
    {
        yield return StartCoroutine(group.Prepare());

        Vector3 predictedPosition = PredictPlayerPosition(target);

        AudioManager.PlayBossHit(3);

        yield return StartCoroutine(group.Attack(predictedPosition));
    }

    private Vector3 PredictPlayerPosition(Transform target)
    {
        if (target == null) return Vector3.zero;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            return target.position + rb.linearVelocity * predictionTime;
        }
        else
        {
            return target.position;
        }
    }

    [ContextMenu("Тест: Запустить атаку")]
    private void TestAttackFromInspector()
    {
        Execute();
    }

    [ContextMenu("Тест: Вкл/Выкл постоянную атаку")]
    private void TestToggleContinuous()
    {
        ToggleContinuousAttack();
    }

    public void Setup(Transform player)
    {
        this.playerTransform = player;
    }

    public void InitializeGroups(List<Transform> letters, int cols, int rows, Vector2 cellSize, Vector2 spacing)
    {
        foreach (var group in groups)
        {
            if (group != null) Destroy(group.gameObject);
        }
        groups.Clear();

        for (int row = 0; row < rows; row++)
        {
            List<Transform> rowLetters = new List<Transform>();
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                if (index < letters.Count) rowLetters.Add(letters[index]);
            }

            if (rowLetters.Count > 0)
            {
                CreateGroup($"Row_{row}", rowLetters, true, cellSize, spacing, rowLetters.Count);
            }
        }

        for (int col = 0; col < cols; col++)
        {
            List<Transform> colLetters = new List<Transform>();
            for (int row = 0; row < rows; row++)
            {
                int index = row * cols + col;
                if (index < letters.Count) colLetters.Add(letters[index]);
            }

            if (colLetters.Count > 0)
            {
                CreateGroup($"Column_{col}", colLetters, false, cellSize, spacing, colLetters.Count);
            }
        }

        Debug.Log($"[LetterCombatManager] Создано {groups.Count} групп");
    }

    private void CreateGroup(string name, List<Transform> letters, bool isRow, Vector2 cellSize, Vector2 spacing, int count)
    {
        GameObject groupObj = new GameObject(name);
        groupObj.transform.SetParent(this.transform);

        Vector3 center = Vector3.zero;
        foreach (var letter in letters) center += letter.position;
        center /= letters.Count;
        groupObj.transform.position = center;

        BoxCollider collider = groupObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        if (isRow)
        {
            float length = count * cellSize.x + (count - 1) * spacing.x;
            collider.size = new Vector3(length + 2 * detectionRadius, 1f, 2 * detectionRadius);
        }
        else
        {
            float length = count * cellSize.y + (count - 1) * spacing.y;
            collider.size = new Vector3(2 * detectionRadius, 1f, length + 2 * detectionRadius);
        }

        LetterGroup group = groupObj.AddComponent<LetterGroup>();
        group.Initialize(letters, attackDistance, attackDuration, returnDelay, returnDuration, isRow);

        GroupTriggerHandler triggerHandler = groupObj.AddComponent<GroupTriggerHandler>();
        triggerHandler.Initialize(group, this);

        groups.Add(group);
    }

    public void OnPlayerEnterTrigger(Transform player)
    {
        playersInTrigger.Add(player);
        Debug.Log($"[LetterCombatManager] Игрок '{player.name}' вошёл в триггер");

        if (!isContinuousAttackActive && !isAttacking)
        {
            int count = Random.Range(minGroupsToAttack, maxGroupsToAttack + 1);
            List<LetterGroup> targets = FindNearestCompatibleGroups(count, player.position);
            if (targets.Count > 0)
            {
                StartCoroutine(ExecuteMultiAttack(targets, player));
            }
        }
    }

    public void OnPlayerExitTrigger(Transform player)
    {
        playersInTrigger.Remove(player);
        Debug.Log($"[LetterCombatManager] Игрок '{player.name}' вышел из триггера");
    }

    public void Execute()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[LetterCombatManager] Игрок не установлен!");
            return;
        }

        if (groups.Count == 0)
        {
            Debug.LogWarning("[LetterCombatManager] Нет групп!");
            OnCompleted?.Invoke();
            return;
        }

        int count = Random.Range(minGroupsToAttack, maxGroupsToAttack + 1);
        List<LetterGroup> targets = FindNearestCompatibleGroups(count, playerTransform.position);

        if (targets.Count > 0)
        {
            StartCoroutine(ExecuteMultiAttack(targets, playerTransform));
        }
    }

    private IEnumerator ExecuteMultiAttack(List<LetterGroup> targets, Transform targetPlayer)
    {
        isAttacking = true;
        List<Coroutine> activeAttacks = new List<Coroutine>();

        foreach (var group in targets)
        {
            Coroutine c = StartCoroutine(AttackSequence(group, targetPlayer));
            activeAttacks.Add(c);

            if (delayBetweenGroups > 0f && group != targets[targets.Count - 1])
            {
                yield return new WaitForSeconds(delayBetweenGroups);
            }
        }

        foreach (var c in activeAttacks)
        {
            if (c != null) yield return c;
        }

        isAttacking = false;
        OnCompleted?.Invoke();
    }

    private List<LetterGroup> FindNearestCompatibleGroups(int count, Vector3 targetPosition)
    {
        if (groups.Count == 0) return new List<LetterGroup>();

        bool attackRows = Random.value > 0.5f;

        List<LetterGroup> compatibleGroups = new List<LetterGroup>();
        foreach (var g in groups)
        {
            if (g.IsRow == attackRows)
            {
                compatibleGroups.Add(g);
            }
        }

        if (compatibleGroups.Count == 0)
        {
            compatibleGroups = new List<LetterGroup>(groups);
        }

        compatibleGroups.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, targetPosition);
            float distB = Vector3.Distance(b.transform.position, targetPosition);
            return distA.CompareTo(distB);
        });

        int actualCount = Mathf.Min(count, compatibleGroups.Count);
        return compatibleGroups.GetRange(0, actualCount);
    }

    public void Cancel()
    {
        StopContinuousAttack();
        OnCompleted?.Invoke();
    }
}