using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Настройки рывка")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Настройки атаки")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask damageableLayer;

    [Header("Анимация")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Настройки звука")]
    [SerializeField] private float footstepInterval = 0.35f;

    // --- Компоненты и состояния ---
    private CharacterController controller;
    private Camera mainCam;

    private float h;
    private float v;
    private Vector3 lastMoveDirection;

    // Состояния рывка
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    // Состояния атаки
    private bool isAttacking = false;

    // Состояния анимации и звука
    private string lastPlayedAnim = "";
    private bool isFacingLeft = false;
    private float footstepTimer = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hitbox != null)
            hitbox.SetActive(false);
    }

    private void Update()
    {
        if (controller == null) return;

        // Атака по клику левой кнопки мыши
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformAttack();
        }

        HandleMovement();
    }

    private void PerformAttack()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            isAttacking = true;
            AudioManager.PlayPlayerAttack();

            Vector3 clickPoint = ray.GetPoint(distance);

            // 🔄 ПРОСТОЙ ФЛИП: если курсор левее персонажа, разворачиваем спрайт
            isFacingLeft = (clickPoint.x < transform.position.x);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = isFacingLeft;
            }

            // Запускаем анимацию атаки (она изначально смотрит вправо)
            if (animator != null)
            {
                animator.Play("attack");
                lastPlayedAnim = "attack"; // Блокируем перезапись анимации движением
            }

            // Логика хитбокса и урона
            Vector3 direction = (clickPoint - transform.position).normalized;
            if (direction == Vector3.zero) direction = isFacingLeft ? Vector3.left : Vector3.right;

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
            bool wordHitThisAttack = false;

            foreach (var col in hitColliders)
            {
                if (col.TryGetComponent<DamageReceiver>(out var receiver))
                {
                    EnemyHealth health = receiver.GetRootHealth();
                    if (health != null && damagedEnemies.Add(health))
                    {
                        health.TakeDamage(attackDamage);
                    }
                }

                if (!wordHitThisAttack && col.TryGetComponent<WordProjectile>(out var wordProjectile))
                {
                    wordProjectile.TakeDamage(attackDamage);
                    wordHitThisAttack = true;
                }

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

    private void HandleMovement()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

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

        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (Vector3.forward * v + Vector3.right * h).normalized;
        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving)
            lastMoveDirection = moveDirection;

        // Рывок (нельзя во время атаки)
        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0 && !isAttacking)
        {
            Vector3 currentDashDir = isMoving ? moveDirection : lastMoveDirection;

            if (currentDashDir != Vector3.zero)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                dashDirection = currentDashDir;
                AudioManager.PlayDash();
                return;
            }
        }

        // Обычное движение
        Vector3 finalMove = moveDirection * moveSpeed;
        controller.Move(finalMove * Time.deltaTime);

        // Звук шагов
        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        // Обновляем анимацию движения ТОЛЬКО если не атакуем
        if (!isAttacking)
        {
            UpdateAnimation(h, v, moveDirection);
        }
    }

    private void UpdateAnimation(float h, float v, Vector3 moveDirection)
    {
        if (animator == null) return;

        string targetAnim = "";
        bool isMoving = moveDirection != Vector3.zero;

        if (isMoving)
        {
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

    private void ResetAttack()
    {
        isAttacking = false;
        // После атаки возвращаем персонажа в анимацию движения/покоя 
        // и восстанавливаем направление взгляда на основе последнего движения
        UpdateAnimation(h, v, (Vector3.forward * v + Vector3.right * h).normalized);
    }

    private void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.SetActive(false);
    }
}
