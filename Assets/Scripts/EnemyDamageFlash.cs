using System.Collections;
using UnityEngine;

/// <summary>
/// Handles visual damage feedback for an enemy by briefly changing its color.
/// 
/// When Flash() is called:
/// - the enemy's sprite color is set to a flash color,
/// - after a short duration, it returns to its current base color,
/// - any ongoing flash is interrupted and restarted.
/// </summary>
public class EnemyDamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    // Cached reference to the sprite renderer used for color changes.
    private SpriteRenderer spriteRenderer;

    // Cached reference to the mark state, if this enemy can be marked.
    private EnemyMark enemyMark;

    // Stores the original color of the sprite before any flash occurs.
    private Color originalColor;

    // Tracks the currently running flash coroutine to prevent overlap.
    private Coroutine flashCoroutine;

    /// <summary>
    /// Caches the SpriteRenderer and records the original color.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyMark = GetComponent<EnemyMark>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Triggers the damage flash effect.
    /// 
    /// If a flash is already in progress, it is stopped and restarted
    /// so that rapid hits still produce a consistent visual response.
    /// </summary>
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

    /// <summary>
    /// Coroutine that handles the flash timing:
    /// - sets the sprite to the flash color,
    /// - waits for the configured duration,
    /// - restores the correct unflashed color.
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = GetRestoredColor();
        flashCoroutine = null;
    }

    private Color GetRestoredColor()
    {
        if (enemyMark != null && enemyMark.IsMarked)
        {
            return enemyMark.MarkColor;
        }

        return originalColor;
    }
}