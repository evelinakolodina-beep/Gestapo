using System;
using UnityEngine;

public interface IBossAttack
{
    /// <summary>
    /// Инициализация общих параметров (передача ссылки на игрока от Менеджера)
    /// </summary>
    void Setup(Transform player);

    /// <summary>
    /// Запуск атаки
    /// </summary>
    void Execute();

    /// <summary>
    /// Принудительное прерывание атаки
    /// </summary>
    void Cancel();

    /// <summary>
    /// Событие, которое атаки вызывают, когда они завершили свой цикл или были отменены
    /// </summary>
    event Action OnCompleted;
}