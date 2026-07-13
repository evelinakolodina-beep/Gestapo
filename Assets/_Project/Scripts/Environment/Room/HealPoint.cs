using UnityEngine;

public class HealPoint : MonoBehaviour
{
    [Header("Настройки лечения")]
    [SerializeField] private float healAmount = 25f; // Сколько здоровья восстанавливает

    [Header("Настройки мигания")]
    [SerializeField] private float blinkSpeed = 1.5f; // Скорость мигания
    [SerializeField] private float minAlpha = 0.3f;   // Минимальная прозрачность
    [SerializeField] private float maxAlpha = 1f;     // Максимальная прозрачность

    [Header("Цвета")]
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f); // Чуть мягче красный
    [SerializeField] private Color whiteColor = Color.white;

    [Header("Настройки перехода")]
    [SerializeField] private float transitionSpeed = 5f; // Скорость смены цвета

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private float timeCounter = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer не найден на объекте HealPoint!");
        }
    }

    void Update()
    {
        if (!isActivated)
        {
            // Медленное мигание красным
            timeCounter += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(timeCounter) + 1f) / 2f);

            Color blinkColor = redColor;
            blinkColor.a = alpha;
            spriteRenderer.color = blinkColor;
        }
        else
        {
            // Плавный переход к белому и удержание белого цвета
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, whiteColor, Time.deltaTime * transitionSpeed);
        }
    }

    
  

    // Для 3D игры (если вдруг используешь 3D коллайдеры)
    void OnTriggerEnter(Collider other)
    {
        TryHealPlayer(other.gameObject);
    }

    private void TryHealPlayer(GameObject target)
    {
        // Проверяем тег и наличие компонента PlayerHealth
        if (target.CompareTag("Player") && !isActivated)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Вызываем метод лечения из твоего скрипта
                playerHealth.Heal(healAmount);

                // Активируем точку (она станет белой и перестанет мигать)
                isActivated = true;

                Debug.Log($"Игрок вылечен на {healAmount} ед. Текущее здоровье: {playerHealth.CurrentHealth}");
            }
            else
            {
                Debug.LogWarning("На игроке не найден компонент PlayerHealth!");
            }
        }
    }

    /// <summary>
    /// Вызови этот метод извне, если нужно сделать точку лечения снова активной (например, через время или по событию)
    /// </summary>
    public void ResetPoint()
    {
        isActivated = false;
        timeCounter = 0f;
    }
}