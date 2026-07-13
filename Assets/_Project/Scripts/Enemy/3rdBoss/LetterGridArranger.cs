using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LetterGridArranger : MonoBehaviour
{
    [Header("Сетка")]
    [SerializeField] private bool autoSquare = true;
    [SerializeField] private int columns = 5;
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 spacing = new Vector2(0.2f, 0.2f);
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Связь с боем")]
    [SerializeField] private LetterCombatManager combatManager;

    // Ссылка на менеджер босса для уведомления о завершении
    private ThirdBossManager bossManager;

    private List<Transform> allLetters = new List<Transform>();

    private void Awake()
    {
        // Автоматически находим ThirdBossManager среди родительских объектов
        bossManager = GetComponentInParent<ThirdBossManager>();
    }

    public void ArrangeIntoGrid()
    {
        allLetters.Clear();

        // Используем transform объекта, на котором висит этот скрипт, как корень
        Transform root = this.transform;

        if (root.childCount == 0)
        {
            Debug.LogError("[LetterGridArranger] У корневого объекта нет дочерних элементов!");
            return;
        }

        // 1. Проходим по первому слою (контейнеры слов)
        for (int i = 0; i < root.childCount; i++)
        {
            Transform container = root.GetChild(i);

            if (container == null) continue;

            // 2. Из каждого контейнера вытаскиваем второй слой (сами буквы)
            for (int j = 0; j < container.childCount; j++)
            {
                Transform letter = container.GetChild(j);

                if (letter != null)
                {
                    allLetters.Add(letter);
                }
            }
        }

        if (allLetters.Count == 0)
        {
            Debug.LogError("[LetterGridArranger] Не найдено ни одной буквы! Проверьте иерархию.");
            return;
        }

        int totalLetters = allLetters.Count;
        int cols = autoSquare ? Mathf.CeilToInt(Mathf.Sqrt(totalLetters)) : columns;
        int rows = Mathf.CeilToInt((float)totalLetters / cols);

        // Перемешиваем буквы
        for (int i = allLetters.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = allLetters[i];
            allLetters[i] = allLetters[j];
            allLetters[j] = temp;
        }

        StartCoroutine(ArrangeLetters(cols, rows));
    }

    private IEnumerator ArrangeLetters(int cols, int rows)
    {
        // Центр сетки — текущая позиция самого объекта
        Vector3 centerPos = this.transform.position;

        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        foreach (Transform letter in allLetters)
        {
            startPositions[letter] = letter.position;
        }

        // Сетка в плоскости XZ
        float totalWidth = cols * cellSize.x + (cols > 1 ? (cols - 1) * spacing.x : 0);
        float totalDepth = rows * cellSize.y + (rows > 1 ? (rows - 1) * spacing.y : 0);

        // Смещаем сетку так, чтобы её центр совпадал с позицией объекта
        float startX = centerPos.x - totalWidth / 2f + cellSize.x / 2f;
        float startZ = centerPos.z - totalDepth / 2f + cellSize.y / 2f;

        // Высота сетки — на уровне самого объекта
        float targetY = centerPos.y;

        Dictionary<Transform, Vector3> targetPositions = new Dictionary<Transform, Vector3>();

        for (int i = 0; i < allLetters.Count; i++)
        {
            Transform letter = allLetters[i];
            int col = i % cols;
            int row = i / cols;

            float targetX = startX + col * (cellSize.x + spacing.x);
            float targetZ = startZ + row * (cellSize.y + spacing.y);

            letter.SetParent(this.transform, true);
            letter.position = startPositions[letter];

            targetPositions[letter] = new Vector3(targetX, targetY, targetZ);
        }

        float time = 0f;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animationDuration);

            foreach (Transform letter in allLetters)
            {
                letter.position = Vector3.Lerp(startPositions[letter], targetPositions[letter], t);
            }
            yield return null;
        }

        foreach (Transform letter in allLetters)
        {
            letter.position = targetPositions[letter];
        }

        // Передаём данные в боевой менеджер
        if (combatManager != null)
        {
            combatManager.InitializeGroups(allLetters, cols, rows, cellSize, spacing);
        }

        // Уведомляем ThirdBossManager о завершении сборки
        if (bossManager != null)
        {
            bossManager.OnGridArrangementComplete();
        }
    }
}