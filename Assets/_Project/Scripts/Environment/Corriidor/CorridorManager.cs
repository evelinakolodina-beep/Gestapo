using UnityEngine;

public class CorridorManager : MonoBehaviour
{
    [Header("Стартовый коридор (уже на сцене)")]
    [SerializeField] private CorridorSegment startCorridor;

    [Header("Комнаты с боссами (3)")]
    [SerializeField] private Room bossRoom1;
    [SerializeField] private Room bossRoom2;
    [SerializeField] private Room bossRoom3;

    [Header("Комнаты хила (2)")]
    [SerializeField] private Room healRoom1;
    [SerializeField] private Room healRoom2;

    [Header("Префаб коридора")]
    [SerializeField] private CorridorSegment corridorPrefab;

    private Room[] levelSequence;
    private int currentIndex = 0;
    private Room previousRoom;

    private void Awake()
    {
        // Формируем последовательность
        levelSequence = new Room[] { bossRoom1, healRoom1, bossRoom2, healRoom2, bossRoom3 };

        // "Заряжаем" стартовый коридор первой комнатой
        if (startCorridor != null && bossRoom1 != null)
        {
            startCorridor.AssignNextRoom(bossRoom1.gameObject);
        }

        previousRoom = levelSequence[0];
    }

    private void OnEnable()
    {
        EventManager.OnRoomCleared += HandleRoomCleared;
    }

    private void OnDisable()
    {
        EventManager.OnRoomCleared -= HandleRoomCleared;
    }

    private void HandleRoomCleared()
    {
        if (currentIndex >= levelSequence.Length - 1)
        {
            Debug.Log("Все комнаты пройдены!");
            return;
        }

        if (previousRoom != null && previousRoom.CorridorSpawnPoint != null && corridorPrefab != null)
        {
            CorridorSegment newCorridor = Instantiate(
                corridorPrefab,
                previousRoom.CorridorSpawnPoint.position,
                Quaternion.identity
            );

            int nextIndex = currentIndex + 1;
            Room nextRoom = levelSequence[nextIndex];
            newCorridor.AssignNextRoom(nextRoom.gameObject);

            newCorridor.Animate();
        }

        currentIndex++;
        previousRoom = levelSequence[currentIndex];
    }
}