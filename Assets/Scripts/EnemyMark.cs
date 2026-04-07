using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the "mark" status effect on an enemy.
/// 
/// A mark:
/// - is applied for a limited duration,
/// - can be consumed early by other systems (e.g., melee attacks),
/// - expires automatically if not consumed.
/// 
/// This script does not define what the mark does visually or mechanically,
/// only its lifecycle and state.
/// </summary>
public class EnemyMark : MonoBehaviour
{
    // Public read-only access to mark state.
    public bool IsMarked => isMarked;

    // Internal state tracking whether the enemy is currently marked.
    private bool isMarked;

    // Tracks the active mark coroutine to prevent overlapping timers.
    private Coroutine markCoroutine;

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

        isMarked = false;
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
        isMarked = true;

        yield return new WaitForSeconds(duration);

        isMarked = false;
        markCoroutine = null;
    }
}