using UnityEngine;
using System.Collections;

public class PlayerFlinch : MonoBehaviour
{
    [Header("Что двигать?")]
    [SerializeField, Tooltip("Перетащите сюда дочерний объект со спрайтом игрока")]
    private Transform spriteTransform;

    [Header("Настройки")]
    [SerializeField] private float flinchDistance = 0.2f; // Сила отскока
    [SerializeField] private float flinchDuration = 0.15f; // Время анимации
    [SerializeField] private bool useRandomDirection = false; // Отлетать хаотично или от врага?

    private Vector3 startLocalPos;

    private void Start()
    {
        // Запоминаем изначальную позицию спрайта, чтобы всегда возвращать его на место
        if (spriteTransform != null)
        {
            startLocalPos = spriteTransform.localPosition;
        }
        else
        {
            Debug.LogError("Вы не назначили spriteTransform в скрипте PlayerFlinch!", this);
        }
    }

    // Этот метод будет дергать PlayerHealth
    public void PlayFlinch(Vector3 attackerPosition)
    {
        if (spriteTransform == null) return;

        StopAllCoroutines();
        StartCoroutine(FlinchRoutine(attackerPosition));
    }

    private IEnumerator FlinchRoutine(Vector3 attackerPos)
    {
        Vector3 localDir;

        // Если включен рандом ИЛИ позиция врага неизвестна
        if (useRandomDirection || attackerPos == Vector3.zero)
        {
            localDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        }
        else
        {
            // Отскок в противоположную от врага сторону
            Vector3 worldDir = transform.position - attackerPos;
            worldDir.y = 0; // Если игра 2D с видом сбоку, возможно тут нужно обнулять Z вместо Y

            worldDir = worldDir.sqrMagnitude > 0.001f ? worldDir.normalized : Random.insideUnitSphere;
            localDir = transform.InverseTransformDirection(worldDir);
        }

        Vector3 targetPos = startLocalPos + localDir * flinchDistance;
        float halfTime = flinchDuration / 2f;

        // Фаза 1: отскок
        float elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            spriteTransform.localPosition = Vector3.Lerp(startLocalPos, targetPos, elapsed / halfTime);
            yield return null;
        }

        // Фаза 2: возврат
        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            spriteTransform.localPosition = Vector3.Lerp(targetPos, startLocalPos, elapsed / halfTime);
            yield return null;
        }

        // Точно ставим на место
        spriteTransform.localPosition = startLocalPos;
    }
}