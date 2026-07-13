using System;
using UnityEngine;

public static class EventManager
{
    public static event Action OnBorderHit;
    public static event Action<GameObject, float> OnEnemyHit;

    public static event Action<CorridorTrigger> OnCorridorAnimation;
    public static event Action OnRoomEntered;
    public static event Action OnBossDied;
    public static event Action OnRoomCleared;

    public static event Action OnBossRoomEntered;
    public static event Action OnBossRoomExited;
    public static void TriggerBorderHit()
    {
        OnBorderHit?.Invoke();
    }

    public static void TriggerEnemyHit(GameObject enemy, float damage)
    {
        OnEnemyHit?.Invoke(enemy, damage);
    }

    public static void TriggerCorridorAnimation(CorridorTrigger trigger)
    {
        OnCorridorAnimation?.Invoke(trigger);
        
    }

    

    public static void TriggerRoomEntered()
    {
        OnRoomEntered?.Invoke();
    }

    public static void TriggerBossRoomEntered() => OnBossRoomEntered?.Invoke();
    public static void TriggerBossRoomExited() => OnBossRoomExited?.Invoke();
    public static void TriggerBossDied() => OnBossDied?.Invoke();

    public static void TriggerRoomCleared() => OnRoomCleared?.Invoke();
}