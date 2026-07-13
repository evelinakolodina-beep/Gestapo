using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Урон")]
    [SerializeField] private float attackDamage = 10f;

    [Header("Слои")]
    [SerializeField] private LayerMask damageableLayer;

    private Camera mainCam;
    private bool isAttacking = false;

    private void Start()
    {
        mainCam = Camera.main;
        if (hitbox != null) hitbox.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            isAttacking = true;

            Vector3 clickPoint = ray.GetPoint(distance);
            Vector3 direction = (clickPoint - transform.position).normalized;

            if (direction == Vector3.zero) direction = transform.forward;
            Vector3 targetPos = transform.position + direction * attackRange;

            if (hitbox != null)
            {
                hitbox.transform.position = targetPos;
                hitbox.SetActive(true);
                Invoke(nameof(DisableHitbox), 0.15f);
            }

            Collider[] hitColliders = Physics.OverlapSphere(targetPos, attackRadius, damageableLayer);

            HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
            HashSet<DamageFlinch> flinchedObjects = new HashSet<DamageFlinch>();

            foreach (var col in hitColliders)
            {
                // 1. УРОН
                if (col.TryGetComponent<DamageReceiver>(out var receiver))
                {
                    EnemyHealth health = receiver.GetRootHealth();
                    if (health != null && damagedEnemies.Add(health))
                    {
                        health.TakeDamage(attackDamage);
                    }
                }

                // 2. ВИЗУАЛ (Ищем скрипт прямо на коллайдере, куда мы его теперь вешаем)
                if (col.TryGetComponent<DamageFlinch>(out var flinch))
                {
                    if (flinchedObjects.Add(flinch))
                    {
                        flinch.ReactToHit(transform.position);
                    }
                }
            }
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack() => isAttacking = false;
    private void DisableHitbox() { if (hitbox != null) hitbox.SetActive(false); }
}