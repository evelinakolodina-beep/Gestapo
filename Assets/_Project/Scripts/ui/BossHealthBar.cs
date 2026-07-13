using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BossHealthBar : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Image currentFill;
    [SerializeField] private Image delayedFill;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Camera mainCamera;

    [Header("Настройки анимации")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float delayedTime = 0.8f;
    [SerializeField] private Ease animationEase = Ease.OutQuad;
    [SerializeField] private float appearDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.4f;

    [Header("Billboard Settings")]
    [SerializeField] private bool faceCamera = true;

    private EnemyHealth _currentBoss;
    private bool _isVisible;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Убираем фиксацию масштаба - теперь размер настраивается через RectTransform
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (faceCamera && mainCamera != null && _isVisible)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    public void Show(EnemyHealth boss)
    {
        if (boss == null)
        {
            Debug.LogError("[BossHealthBar] Попытка показать шкалу с null боссом!");
            return;
        }

        UnsubscribeFromBoss();

        _currentBoss = boss;
        _isVisible = true;

        _currentBoss.OnDamaged += OnBossDamaged;

        currentFill.fillAmount = 1f;
        delayedFill.fillAmount = 1f;

        // Только fade анимация, без scale
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, appearDuration);
        }

        Debug.Log($"[BossHealthBar] Показана шкала для {_currentBoss.gameObject.name}");
    }

    private void OnBossDamaged(float damage)
    {
        if (_currentBoss == null || !_isVisible) return;

        float normalized = _currentBoss.CurrentHealthNormalized;

        currentFill.DOKill();
        delayedFill.DOKill();

        currentFill.DOFillAmount(normalized, smoothTime).SetEase(animationEase);
        delayedFill.DOFillAmount(normalized, delayedTime).SetEase(Ease.OutSine);
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;

        UnsubscribeFromBoss();

        // Только fade анимация
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, hideDuration)
                .OnComplete(() =>
                {
                    ResetState();
                });
        }
        else
        {
            ResetState();
        }
    }

    private void UnsubscribeFromBoss()
    {
        if (_currentBoss != null)
        {
            _currentBoss.OnDamaged -= OnBossDamaged;
            _currentBoss = null;
        }
    }

    public void ResetState()
    {
        currentFill.fillAmount = 1f;
        delayedFill.fillAmount = 1f;
        currentFill.DOKill();
        delayedFill.DOKill();
    }

    private void OnDestroy()
    {
        UnsubscribeFromBoss();
    }
}