using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordCircleAttack : MonoBehaviour, IBossAttack
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] letters;

    [Header("Параметры атаки")]
    [SerializeField] private bool useClosestLetterAttack = true;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private float moveToCircleSpeed = 8f;
    [SerializeField] private float attackSpeed = 20f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float delayBeforeCenterAttack = 0.3f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    private float nextAttackTime;
    private readonly List<Transform> attackingLetters = new List<Transform>();

    private bool isExecuting = false;
    public event System.Action OnCompleted;

    public void Setup(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void Execute()
    {
        isExecuting = true;
    }

    public void Cancel()
    {
        isExecuting = false;
        StopAllCoroutines();
        attackingLetters.Clear();
    }

    private void Update()
    {
        if (isExecuting)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || player == null)
        {
            return;
        }

        if (useClosestLetterAttack)
        {
            AttackWithClosestLetter();
        }
        else
        {
            AttackWithAllLetters();
        }

        nextAttackTime = Time.time + attackCooldown;
    }

    private void AttackWithAllLetters()
    {
        List<Transform> availableLetters = GetAvailableLetters();

        if (availableLetters.Count > 0)
        {
            StartCoroutine(CircleAttackSequence(availableLetters));
        }
    }

    private void AttackWithClosestLetter()
    {
        Transform closestLetter = GetClosestAvailableLetter();

        if (closestLetter != null)
        {
            List<Transform> singleLetterList = new List<Transform> { closestLetter };
            StartCoroutine(CircleAttackSequence(singleLetterList));
        }
    }

    private List<Transform> GetAvailableLetters()
    {
        List<Transform> availableLetters = new List<Transform>();

        foreach (Transform letter in letters)
        {
            if (IsLetterAvailable(letter))
            {
                availableLetters.Add(letter);
            }
        }

        return availableLetters;
    }

    private Transform GetClosestAvailableLetter()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Transform letter in letters)
        {
            if (!IsLetterAvailable(letter))
            {
                continue;
            }

            float distance = Vector3.Distance(letter.position, player.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = letter;
            }
        }

        return closest;
    }

    private bool IsLetterAvailable(Transform letter)
    {
        return letter.parent == transform && !attackingLetters.Contains(letter);
    }

    private IEnumerator CircleAttackSequence(List<Transform> lettersToAttack)
    {
        Dictionary<Transform, Vector3> originalLocalPositions = new Dictionary<Transform, Vector3>();
        foreach (Transform letter in lettersToAttack)
        {
            attackingLetters.Add(letter);
            originalLocalPositions[letter] = letter.localPosition;
        }

        Vector3 playerLocalPosition = transform.InverseTransformPoint(player.position);
        playerLocalPosition.y = 0f;

        List<Vector3> circlePositions = CalculateCirclePositions(playerLocalPosition, lettersToAttack.Count);
        StartMovementToLocalPositions(lettersToAttack, circlePositions, moveToCircleSpeed);

        yield return new WaitUntil(() => AllLettersAtLocalPositions(lettersToAttack, circlePositions));

        yield return new WaitForSeconds(delayBeforeCenterAttack);

        StartMovementToLocalPosition(lettersToAttack, playerLocalPosition, attackSpeed);

        yield return new WaitUntil(() => AllLettersAtLocalPosition(lettersToAttack, playerLocalPosition));

        ReturnLettersToOriginalPositions(lettersToAttack, originalLocalPositions);

        yield return new WaitUntil(() => AllLettersAtLocalPositions(
            lettersToAttack,
            GetOriginalPositionsList(lettersToAttack, originalLocalPositions)));

        foreach (Transform letter in lettersToAttack)
        {
            attackingLetters.Remove(letter);
        }
    }

    private List<Vector3> CalculateCirclePositions(Vector3 centerLocalPosition, int count)
    {
        List<Vector3> positions = new List<Vector3>();
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * circleRadius;
            positions.Add(centerLocalPosition + offset);
        }

        return positions;
    }

    private void StartMovementToLocalPositions(List<Transform> lettersToMove, List<Vector3> targets, float speed)
    {
        for (int i = 0; i < lettersToMove.Count; i++)
        {
            StartCoroutine(MoveLetterToLocalPosition(lettersToMove[i], targets[i], speed));
        }
    }

    private void StartMovementToLocalPosition(List<Transform> lettersToMove, Vector3 target, float speed)
    {
        foreach (Transform letter in lettersToMove)
        {
            StartCoroutine(MoveLetterToLocalPosition(letter, target, speed));
        }
    }

    private void ReturnLettersToOriginalPositions(List<Transform> lettersToReturn, Dictionary<Transform, Vector3> originalPositions)
    {
        foreach (Transform letter in lettersToReturn)
        {
            StartCoroutine(MoveLetterToLocalPosition(letter, originalPositions[letter], returnSpeed));
        }
    }

    private List<Vector3> GetOriginalPositionsList(List<Transform> lettersToCheck, Dictionary<Transform, Vector3> originalPositions)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Transform letter in lettersToCheck)
        {
            positions.Add(originalPositions[letter]);
        }
        return positions;
    }

    private IEnumerator MoveLetterToLocalPosition(Transform letter, Vector3 targetLocalPosition, float speed)
    {
        targetLocalPosition.y = 0f;

        while (Vector3.Distance(letter.localPosition, targetLocalPosition) > arrivalThreshold)
        {
            Vector3 currentPos = letter.localPosition;
            currentPos.y = 0f;

            letter.localPosition = Vector3.MoveTowards(
                currentPos,
                targetLocalPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }

        letter.localPosition = targetLocalPosition;
    }

    private bool AllLettersAtLocalPositions(List<Transform> lettersToCheck, List<Vector3> positions)
    {
        for (int i = 0; i < lettersToCheck.Count; i++)
        {
            if (Vector3.Distance(lettersToCheck[i].localPosition, positions[i]) > arrivalThreshold)
            {
                return false;
            }
        }
        return true;
    }

    private bool AllLettersAtLocalPosition(List<Transform> lettersToCheck, Vector3 position)
    {
        foreach (Transform letter in lettersToCheck)
        {
            if (Vector3.Distance(letter.localPosition, position) > arrivalThreshold)
            {
                return false;
            }
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, circleRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}