using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.TriggerBossRoomEntered();
        EventManager.TriggerRoomEntered();
        Destroy(this);
    }
}