using UnityEngine;
using System.Collections;

public class WordAttack : MonoBehaviour, IBossAttack
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] letters;

    [Header("Параметры атаки")]
    [SerializeField] private bool useClosestLetterAttack = true;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float lungeDistance = 3f;
    [SerializeField] private float lungeSpeed = 15f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    private float nextAttackTime;
    private readonly System.Collections.Generic.List<Transform> attackingLetters =
        new System.Collections.Generic.List<Transform>();

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
        foreach (Transform letter in letters)
        {
            if (IsLetterAvailable(letter))
            {
                StartCoroutine(LungeLetter(letter));
            }
        }
    }

    private void AttackWithClosestLetter()
    {
        Transform closestLetter = GetClosestAvailableLetter();

        if (closestLetter != null)
        {
            StartCoroutine(LungeLetter(closestLetter));
        }
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
        return !attackingLetters.Contains(letter);
    }

    private IEnumerator LungeLetter(Transform letter)
    {
        attackingLetters.Add(letter);

        AudioManager.PlayBossHit(1);

        Vector3 originalLocalPosition = letter.localPosition;

        Vector3 playerLocal = transform.InverseTransformPoint(player.position);
        Vector3 directionToPlayer = playerLocal - originalLocalPosition;
        directionToPlayer.y = 0f;
        directionToPlayer = directionToPlayer.sqrMagnitude > 0.0001f
            ? directionToPlayer.normalized
            : Vector3.forward;

        float localLungeDistance = GetLocalScalar(lungeDistance);
        Vector3 targetLocalPosition = originalLocalPosition + directionToPlayer * localLungeDistance;

        float localLungeSpeed = GetLocalScalar(lungeSpeed);
        float localReturnSpeed = GetLocalScalar(returnSpeed);

        while (Vector3.Distance(letter.localPosition, targetLocalPosition) > arrivalThreshold)
        {
            letter.localPosition = Vector3.MoveTowards(
                letter.localPosition,
                targetLocalPosition,
                localLungeSpeed * Time.deltaTime
            );
            yield return null;
        }

        letter.localPosition = targetLocalPosition;

        while (Vector3.Distance(letter.localPosition, originalLocalPosition) > arrivalThreshold)
        {
            letter.localPosition = Vector3.MoveTowards(
                letter.localPosition,
                originalLocalPosition,
                localReturnSpeed * Time.deltaTime
            );
            yield return null;
        }

        letter.localPosition = originalLocalPosition;

        attackingLetters.Remove(letter);
    }

    private float GetLocalScalar(float worldValue)
    {
        float scale = transform.lossyScale.x;
        return Mathf.Approximately(scale, 0f) ? worldValue : worldValue / scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}