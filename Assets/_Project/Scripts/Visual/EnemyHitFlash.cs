using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    [SerializeField] private Renderer meshRenderer; 
    private Color originalColor;
    private EnemyHealth health;

    private void Awake()
    {
        //meshRenderer = GetComponent<Renderer>(); 
        health = GetComponent<EnemyHealth>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged += Flash;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= Flash;
    }

    private void Flash(float damage)
    {
        if (meshRenderer == null) return;
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        meshRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        meshRenderer.material.color = originalColor;
    }
}