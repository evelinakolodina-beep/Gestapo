using UnityEngine;

public class CorridorTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.TriggerCorridorAnimation(this);
        gameObject.SetActive(false);
        Debug.Log("триггер прошел");
    }
}