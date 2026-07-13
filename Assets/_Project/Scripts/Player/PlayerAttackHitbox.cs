using System.Collections.Generic;
using UnityEngine;


public class PlayerAttackHitbox : MonoBehaviour
{
    private HashSet<Collider> hitEnemies = new HashSet<Collider>();
    public float CurrentDamage { get; set; }

    private void OnEnable()
    {
        hitEnemies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss") && !hitEnemies.Contains(other))
        {
            hitEnemies.Add(other);

            // Берём здоровье врага и вызываем напрямую
            if (other.TryGetComponent<EnemyHealth>(out var health))
            {
                health.TakeDamage(CurrentDamage);
            }
        }
    }
}
