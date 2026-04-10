using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a visual flash effect for checkpoints when activated.
/// </summary>
public class CheckpointFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.green;
    public float flashDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Flash()
    {
        if (spriteRenderer == null)
            return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }
}