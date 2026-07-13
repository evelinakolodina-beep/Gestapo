using System;
using UnityEngine;

public class BossHelicopterAttack : MonoBehaviour, IBossAttack
{
    public event Action OnCompleted;
    private Transform _playerTransform;

    [Header("Ссылки")]
    [SerializeField] private Transform[] attackObjects;
    [SerializeField] private Transform wordObject;

    [Header("Параметры распределения")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float arrangeLerpSpeed = 5f;

    [Header("Параметры вращения")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Параметры рывка")]
    [SerializeField] private float dashSpeed = 10f;

    private enum State { IDLE, ARRANGING, ROTATING, DASHING }
    private State currentState = State.IDLE;

    private Vector3 dashDirection;
    private Rigidbody rb;
    private bool isArranging = false;

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
    }

    public void Execute()
    {
        currentState = State.ARRANGING;
    }

    public void Cancel()
    {
        StopDash();
        isArranging = false;
        currentState = State.IDLE;
        OnCompleted?.Invoke();
    }

    private void Update()
    {
        if (attackObjects == null || attackObjects.Length == 0) return;

        switch (currentState)
        {
            case State.IDLE:
                break;
            case State.ARRANGING:
                HandleArranging();
                break;
            case State.ROTATING:
                HandleRotating();
                break;
            case State.DASHING:
                HandleDashing();
                break;
        }
    }

    private void HandleArranging()
    {
        ArrangeObjects();

        if (!isArranging)
        {
            currentState = State.ROTATING;
        }
    }

    private void HandleRotating()
    {
        if (wordObject != null)
        {
            wordObject.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }

        if (_playerTransform != null)
        {
            Vector3 toPlayer = _playerTransform.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                dashDirection = toPlayer.normalized;
                currentState = State.DASHING;
            }
        }
    }

    private void HandleDashing()
    {
        if (wordObject != null)
        {
            wordObject.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }

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

    private void OnBorderHit()
    {
        if (currentState == State.DASHING)
        {
            StopDash();
            currentState = State.ROTATING;
        }
    }

    private void StopDash()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    public void StopRotation()
    {
        // Метод есть, но не вызывается
    }

    private void ArrangeObjects()
    {
        if (!isArranging)
        {
            isArranging = true;
        }

        int count = attackObjects.Length;
        bool allArranged = true;

        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            Transform child = attackObjects[i];
            if (child == null) continue;

            float targetAngle = i * (360f / count);
            float targetAngleRad = targetAngle * Mathf.Deg2Rad;

            Vector3 targetWorldPos = center + new Vector3(
                radius * Mathf.Cos(targetAngleRad),
                0f,
                radius * Mathf.Sin(targetAngleRad)
            );

            child.position = Vector3.Lerp(
                child.position,
                targetWorldPos,
                arrangeLerpSpeed * Time.deltaTime
            );

            Vector3 directionFromCenter = (targetWorldPos - center).normalized;
            Quaternion targetWorldRotation = Quaternion.LookRotation(directionFromCenter, Vector3.up);

            Transform parent = child.parent;
            Quaternion targetLocalRotation;
            if (parent != null)
            {
                targetLocalRotation = Quaternion.Inverse(parent.rotation) * targetWorldRotation;
            }
            else
            {
                targetLocalRotation = targetWorldRotation;
            }

            targetLocalRotation = targetLocalRotation * Quaternion.Euler(90f, 0f, 0f);

            child.localRotation = Quaternion.Slerp(
                child.localRotation,
                targetLocalRotation,
                arrangeLerpSpeed * Time.deltaTime
            );

            float posDistance = Vector3.Distance(child.position, targetWorldPos);
            float rotDistance = Quaternion.Angle(child.localRotation, targetLocalRotation);

            if (posDistance > 0.05f || rotDistance > 0.5f)
            {
                allArranged = false;
            }
            else
            {
                child.position = targetWorldPos;
                child.localRotation = targetLocalRotation;
            }
        }

        if (allArranged)
        {
            isArranging = false;
            Debug.Log("[BossHelicopterAttack] Распределение завершено");
        }
    }
}