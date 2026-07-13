using UnityEngine;
using System.Collections;

public class DamageFlinch : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField, Tooltip("Дочерний объект с мешем, который будет дергаться. Если пусто - возьмет первого ребенка.")]
    private Transform visualModel;

    [Header("Настройки вздрагивания")]
    [SerializeField] private float flinchDistance = 0.2f;
    [SerializeField] private float flinchDuration = 0.15f;
    [SerializeField] private bool useRandomDirection = false;

    private Vector3 startLocalPos;

    private void Awake()
    {
        // Пытаемся найти визуал автоматически, если он не задан в инспекторе
        if (visualModel == null)
        {
            if (transform.childCount > 0)
            {
                visualModel = transform.GetChild(0);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] DamageFlinch не нашел дочерний объект (visualModel) для анимации!", this);
                return;
            }
        }

        // Запоминаем локальную позицию именно ВИЗУАЛЬНОЙ модели
        startLocalPos = visualModel.localPosition;
    }

    public void ReactToHit(Vector3 attackerPosition)
    {
        if (visualModel == null) return;
        StopAllCoroutines();
        StartCoroutine(FlinchRoutine(attackerPosition));
    }

    public void ReactToHit()
    {
        if (visualModel == null) return;
        StopAllCoroutines();
        StartCoroutine(FlinchRoutine(Vector3.zero));
    }

    private IEnumerator FlinchRoutine(Vector3 attackerPos)
    {
        Vector3 localDir;

        if (useRandomDirection)
        {
            localDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }
        else
        {
            // Направление считаем от объекта с коллайдером (transform)
            Vector3 worldDir = (transform.position - attackerPos);
            worldDir.y = 0;
            worldDir = worldDir.sqrMagnitude > 0.001f ? worldDir.normalized : transform.forward;

            // Переводим мировое направление в локальное относительно родителя (коллайдера)
            localDir = transform.InverseTransformDirection(worldDir);
        }

        Vector3 targetPos = startLocalPos + localDir * flinchDistance;
        float halfTime = flinchDuration / 2f;

        float elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            // Двигаем дочерний объект, а не сам скрипт
            visualModel.localPosition = Vector3.Lerp(startLocalPos, targetPos, elapsed / halfTime);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            visualModel.localPosition = Vector3.Lerp(targetPos, startLocalPos, elapsed / halfTime);
            yield return null;
        }

        // Гарантированно возвращаем на место
        visualModel.localPosition = startLocalPos;
    }
}