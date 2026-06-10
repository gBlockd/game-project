using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a visual flash effect for checkpoints when activated.
///
/// This gives the player immediate feedback that the checkpoint was struck,
/// healed them, and saved a new respawn position.
/// </summary>
public class CheckpointFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.green;
    public float flashDuration = 0.15f;

    // Sprite renderer whose color is changed during the flash.
    private SpriteRenderer spriteRenderer;

    // The normal sprite color to restore after the flash ends.
    private Color originalColor;

    // Tracks the current flash so repeated activations restart the effect cleanly.
    private Coroutine flashCoroutine;

    /// <summary>
    /// Caches the SpriteRenderer and records its starting color when one exists.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Starts or restarts the checkpoint flash effect.
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
