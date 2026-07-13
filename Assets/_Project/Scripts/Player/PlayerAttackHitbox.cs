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
        // Если мы уже ударили этот объект за текущий взмах - игнорируем
        if (hitEnemies.Contains(other)) return;

        // 1. Проверяем попадание по самому Боссу (использует твой старый код)
        if (other.CompareTag("Boss"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var health))
            {
                hitEnemies.Add(other);
                health.TakeDamage(CurrentDamage);
                return; // Прерываем дальнейшие проверки, так как уже попали
            }
        }

        // 2. Проверяем попадание по маленькому слову-снаряду
        if (other.TryGetComponent<WordProjectile>(out var wordProjectile))
        {
            Debug.Log("в хит боксе прошло");
            hitEnemies.Add(other);
            wordProjectile.TakeDamage(CurrentDamage);
        }
    }
}