using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.TriggerBossRoomEntered();
        EventManager.TriggerRoomEntered();
        Debug.Log("триггер входа");
        Destroy(this);
    }
}