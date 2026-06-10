using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a visual flash effect for buttons when struck by the player's attack.
///
/// The flash confirms that the melee hitbox reached the button, even if the
/// button was already activated and will not open anything new.
/// </summary>
public class ButtonFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;
    public float flashDuration = 0.1f;

    // Sprite renderer whose color is changed during the flash.
    private SpriteRenderer spriteRenderer;

    // The normal sprite color to restore after the flash ends.
    private Color originalColor;

    // Tracks the current flash so repeated hits restart the effect cleanly.
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
    /// Starts or restarts the button flash effect.
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
