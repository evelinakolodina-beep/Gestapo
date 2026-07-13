using UnityEngine;

public class RoomExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Сообщаем системе, что комната пройдена
        EventManager.TriggerRoomCleared();

        // Отключаем триггер, чтобы игрок не вызвал его повторно
        gameObject.SetActive(false);
    }
}