using UnityEngine;
using System.Collections.Generic;

public class WordController : MonoBehaviour
{
    [Header("Настройки квадрата")]
    public float squareSize = 10f;

    [Header("Настройки движения")]
    public float headSpeed = 5f;
    public float followDistance = 1.5f;
    public bool rotateLetters = true;
    public float rotationSmoothness = 10f;
    public float pauseDuration = 2f;

    [Header("Управление движением")]
    [SerializeField] private bool canMove = false;

    private Transform[] letters;
    private Vector3[] corners;

    private readonly int[] route = { 0, 3, 2, 1 };
    private int currentTargetIndex = 0;

    private Vector3 targetPosition;

    private List<Vector3> headHistory = new List<Vector3>();
    private List<float> headDistances = new List<float>();
    private float totalDistance = 0f;

    private enum State { Paused, MovingToCorner, MovingToCenter }
    private State currentState = State.Paused;
    private float pauseTimer = 0f;

    void Start()
    {
        letters = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            letters[i] = transform.GetChild(i);

        if (letters.Length == 0)
        {
            Debug.LogError("No child letters found!");
            return;
        }

        UpdateCorners();
        InitializeStart();
    }

    public void InitializeStart()
    {
        float totalWordLength = (letters.Length - 1) * followDistance;
        float halfWordLength = totalWordLength / 2f;

        int fromCorner = 1;
        int toCorner = 0;
        Vector3 dir = (corners[toCorner] - corners[fromCorner]).normalized;
        Vector3 sideCenter = (corners[fromCorner] + corners[toCorner]) / 2f;

        Vector3 headPos = sideCenter + dir * halfWordLength;
        letters[0].position = headPos;

        for (int i = 1; i < letters.Length; i++)
            letters[i].position = headPos - dir * (i * followDistance);

        headHistory.Clear();
        headDistances.Clear();
        totalDistance = 0f;

        for (int i = letters.Length - 1; i >= 1; i--)
        {
            headHistory.Add(headPos - dir * (i * followDistance));
            headDistances.Add(-i * followDistance);
        }
        headHistory.Add(headPos);
        headDistances.Add(0f);

        currentState = State.Paused;
        pauseTimer = pauseDuration;
        currentTargetIndex = 0;

        canMove = false;
    }

    public void StartMoving()
    {
        canMove = true;
        AudioManager.PlayBossHit(3);
        Debug.Log("[WordController] Движение запущено");
    }

    public void StopMoving()
    {
        canMove = false;
        Debug.Log("[WordController] Движение остановлено");
    }

    void Update()
    {
        if (letters == null || letters.Length == 0) return;
        if (!canMove) return;

        UpdateCorners();

        switch (currentState)
        {
            case State.Paused:
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0f)
                {
                    targetPosition = corners[route[currentTargetIndex]];
                    currentState = State.MovingToCorner;
                }
                break;

            case State.MovingToCorner:
                letters[0].position = Vector3.MoveTowards(letters[0].position, targetPosition, headSpeed * Time.deltaTime);
                if (Vector3.Distance(letters[0].position, targetPosition) < 0.01f)
                {
                    int fromIdx = route[currentTargetIndex];
                    int toIdx = route[(currentTargetIndex + 1) % 4];
                    targetPosition = GetHeadCenterPosition(fromIdx, toIdx);
                    currentState = State.MovingToCenter;
                }
                break;

            case State.MovingToCenter:
                letters[0].position = Vector3.MoveTowards(letters[0].position, targetPosition, headSpeed * Time.deltaTime);
                if (Vector3.Distance(letters[0].position, targetPosition) < 0.01f)
                {
                    currentTargetIndex = (currentTargetIndex + 1) % 4;
                    currentState = State.Paused;
                    pauseTimer = pauseDuration;
                }
                break;
        }

        RecordHeadHistory();
        MoveTailFromHistory();
    }

    Vector3 GetHeadCenterPosition(int fromCornerIdx, int toCornerIdx)
    {
        float halfWordLength = (letters.Length - 1) * followDistance / 2f;
        Vector3 dir = (corners[toCornerIdx] - corners[fromCornerIdx]).normalized;
        Vector3 center = (corners[fromCornerIdx] + corners[toCornerIdx]) / 2f;
        return center + dir * halfWordLength;
    }

    void RecordHeadHistory()
    {
        Vector3 pos = letters[0].position;
        if (headHistory.Count == 0)
        {
            headHistory.Add(pos);
            headDistances.Add(0f);
            return;
        }

        float dist = Vector3.Distance(headHistory[headHistory.Count - 1], pos);
        if (dist > 0.001f)
        {
            totalDistance += dist;
            headHistory.Add(pos);
            headDistances.Add(totalDistance);
        }

        float maxDist = (letters.Length + 2) * followDistance;
        while (headDistances.Count > 1 && totalDistance - headDistances[1] > maxDist)
        {
            headHistory.RemoveAt(0);
            headDistances.RemoveAt(0);
        }
    }

    void MoveTailFromHistory()
    {
        if (headHistory.Count < 2) return;

        for (int i = 1; i < letters.Length; i++)
        {
            float targetDist = totalDistance - i * followDistance;

            int idx = 0;
            for (int j = headDistances.Count - 2; j >= 0; j--)
            {
                if (headDistances[j] <= targetDist)
                {
                    idx = j;
                    break;
                }
            }

            if (idx >= headHistory.Count - 1) continue;

            float d1 = headDistances[idx];
            float d2 = headDistances[idx + 1];
            float t = (d2 - d1) > 0.0001f ? (targetDist - d1) / (d2 - d1) : 0f;
            t = Mathf.Clamp01(t);

            letters[i].position = Vector3.Lerp(headHistory[idx], headHistory[idx + 1], t);

            if (rotateLetters)
            {
                Vector3 segDir = headHistory[idx + 1] - headHistory[idx];
                if (segDir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(segDir.x, segDir.z) * Mathf.Rad2Deg;
                    Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                    if (rotationSmoothness > 0)
                        letters[i].localRotation = Quaternion.Slerp(letters[i].localRotation, targetRot, rotationSmoothness * Time.deltaTime);
                    else
                        letters[i].localRotation = targetRot;
                }
            }
        }
    }

    void UpdateCorners()
    {
        float h = squareSize / 2f;
        float y = transform.position.y;

        corners = new Vector3[4];
        corners[0] = new Vector3(transform.position.x - h, y, transform.position.z + h);
        corners[1] = new Vector3(transform.position.x + h, y, transform.position.z + h);
        corners[2] = new Vector3(transform.position.x + h, y, transform.position.z - h);
        corners[3] = new Vector3(transform.position.x - h, y, transform.position.z - h);
    }

    void OnDrawGizmos()
    {
        Vector3[] gc = GetGizmoCorners();

        Gizmos.color = Color.green;
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(gc[i], gc[(i + 1) % 4]);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < 4; i++)
            Gizmos.DrawWireSphere(gc[i], 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    Vector3[] GetGizmoCorners()
    {
        float h = squareSize / 2f;
        float y = transform.position.y;
        return new Vector3[4]
        {
            new Vector3(transform.position.x - h, y, transform.position.z + h),
            new Vector3(transform.position.x + h, y, transform.position.z + h),
            new Vector3(transform.position.x + h, y, transform.position.z - h),
            new Vector3(transform.position.x - h, y, transform.position.z - h)
        };
    }
}