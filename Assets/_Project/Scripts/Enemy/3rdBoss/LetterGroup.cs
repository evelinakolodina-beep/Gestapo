using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterGroup : MonoBehaviour
{
    private List<Transform> letters = new List<Transform>();
    private float attackDistance;
    private float attackDuration;
    private float returnDelay;
    private float returnDuration;

    [Header("Настройки подготовки")]
    [SerializeField] private float prepareDuration = 0.8f;
    [SerializeField] private float shakeAmplitude = 0.1f;
    [SerializeField] private float shakeFrequency = 20f;

    private Vector3 homePosition;
    private Dictionary<Transform, Vector3> localOffsets = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> homeRotations = new Dictionary<Transform, Quaternion>();

    // Свойство, указывающее, является ли группа строкой
    public bool IsRow { get; private set; }

    public void Initialize(List<Transform> letters, float attackDistance,
                          float attackDuration, float returnDelay, float returnDuration, bool isRow)
    {
        this.letters = letters;
        this.attackDistance = attackDistance;
        this.attackDuration = attackDuration;
        this.returnDelay = returnDelay;
        this.returnDuration = returnDuration;
        this.IsRow = isRow; // Сохраняем тип группы

        homePosition = transform.position;

        foreach (var letter in letters)
        {
            localOffsets[letter] = letter.position - transform.position;
            homeRotations[letter] = letter.rotation;
        }
    }

    public IEnumerator Prepare()
    {
        float time = 0f;

        while (time < prepareDuration)
        {
            time += Time.deltaTime;
            float progress = time / prepareDuration;
            float currentAmplitude = shakeAmplitude * progress;

            foreach (var letter in letters)
            {
                Vector3 shakeOffset = new Vector3(
                    Mathf.PerlinNoise(Time.time * shakeFrequency, letter.GetInstanceID()) - 0.5f,
                    0f,
                    Mathf.PerlinNoise(Time.time * shakeFrequency + 100f, letter.GetInstanceID() + 50f) - 0.5f
                ) * currentAmplitude * 2f;

                float shakeAngle = (Mathf.PerlinNoise(Time.time * shakeFrequency + 200f, letter.GetInstanceID() + 100f) - 0.5f) * 30f * progress;

                letter.position = transform.position + localOffsets[letter] + shakeOffset;
                letter.rotation = homeRotations[letter] * Quaternion.Euler(0f, shakeAngle, 0f);
            }
            yield return null;
        }

        foreach (var letter in letters)
        {
            letter.position = transform.position + localOffsets[letter];
            letter.rotation = homeRotations[letter];
        }
    }

    public IEnumerator Attack(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - homePosition).normalized;
        Vector3 attackOffset = direction * attackDistance;

        Vector3 startPos = homePosition;
        Vector3 attackPos = homePosition + attackOffset;

        float time = 0f;
        bool damageDealt = false;

        while (time < attackDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / attackDuration);

            transform.position = Vector3.Lerp(startPos, attackPos, t);

            foreach (var letter in letters)
            {
                letter.position = transform.position + localOffsets[letter];
                letter.rotation = homeRotations[letter];
            }

            if (!damageDealt && CheckPlayerCollision())
            {
                damageDealt = true;
                Debug.Log($"[LetterGroup] {gameObject.name} НАНЁС УРОН игроку!");
            }

            yield return null;
        }

        transform.position = attackPos;
        foreach (var letter in letters)
        {
            letter.position = transform.position + localOffsets[letter];
            letter.rotation = homeRotations[letter];
        }

        if (!damageDealt && CheckPlayerCollision())
        {
            Debug.Log($"[LetterGroup] {gameObject.name} НАНЁС УРОН игроку!");
        }

        yield return new WaitForSeconds(returnDelay);

        time = 0f;
        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / returnDuration);

            transform.position = Vector3.Lerp(attackPos, startPos, t);

            foreach (var letter in letters)
            {
                letter.position = transform.position + localOffsets[letter];
                letter.rotation = homeRotations[letter];
            }
            yield return null;
        }

        transform.position = startPos;
        foreach (var letter in letters)
        {
            letter.position = transform.position + localOffsets[letter];
            letter.rotation = homeRotations[letter];
        }
    }

    private bool CheckPlayerCollision()
    {
        Collider groupCollider = GetComponent<Collider>();
        if (groupCollider == null) return false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && groupCollider.bounds.Intersects(playerCollider.bounds))
            {
                return true;
            }
        }
        return false;
    }
}