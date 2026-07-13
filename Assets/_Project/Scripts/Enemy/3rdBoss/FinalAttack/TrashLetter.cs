using UnityEngine;

public class TrashLetter : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private GameObject explosionPrefab;

    public void Initialize(float moveSpeed, GameObject particles)
    {
        speed = moveSpeed;
        explosionPrefab = particles;

        // Генерируем случайное направление строго в плоскости ZX (Y = 0)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(randomDir.x, 0f, randomDir.y);

        // Таймер самоуничтожения (страховка, если буква не заденет триггер)
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        // Двигаем букву
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    // Метод входа в триггер (на букве должен висеть коллайдер с галочкой IsTrigger)
    private void OnTriggerEnter(Collider other)
    { 
        
        if (explosionPrefab != null)
            {
                GameObject particles = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(particles, 2f);
            }

            // Уничтожаем букву при касании
            Destroy(gameObject);
       
    }
}