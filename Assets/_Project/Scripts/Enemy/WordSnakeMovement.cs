using UnityEngine;

public class WordSnakeMovement : MonoBehaviour
{
    #region References

    [Header("Ссылки")]
    [Tooltip("Массив дочерних объектов (букв)")]
    [SerializeField] private Transform[] letters;

    #endregion

    #region Settings

    [Header("Параметры движения")]
    [Tooltip("Центр окружности. Если не задан — используется начальная позиция первой буквы")]
    [SerializeField] private Transform circleCenterOverride;

    [Tooltip("Радиус окружности")]
    [SerializeField] private float circleRadius = 5f;

    [Tooltip("Скорость вращения (градусы в секунду)")]
    [SerializeField] private float rotationSpeed = 30f;

    [Tooltip("Направление вращения (true = по часовой, false = против часовой)")]
    [SerializeField] private bool clockwise = true;

    [Tooltip("Расстояние между буквами вдоль окружности")]
    [SerializeField] private float linkDistance = 1f;

    #endregion

    #region Private Fields

    private Vector3 circleCenter;
    private float leaderAngle;
    private float angleOffsetPerLetter;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (letters.Length == 0)
        {
            Debug.LogError("WordSnakeMovement: Массив букв пуст!");
            return;
        }

        InitializeCircleCenter();
        InitializeAngles();
    }

    private void Update()
    {
        UpdateLeaderAngle();
        UpdateLetterPositions();
    }

    #endregion

    #region Initialization

    private void InitializeCircleCenter()
    {
        if (circleCenterOverride != null)
        {
            circleCenter = circleCenterOverride.position;
        }
        else
        {
            circleCenter = letters[0].position - Vector3.right * circleRadius;
        }
    }

    private void InitializeAngles()
    {
        // Вычисляем начальный угол первой буквы
        Vector3 offset = letters[0].position - circleCenter;
        offset.y = 0f;

        if (offset.magnitude > 0.01f)
        {
            leaderAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        }

        // Угловой сдвиг между буквами (в градусах)
        // Длина дуги = радиус * угол(в радианах) => угол = длина / радиус
        if (circleRadius > 0.01f)
        {
            angleOffsetPerLetter = (linkDistance / circleRadius) * Mathf.Rad2Deg;
        }
    }

    #endregion

    #region Movement

    private void UpdateLeaderAngle()
    {
        float directionSign = clockwise ? -1f : 1f;
        leaderAngle += rotationSpeed * directionSign * Time.deltaTime;
    }

    private void UpdateLetterPositions()
    {
        for (int i = 0; i < letters.Length; i++)
        {
            // Каждая буква имеет свой угол, отстающий от ведущей
            float letterAngle = leaderAngle - (i * angleOffsetPerLetter);

            float angleRad = letterAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad),
                0f,
                Mathf.Sin(angleRad)
            ) * circleRadius;

            Vector3 targetPosition = circleCenter + offset;
            targetPosition.y = circleCenter.y;

            letters[i].position = targetPosition;
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        Vector3 center = circleCenterOverride != null ? circleCenterOverride.position : circleCenter;

        Gizmos.color = Color.cyan;
        DrawCircle(center, circleRadius);

        if (Application.isPlaying && letters.Length > 0)
        {
            for (int i = 0; i < letters.Length; i++)
            {
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, (float)i / letters.Length);
                Gizmos.DrawSphere(letters[i].position, 0.15f);
            }
        }
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 64;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), 0f, Mathf.Sin(angle1)) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), 0f, Mathf.Sin(angle2)) * radius;

            Gizmos.DrawLine(point1, point2);
        }
    }

    #endregion
}