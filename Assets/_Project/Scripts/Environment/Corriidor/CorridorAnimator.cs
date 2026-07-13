using UnityEngine;

public class CorridorAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform targetImage;

    [Header("Параметры анимации")]
    [Tooltip("Начальная высота коридора (до анимации)")]
    [SerializeField] private float initialHeight = 12f;

    [Tooltip("Насколько вырастет коридор (конечная высота = initialHeight + growAmount)")]
    [SerializeField] private float growAmount = 48f; // 12 + 48 = 60

    [SerializeField] private float growSpeed = 400f;

    private float startHeight;
    private float targetHeight;
    private bool isGrowing = false;
    private bool hasGrown = false;

    [ContextMenu("Test Start Growth (Editor)")]
    public void StartGrowth()
    {
        if (targetImage == null)
        {
            Debug.LogError("[CorridorAnimator] Ошибка: targetImage не назначен в инспекторе!");
            return;
        }

        if (isGrowing || hasGrown)
        {
            Debug.Log("[CorridorAnimator] Анимация уже запущена или уже была завершена.");
            return;
        }

        startHeight = initialHeight;
        targetHeight = initialHeight + growAmount;
        isGrowing = true;

       // Debug.Log($"<color=green>[StartGrowth]</color> Анимация запущена. Начальная высота: {startHeight}, Целевая высота: {targetHeight}");
    }

    public void ResetState()
    {
        isGrowing = false;
        hasGrown = false;

        if (targetImage != null)
        {
            targetImage.sizeDelta = new Vector2(targetImage.sizeDelta.x, initialHeight);
        }

        //Debug.Log("<color=yellow>[ResetState]</color> Состояние сброшено. Высота возвращена к начальной.");
    }

    private void Update()
    {
        if (!isGrowing) return;

        float currentHeight = Mathf.MoveTowards(
            targetImage.sizeDelta.y,
            targetHeight,
            growSpeed * Time.deltaTime
        );

        targetImage.sizeDelta = new Vector2(targetImage.sizeDelta.x, currentHeight);

        if (Mathf.Approximately(currentHeight, targetHeight))
        {
            isGrowing = false;
            hasGrown = true;
           // Debug.Log($"<color=cyan>[EndGrowth]</color> Анимация успешно завершена. Итоговая высота: {targetImage.sizeDelta.y}");
        }
    }

   
}