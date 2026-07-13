using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Настройки рывка")]
    [SerializeField] private float dashSpeed = 15f;      // Скорость рывка
    [SerializeField] private float dashDuration = 0.2f;  // Длительность рывка в секундах
    [SerializeField] private float dashCooldown = 1f;    // Перезарядка рывка в секундах
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift; // Клавиша рывка

    private CharacterController controller;
    private float h;
    private float v;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 lastMoveDirection;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (controller == null) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Обновляем таймер перезарядки рывка
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // 2. Если идет рывок, применяем его и пропускаем обычное движение
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            else
            {
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);
                return; // Прерываем выполнение, чтобы не сработало обычное движение
            }
        }

        // 3. Считываем ввод
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // 4. Абсолютное движение по мировым осям X и Z
        Vector3 moveDirection = (Vector3.forward * v + Vector3.right * h).normalized;

        // Запоминаем последнее направление движения (нужно для рывка на месте)
        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
        }

        // 5. Проверка нажатия рывка
        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0)
        {
            // Рывок в текущем направлении, либо в последнем, если ввод сброшен
            Vector3 currentDashDir = moveDirection != Vector3.zero ? moveDirection : lastMoveDirection;

            if (currentDashDir != Vector3.zero)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                dashDirection = currentDashDir;
                return;
            }
        }

        // 6. Применяем обычное движение
        Vector3 finalMove = moveDirection * moveSpeed;
        controller.Move(finalMove * Time.deltaTime);
    }
}