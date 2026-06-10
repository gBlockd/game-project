using System.Collections;
using UnityEngine;

/// <summary>
/// Handles visual damage feedback for the player by briefly changing its sprite color.
///
/// When Flash() is called:
/// - any current flash is stopped,
/// - the player sprite changes to the flash color,
/// - after a short delay, the original color is restored.
/// </summary>
public class PlayerDamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    // Sprite renderer whose color is changed during the flash.
    private SpriteRenderer spriteRenderer;

    // The normal sprite color to restore after the flash ends.
    private Color originalColor;

    // Tracks the running flash so repeated hits restart the effect cleanly.
    private Coroutine flashCoroutine;

    /// <summary>
    /// Caches the SpriteRenderer and records the color used when not flashing.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Starts or restarts the damage flash effect.
    /// </summary>
    public void Flash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    /// <summary>
    /// Performs the timed color change and then restores the original color.
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }
}
