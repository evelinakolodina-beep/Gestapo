using System;
using UnityEngine;

public class BossDash : MonoBehaviour, IBossAttack
{
    public event Action OnCompleted;
    private Transform _playerTransform;

    [Header("Ссылки")]
    [SerializeField] private Collider attackCollider;

    [Header("Параметры поворота")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float aimThreshold = 1f;

    [Header("Параметры упреждения")]
    [SerializeField] private float predictionTime = 0.5f; // На сколько секунд вперед предсказываем движение

    [Header("Параметры рывка")]
    [SerializeField] private float dashSpeed = 15f;

    [Header("Параметры задержки")]
    [SerializeField] private float pauseAfterBorderHit = 1f;
    private float pauseTimer;

    private enum State { IDLE, ROTATING, DASHING, PAUSED }
    private State currentState = State.IDLE;

    private Vector3 dashDirection;
    private Rigidbody rb;

    // Переменные для предсказания движения игрока
    private Vector3 _targetPosition; // Запомненная цель с упреждением
    private Vector3 _lastPlayerPos;
    private Vector3 _playerVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        EventManager.OnBorderHit += OnBorderHit;
    }

    private void OnDisable()
    {
        EventManager.OnBorderHit -= OnBorderHit;
    }

    public void Setup(Transform player)
    {
        _playerTransform = player;
        if (_playerTransform != null) _lastPlayerPos = _playerTransform.position;
    }

    public void Execute()
    {
        // Запоминаем позицию игрока и вычисляем упреждение
        CalculateTargetPosition();
        currentState = State.ROTATING;
    }

    public void Cancel()
    {
        StopDash();
        currentState = State.IDLE;
        OnCompleted?.Invoke();
    }

    private void Update()
    {
        if (_playerTransform == null || attackCollider == null) return;

        // Обновляем скорость игрока для следующего использования
        float deltaTime = Time.deltaTime > 0f ? Time.deltaTime : 0.01f;
        _playerVelocity = (_playerTransform.position - _lastPlayerPos) / deltaTime;
        _lastPlayerPos = _playerTransform.position;

        switch (currentState)
        {
            case State.IDLE:
                break;
            case State.ROTATING:
                HandleRotating();
                break;
            case State.DASHING:
                HandleDashing();
                break;
            case State.PAUSED:
                HandlePaused();
                break;
        }
    }

    private void CalculateTargetPosition()
    {
        if (_playerTransform == null) return;

        // Берем текущую позицию игрока
        Vector3 playerPos = _playerTransform.position;

        // Добавляем упреждение на основе скорости игрока
        Vector3 predictedPosition = playerPos + _playerVelocity * predictionTime;

        // Сохраняем как цель
        _targetPosition = predictedPosition;
    }

    private void HandleRotating()
    {
        Vector3 parentPos = transform.position;
        Vector3 childPos = attackCollider.transform.position;

        Vector3 currentDir = childPos - parentPos;
        currentDir.y = 0f;

        // Используем запомненную позицию с упреждением вместо текущей позиции игрока
        Vector3 targetDir = _targetPosition - parentPos;
        targetDir.y = 0f;

        if (currentDir.sqrMagnitude < 0.0001f || targetDir.sqrMagnitude < 0.0001f) return;

        currentDir.Normalize();
        targetDir.Normalize();

        float angle = Vector3.SignedAngle(currentDir, targetDir, Vector3.up);

        if (Mathf.Abs(angle) <= aimThreshold)
        {
            dashDirection = targetDir;
            currentState = State.DASHING;
            return;
        }

        if (rotationSpeed <= 0f)
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.up) * transform.rotation;
        }
        else
        {
            float maxDelta = rotationSpeed * Time.deltaTime;
            float delta = Mathf.Clamp(angle, -maxDelta, maxDelta);
            transform.rotation = Quaternion.AngleAxis(delta, Vector3.up) * transform.rotation;
        }
    }

    private void HandleDashing()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                rb.linearVelocity.y,
                dashDirection.z * dashSpeed
            );
        }
        else
        {
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
        }
    }

    private void HandlePaused()
    {
        pauseTimer -= Time.deltaTime;

        if (pauseTimer <= 0f)
        {
            // После паузы снова вычисляем новую цель с упреждением
            CalculateTargetPosition();
            currentState = State.ROTATING;
        }
    }

    private void OnBorderHit()
    {
        if (currentState == State.DASHING)
        {
            StopDash();
            currentState = State.PAUSED;
            pauseTimer = pauseAfterBorderHit;
        }
    }

    private void StopDash()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}