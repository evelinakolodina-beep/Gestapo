using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Dash_TopDownController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Настройки рывка")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    private CharacterController controller;
    private float h;
    private float v;

    private float dashCooldownTimer = 0f;
    private Vector3 lastMoveDirection;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (controller == null) return;

        // Обновляем кулдаун
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // Получаем ввод
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (Vector3.forward * v + Vector3.right * h).normalized;

        // Запоминаем направление
        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
        }

        // Рывок при нажатии WASD
        if (moveDirection != Vector3.zero && dashCooldownTimer <= 0)
        {
            float dashDistance = dashSpeed * dashDuration;
            controller.Move(moveDirection * dashDistance);

            dashCooldownTimer = dashCooldown;
        }
    }
}