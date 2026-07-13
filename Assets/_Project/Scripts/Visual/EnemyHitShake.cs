using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged += OnHit;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= OnHit;
    }

    private void OnHit(float damage)
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }
}