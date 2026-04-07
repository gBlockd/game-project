using System.Collections;
using UnityEngine;

/// <summary>
/// Simple enemy movement AI that normally moves toward whichever side of the player
/// is closer, but if it becomes horizontally aligned with the player, it pauses,
/// dashes horizontally toward them, then resumes chasing.
/// 
/// This enemy ignores physics and collision and moves by directly changing
/// its transform position.
/// </summary>
public class SimpleChaseEnemy : MonoBehaviour
{
    [Header("Activation")]
    public float activationRange = 10f;

    [Header("Target")]
    public Transform player;
    public float sideOffset = 2f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stoppingDistance = 0.05f;

    [Header("Dash Attack")]
    public float horizontalAlignmentTolerance = 0.4f;
    public float dashPauseDuration = 0.4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.2f;

    private bool isActive;
    private bool isPreparingDash;
    private bool isDashing;
    private bool canDash = true;
    private Vector2 dashDirection;

    private void Update()
    {
        if (player == null)
            return;

        if (!isActive)
        {
            TryActivate();
            return;
        }

        if (isPreparingDash || isDashing)
            return;

        TryStartDash();

        if (!isPreparingDash && !isDashing)
        {
            HandleChaseMovement();
        }
    }

    private void TryActivate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= activationRange)
        {
            isActive = true;
        }
    }

    /// <summary>
    /// Handles the enemy's normal side-target chasing behavior.
    /// The enemy moves toward whichever point is closer:
    /// a point slightly left of the player or slightly right of the player.
    /// </summary>
    private void HandleChaseMovement()
    {
        Vector2 leftTarget = (Vector2)player.position + Vector2.left * sideOffset;
        Vector2 rightTarget = (Vector2)player.position + Vector2.right * sideOffset;

        float distanceToLeft = Vector2.Distance(transform.position, leftTarget);
        float distanceToRight = Vector2.Distance(transform.position, rightTarget);

        Vector2 chosenTarget = distanceToLeft <= distanceToRight ? leftTarget : rightTarget;

        float distanceToTarget = Vector2.Distance(transform.position, chosenTarget);
        if (distanceToTarget <= stoppingDistance)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            chosenTarget,
            moveSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Checks whether the enemy is horizontally aligned with the player closely enough
    /// to begin its dash sequence.
    /// </summary>
    private void TryStartDash()
    {
        if (!canDash)
            return;

        // Check vertical alignment
        float verticalDifference = Mathf.Abs(transform.position.y - player.position.y);
        if (verticalDifference > horizontalAlignmentTolerance)
            return;

        // Calculate side targets
        Vector2 leftTarget = (Vector2)player.position + Vector2.left * sideOffset;
        Vector2 rightTarget = (Vector2)player.position + Vector2.right * sideOffset;

        float distanceToLeft = Vector2.Distance(transform.position, leftTarget);
        float distanceToRight = Vector2.Distance(transform.position, rightTarget);

        Vector2 chosenTarget = distanceToLeft <= distanceToRight ? leftTarget : rightTarget;

        // Only dash if we've actually reached (or are very close to) that side position
        float distanceToTarget = Vector2.Distance(transform.position, chosenTarget);

        if (distanceToTarget <= stoppingDistance + 0.3f)
        {
            StartCoroutine(DashSequence());
        }
    }

    /// <summary>
    /// Handles the full dash sequence:
    /// - pause,
    /// - dash horizontally toward the player,
    /// - wait out cooldown,
    /// - resume normal chase behavior.
    /// </summary>
    private IEnumerator DashSequence()
    {
        canDash = false;
        isPreparingDash = true;

        // Stop and "wind up" before dashing.
        yield return new WaitForSeconds(dashPauseDuration);

        isPreparingDash = false;
        isDashing = true;

        float directionX = player.position.x >= transform.position.x ? 1f : -1f;
        dashDirection = new Vector2(directionX, 0f);

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Vector2 leftTarget = (Vector2)player.position + Vector2.left * sideOffset;
        Vector2 rightTarget = (Vector2)player.position + Vector2.right * sideOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftTarget, 0.2f);
        Gizmos.DrawWireSphere(rightTarget, 0.2f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, leftTarget);
        Gizmos.DrawLine(transform.position, rightTarget);

        Gizmos.color = Color.red;
        Vector3 upperLine = new Vector3(player.position.x, player.position.y + horizontalAlignmentTolerance, 0f);
        Vector3 lowerLine = new Vector3(player.position.x, player.position.y - horizontalAlignmentTolerance, 0f);
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5f, upperLine.y, 0f),
            new Vector3(player.position.x + 5f, upperLine.y, 0f)
        );
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5f, lowerLine.y, 0f),
            new Vector3(player.position.x + 5f, lowerLine.y, 0f)
        );
    }
}