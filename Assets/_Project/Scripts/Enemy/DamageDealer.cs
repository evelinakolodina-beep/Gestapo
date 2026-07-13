using UnityEngine;

// Вешается на когти/лапы босса, которые бьют игрока
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float contactDamage = 20f;

    // Если это триггер, который должен наносить урон при касании
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что столкнулись именно с игроком (по тегу или слою)
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }
}
