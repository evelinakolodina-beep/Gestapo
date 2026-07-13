using System;
using UnityEngine;

public class WordProjectile : MonoBehaviour
{
    [Header("Настройки полета")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float xRotationOffset = 90f;

    [Header("Здоровье снаряда")]
    [SerializeField] private float maxHealth = 10f; // Сколько урона нужно нанести слову
    private float currentHealth;

    private Vector3 moveDirection;
    private Action<WordProjectile> returnToPool;
    private Action onKilledByPlayer;

    private float lifeTimer;
    private bool isDead = false;

    public void Initialize(Vector3 startPos, Vector3 direction, Action<WordProjectile> poolCallback, Action onKilledCallback)
    {
        transform.position = startPos;
        moveDirection = new Vector3(direction.x, 0f, direction.z).normalized;

        if (moveDirection != Vector3.zero)
        {
            Quaternion baseLookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = baseLookRotation * Quaternion.Euler(xRotationOffset, 0f, 0f);
        }

        returnToPool = poolCallback;
        onKilledByPlayer = onKilledCallback;

        // Сброс параметров при доставании из пула
        lifeTimer = lifetime;
        currentHealth = maxHealth;
        isDead = false;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isDead) return;

        transform.position += moveDirection * speed * Time.deltaTime;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            ExplodeAndReturnToPool(hitByPlayer: false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // Если врезались в игрока или стену (не сбиты атакой)
        if (other.CompareTag("Player") || other.CompareTag("Border"))
        {
            ExplodeAndReturnToPool(hitByPlayer: false);
        }
    }

    // --- ПУБЛИЧНЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ УРОНА ОТ ИГРОКА ---
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isDead = true;
            ExplodeAndReturnToPool(hitByPlayer: true);
        }
    }

    private void ExplodeAndReturnToPool(bool hitByPlayer)
    {
        SpawnParticles();

        // Если слово было убито атакой игрока — передаем сигнал боссу
        if (hitByPlayer)
        {
            onKilledByPlayer?.Invoke();
        }

        // Выключаем и возвращаем в очередь пула
        gameObject.SetActive(false);
        returnToPool?.Invoke(this);
    }

    private void SpawnParticles()
    {
        if (explosionPrefab != null)
        {
            GameObject particles = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(particles, 2f);
        }
    }
}