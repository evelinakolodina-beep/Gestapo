using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Стены комнаты")]
    [SerializeField] private GameObject entranceWall;
    [SerializeField] private GameObject exitWall;

    [Header("Босс")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bossPrefab;

    [Header("Следующая локация")]
    [SerializeField] private Transform corridorSpawnPoint;
    private bool isBossSpawned = false;

    // Делаем якорь доступным для чтения из CorridorManager
    public Transform CorridorSpawnPoint => corridorSpawnPoint;

    private void Start()
    {
        if (entranceWall) entranceWall.SetActive(false);
        if (exitWall) exitWall.SetActive(true);
    }

    private void OnEnable()
    {
        EventManager.OnRoomEntered += HandleRoomEntered;
        EventManager.OnRoomCleared += HandleBossDied;

        // Спавним босса сразу при активации комнаты
        SpawnBoss();
    }

    private void OnDisable()
    {
        EventManager.OnRoomEntered -= HandleRoomEntered;
        EventManager.OnRoomCleared -= HandleBossDied;
    }

    private void HandleRoomEntered()
    {
        CloseEntrance();
        // SpawnBoss() убран отсюда — босс уже спавнится при активации комнаты
    }

    private void HandleBossDied()
    {
        OpenExit();
    }

    private void CloseEntrance()
    {
        if (entranceWall) entranceWall.SetActive(true);
    }

    private void SpawnBoss()
    {
        if (isBossSpawned) return; // Если босс уже спавнился, выходим

        if (bossPrefab && bossSpawnPoint)
        {
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossPrefab.transform.rotation);
            InitializeBoss(boss);
            isBossSpawned = true; // Ставим флаг
        }
    }

    private void InitializeBoss(GameObject boss)
    {
        // Твоя логика
    }

    private void OpenExit()
    {
        if (exitWall) exitWall.SetActive(false);
    }

    /// <summary>
    /// Этот метод нужно повесить на Unity Event триггера ВЫХОДА из комнаты.
    /// Когда игрок проходит через открытую дверь, он вызывает это событие.
    /// </summary>
    public void OnExitTriggered()
    {
        OpenExit();
        EventManager.TriggerRoomCleared();
    }
}