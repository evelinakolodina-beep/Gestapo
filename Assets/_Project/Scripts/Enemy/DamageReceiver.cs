using UnityEngine;


public class DamageReceiver : MonoBehaviour
{
    private EnemyHealth rootHealth;

    private void Awake()
    {
        rootHealth = GetComponentInParent<EnemyHealth>();
        if (rootHealth == null)
        {
            Debug.LogError($"[DamageReceiver] Не найден EnemyHealth для {gameObject.name}!", this);
        }
    }

    public EnemyHealth GetRootHealth() => rootHealth;

    
}