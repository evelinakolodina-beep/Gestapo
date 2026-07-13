using Unity.Cinemachine;
using UnityEngine;

public class BossCameraController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Настройки зума (FOV)")]
    [SerializeField] private float normalFOV = 60f;  // Обычный FOV
    [SerializeField] private float bossFOV = 40f;    // FOV в комнате босса (меньше = камера дальше)

    [Header("Плавность")]
    [SerializeField] private float transitionTime = 1.5f;

    private float targetFOV;
    private float currentFOV;
    private float fovVelocity;

    private void Start()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = FindObjectOfType<CinemachineCamera>();

        currentFOV = normalFOV;
        targetFOV = normalFOV;
        cinemachineCamera.Lens.FieldOfView = currentFOV;

        EventManager.OnBossRoomEntered += OnBossRoomEnter;
        EventManager.OnBossRoomExited += OnBossRoomExit;
    }

    private void OnDestroy()
    {
        EventManager.OnBossRoomEntered -= OnBossRoomEnter;
        EventManager.OnBossRoomExited -= OnBossRoomExit;
    }

    private void OnBossRoomEnter() => targetFOV = bossFOV;
    private void OnBossRoomExit() => targetFOV = normalFOV;

    private void Update()
    {
        if (cinemachineCamera == null) return;

        currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref fovVelocity, transitionTime);
        cinemachineCamera.Lens.FieldOfView = currentFOV;
    }
}