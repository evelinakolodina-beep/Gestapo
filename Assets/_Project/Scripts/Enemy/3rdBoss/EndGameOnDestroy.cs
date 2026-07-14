using UnityEngine;

public class EndGameOnDestroy : MonoBehaviour
{
    private void OnDestroy()
    {
        // Проверяем, что игра действительно идет (а не просто сцена выгружается)
        if (Application.isPlaying)
        {
            EventManager.TriggerGameEnded();
        }
    }
}