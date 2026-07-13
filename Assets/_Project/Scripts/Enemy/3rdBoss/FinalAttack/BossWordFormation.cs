using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWordFormation : MonoBehaviour, IBossAttack
{
    [Header("Объекты для старта")]
    [Tooltip("Заголовок или объект, который нужно уничтожить в начале фазы")]
    [SerializeField] private GameObject headerToDestroy;

    [Header("Ссылки на объекты")]
    [SerializeField] private Transform initialLettersParent;
    [SerializeField] private List<GameObject> selectedLetters;
    private Transform player;

    [Header("Настройки слова")]
    [Tooltip("Если оставить пустым, скрипт найдет объект с тегом 'final_anchor'")]
    [SerializeField] private Transform wordAnchor;
    [SerializeField] private Transform wordContainer;
    [SerializeField] private float letterSpacing = 2f;
    [SerializeField] private float formationSpeed = 10f;
    [SerializeField] private float targetScaleMultiplier = 3f;
    [SerializeField] private float scalingDuration = 1.5f;

    [Header("Настройки мусора")]
    [SerializeField] private float trashFlySpeed = 15f;
    [SerializeField] private GameObject trashExplosionPrefab;

    [Header("Настройки стрельбы (Пул)")]
    [SerializeField] private WordProjectile smallWordPrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private float fireRate = 1.5f;

    [Header("Связь со здоровьем босса")]
    [SerializeField] private EnemyHealth mainBossHealth;
    [SerializeField] private float damageToBossPerWord = 10f;

    private Queue<WordProjectile> projectilePool = new Queue<WordProjectile>();
    private bool isActivated = false;

    public event Action OnCompleted;

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
            CreateNewProjectileForPool();
    }

    private WordProjectile CreateNewProjectileForPool()
    {
        WordProjectile proj = Instantiate(smallWordPrefab, transform.position, Quaternion.identity);
        proj.gameObject.SetActive(false);
        projectilePool.Enqueue(proj);
        return proj;
    }

    private WordProjectile GetProjectile()
    {
        if (projectilePool.Count > 0)
            return projectilePool.Dequeue();

        return CreateNewProjectileForPool();
    }

    private void ReturnProjectileToPool(WordProjectile proj)
    {
        projectilePool.Enqueue(proj);
    }

    // ==========================================
    // РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА IBossAttack
    // ==========================================

    public void Setup(Transform playerTransform)
    {
        player = playerTransform;

        // --- ДОБАВЛЕН АВТОПОИСК ЯКОРЯ ---
        if (wordAnchor == null)
        {
            GameObject anchorObj = GameObject.FindGameObjectWithTag("final_anchor");
            if (anchorObj != null)
            {
                wordAnchor = anchorObj.transform;
            }
            else
            {
                Debug.LogWarning("[BossWordFormation] Якорь не назначен вручную, и объект с тегом 'final_anchor' не найден на сцене!");
            }
        }
    }

    public void Execute()
    {
        if (isActivated) return;
        isActivated = true;

        if (headerToDestroy != null)
        {
            if (trashExplosionPrefab != null)
            {
                GameObject particles = Instantiate(trashExplosionPrefab, headerToDestroy.transform.position, Quaternion.identity);
                Destroy(particles, 2f);
            }
            Destroy(headerToDestroy);
        }

        ProcessTrashLetters();
        StartCoroutine(FormWordAndScaleRoutine());
    }

    public void Cancel()
    {
        isActivated = false;
        StopAllCoroutines();
        OnCompleted?.Invoke();
    }

    // ==========================================
    // ЛОГИКА ФАЗЫ
    // ==========================================

    private void ProcessTrashLetters()
    {
        List<GameObject> allObjects = new List<GameObject>();
        foreach (Transform child in initialLettersParent)
            allObjects.Add(child.gameObject);

        foreach (var obj in allObjects)
        {
            if (obj == null) continue;

            if (!selectedLetters.Contains(obj))
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

                if (sr == null)
                    Destroy(obj);
                else
                {
                    TrashLetter trashScript = obj.AddComponent<TrashLetter>();
                    trashScript.Initialize(trashFlySpeed, trashExplosionPrefab);
                    obj.transform.SetParent(null);
                }
            }
        }
    }

    private IEnumerator FormWordAndScaleRoutine()
    {
        // Защита: если якоря всё еще нет, выходим, чтобы не сломать игру
        if (wordAnchor == null)
        {
            Debug.LogError("[BossWordFormation] Невозможно построить слово: нет якоря!");
            yield break;
        }

        int n = selectedLetters.Count;
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < n; i++)
        {
            GameObject letter = selectedLetters[i];
            if (letter == null) continue;

            letter.transform.SetParent(wordContainer);
            float offset = (i - (n - 1) / 2f) * letterSpacing;

            Vector3 targetPos = wordAnchor.position + (wordAnchor.right * offset);
            targetPos.y = wordAnchor.position.y;
            targetPositions.Add(targetPos);
        }

        bool allInPosition = false;
        while (!allInPosition)
        {
            allInPosition = true;
            for (int i = 0; i < n; i++)
            {
                GameObject letter = selectedLetters[i];
                if (letter == null) continue;

                letter.transform.position = Vector3.MoveTowards(
                    letter.transform.position, targetPositions[i], formationSpeed * Time.deltaTime);

                if (Vector3.Distance(letter.transform.position, targetPositions[i]) > 0.01f)
                    allInPosition = false;
            }
            yield return null;
        }

        float timer = 0f;
        Vector3[] initialScales = new Vector3[n];

        for (int i = 0; i < n; i++)
            if (selectedLetters[i] != null) initialScales[i] = selectedLetters[i].transform.localScale;

        while (timer < scalingDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scalingDuration;

            for (int i = 0; i < n; i++)
            {
                GameObject letter = selectedLetters[i];
                if (letter != null)
                {
                    Vector3 targetScale = initialScales[i] * targetScaleMultiplier;
                    letter.transform.localScale = Vector3.Lerp(initialScales[i], targetScale, progress);
                }
            }
            yield return null;
        }

        StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            if (player == null) yield break;

            foreach (var letter in selectedLetters)
            {
                if (letter == null) continue;

                WordProjectile proj = GetProjectile();
                Vector3 directionToPlayer = player.position - letter.transform.position;

                proj.Initialize(letter.transform.position, directionToPlayer, ReturnProjectileToPool, OnWordDestroyedByPlayer);
            }

            yield return new WaitForSeconds(fireRate);
        }
    }

    private void OnWordDestroyedByPlayer()
    {
        if (mainBossHealth != null)
        {
            mainBossHealth.TakeDamage(damageToBossPerWord);
        }
    }
}