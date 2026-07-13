using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Настройки рывка")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Анимация (Спрайт и Аниматор на другом объекте)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Настройки звука")]
    [SerializeField] private float footstepInterval = 0.35f; // Интервал между шагами (в секундах)

    private CharacterController controller;
    private float h;
    private float v;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 lastMoveDirection;

    private string lastPlayedAnim = "";
    private bool isFacingLeft = false;

    // Таймер для звука шагов
    private float footstepTimer = 0f;

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
                return;
            }
        }

        // 3. Считываем ввод
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // 4. Абсолютное движение по мировым осям X и Z
        Vector3 moveDirection = (Vector3.forward * v + Vector3.right * h).normalized;
        bool isMoving = moveDirection != Vector3.zero; // Флаг движения для звука и анимации

        if (isMoving)
        {
            lastMoveDirection = moveDirection;
        }

        // --- ЛОГИКА АНИМАЦИИ ---
        UpdateAnimation(h, v, moveDirection);

        // 5. Проверка нажатия рывка
        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0)
        {
            Vector3 currentDashDir = isMoving ? moveDirection : lastMoveDirection;

            if (currentDashDir != Vector3.zero)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                dashDirection = currentDashDir;

                // ВОСПРОИЗВЕДЕНИЕ ЗВУКА РЫВКА
                AudioManager.PlayDash();

                return;
            }
        }

        // 6. Применяем обычное движение
        Vector3 finalMove = moveDirection * moveSpeed;
        controller.Move(finalMove * Time.deltaTime);

        //  ЛОГИКА ЗВУКА ШАГОВ
        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.PlayFootstep();
                footstepTimer = footstepInterval; // Сбрасываем таймер на следующий шаг
            }
        }
        else
        {
            // Если игрок остановился, сбрасываем таймер, 
            // чтобы при следующем движении первый шаг проигрался сразу
            footstepTimer = 0f;
        }
    }

    private void UpdateAnimation(float h, float v, Vector3 moveDirection)
    {
        if (animator == null) return;

        string targetAnim = "";
        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving)
        {
            // Определяем доминирующую ось
            if (Mathf.Abs(v) > Mathf.Abs(h))
            {
                targetAnim = v > 0 ? "back" : "forward";
            }
            else
            {
                targetAnim = "side";
                isFacingLeft = (h < 0);
            }

            if (targetAnim != lastPlayedAnim)
            {
                animator.Play(targetAnim);
                lastPlayedAnim = targetAnim;
            }
        }
        else
        {
            targetAnim = "idle";

            if (targetAnim != lastPlayedAnim)
            {
                animator.Play(targetAnim);
                lastPlayedAnim = targetAnim;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isFacingLeft;
        }
    }
}