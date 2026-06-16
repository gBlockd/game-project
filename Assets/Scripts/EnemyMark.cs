using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the "mark" status effect on an enemy.
/// 
/// A mark:
/// - is applied for a limited duration,
/// - can be consumed early by other systems (e.g., melee attacks),
/// - expires automatically if not consumed,
/// - changes the enemy sprite color while active.
/// </summary>
public class EnemyMark : MonoBehaviour
{
    [Header("Visuals")]
    public Color markColor = Color.green;

    // Public read-only access to mark state.
    public bool IsMarked => isMarked;
    public Color MarkColor => markColor;

    // Cached reference to the enemy sprite used for the mark color.
    private SpriteRenderer spriteRenderer;

    // The sprite color to restore when the mark is removed.
    private Color originalColor;

    // Internal state tracking whether the enemy is currently marked.
    private bool isMarked;

    // Tracks the active mark coroutine to prevent overlapping timers.
    private Coroutine markCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Applies a mark to the enemy for the specified duration.
    /// 
    /// If a mark is already active, its timer is reset.
    /// </summary>
    /// <param name="duration">Duration in seconds the mark should remain active.</param>
    public void ApplyMark(float duration)
    {
        if (markCoroutine != null)
        {
            StopCoroutine(markCoroutine);
        }

        markCoroutine = StartCoroutine(MarkRoutine(duration));
    }

    /// <summary>
    /// Attempts to consume the mark.
    /// 
    /// Returns true if a mark was active and successfully consumed.
    /// Returns false if no mark was present.
    /// 
    /// Consuming a mark:
    /// - immediately removes the mark,
    /// - cancels the remaining duration.
    /// </summary>
    public bool ConsumeMark()
    {
        if (!isMarked)
            return false;

        if (markCoroutine != null)
        {
            StopCoroutine(markCoroutine);
            markCoroutine = null;
        }

        SetMarked(false);
        return true;
    }

    /// <summary>
    /// Coroutine that controls the lifetime of the mark.
    /// 
    /// - Activates the mark,
    /// - waits for the specified duration,
    /// - then automatically removes the mark if not consumed.
    /// </summary>
    /// <param name="duration">Duration in seconds before the mark expires.</param>
    private IEnumerator MarkRoutine(float duration)
    {
        SetMarked(true);

        yield return new WaitForSeconds(duration);

        SetMarked(false);
        markCoroutine = null;
    }

    private void SetMarked(bool marked)
    {
        isMarked = marked;

        if (spriteRenderer == null)
            return;

        spriteRenderer.color = isMarked ? markColor : originalColor;
    }
}