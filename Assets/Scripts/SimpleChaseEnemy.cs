using System.Collections;
using UnityEngine;

/// <summary>
/// Simple enemy movement AI.
///
/// Behavior:
/// - Starts idle until the player enters activation range.
/// - Once active, moves toward a randomly chosen horizontal point near the player.
/// - Uses acceleration/deceleration so movement has some weight.
/// - Periodically switches its target side at a random interval.
/// - When vertically aligned with the player and positioned between the
///   left/right side offset points, pauses and dashes horizontally.
/// - After dashing, resumes chase behavior.
/// - Can be temporarily frozen by knockback, unless currently preparing a dash or dashing.
/// 
/// This enemy ignores physics and collision and moves by directly changing
/// its transform position.
/// </summary>
public class SimpleChaseEnemy : MonoBehaviour, IFlyingEnemyMovement
{
    [Header("Activation")]
    public float activationRange = 10f;

    [Header("Target")]
    public Transform player;
    public float sideOffset = 2f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float stoppingDistance = 0.05f;

    [Header("Target Switching")]
    public float minTargetSwitchTime = 2f;
    public float maxTargetSwitchTime = 5f;

    [Header("Dash Attack")]
    public float horizontalAlignmentTolerance = 0.4f;
    public float dashPauseDuration = 0.4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.2f;

    private bool isActive;
    private bool isPreparingDash;
    private bool isDashing;
    private bool isFrozen;
    private bool canDash = true;
    private bool targetingRightSide;

    private Vector2 dashDirection;
    private Vector2 currentVelocity;
    private Coroutine targetSwitchCoroutine;

    public bool CanReceiveKnockback => !isPreparingDash && !isDashing && !isFrozen;
    public Vector2 CurrentVelocity => currentVelocity;

    private void Update()
    {
        if (player == null)
            return;

        if (!isActive)
        {
            TryActivate();
            return;
        }

        if (isFrozen || isPreparingDash || isDashing)
            return;

        TryStartDash();

        if (!isPreparingDash && !isDashing)
        {
            HandleChaseMovement();
        }
    }

    /// <summary>
    /// Activates the enemy once the player is close enough.
    /// Also picks an initial random side target and starts the periodic switching behavior.
    /// </summary>
    private void TryActivate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= activationRange)
        {
            isActive = true;
            PickRandomTargetSide();

            if (targetSwitchCoroutine == null)
            {
                targetSwitchCoroutine = StartCoroutine(TargetSwitchRoutine());
            }
        }
    }

    /// <summary>
    /// Handles the enemy's weighted chase movement.
    ///
    /// The enemy moves toward whichever side is currently selected:
    /// - a point to the left of the player
    /// - a point to the right of the player
    ///
    /// Horizontal and vertical motion accelerate toward that target instead
    /// of snapping instantly, giving the enemy more momentum and weight.
    /// </summary>
    private void HandleChaseMovement()
    {
        Vector2 chosenTarget = targetingRightSide
            ? (Vector2)player.position + Vector2.right * sideOffset
            : (Vector2)player.position + Vector2.left * sideOffset;

        Vector2 toTarget = chosenTarget - (Vector2)transform.position;
        Vector2 desiredVelocity = Vector2.zero;

        if (toTarget.magnitude > stoppingDistance)
        {
            desiredVelocity = toTarget.normalized * moveSpeed;
        }

        float rate = desiredVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            rate * Time.deltaTime
        );

        transform.position += (Vector3)(currentVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Checks whether the enemy should begin its dash sequence.
    ///
    /// Dash requirements:
    /// - dash is available,
    /// - enemy is vertically aligned with the player,
    /// - enemy is horizontally between the player's left/right side offset points.
    /// </summary>
    private void TryStartDash()
    {
        if (!canDash)
            return;

        float verticalDifference = Mathf.Abs(transform.position.y - player.position.y);
        if (verticalDifference > horizontalAlignmentTolerance)
            return;

        float leftBoundary = player.position.x - sideOffset;
        float rightBoundary = player.position.x + sideOffset;
        float enemyX = transform.position.x;

        bool isBetweenPoints = enemyX >= leftBoundary && enemyX <= rightBoundary;

        if (isBetweenPoints)
        {
            StartCoroutine(DashSequence());
        }
    }

    /// <summary>
    /// Repeatedly chooses a new random target side at a random interval.
    /// This makes the enemy's movement less predictable and more dynamic.
    /// </summary>
    private IEnumerator TargetSwitchRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTargetSwitchTime, maxTargetSwitchTime);
            yield return new WaitForSeconds(waitTime);

            PickRandomTargetSide();
        }
    }

    /// <summary>
    /// Randomly selects whether the enemy should target the left or right side of the player.
    /// </summary>
    private void PickRandomTargetSide()
    {
        targetingRightSide = Random.value > 0.5f;
    }

    /// <summary>
    /// Handles the full dash sequence:
    /// - stop and pause,
    /// - dash horizontally toward the locked player-side direction,
    /// - stop the dash,
    /// - wait out cooldown,
    /// - resume chase behavior.
    /// </summary>
    private IEnumerator DashSequence()
    {
        canDash = false;
        isPreparingDash = true;

        currentVelocity = Vector2.zero;

        // Lock the dash direction at the start of the wind-up.
        float directionX = player.position.x >= transform.position.x ? 1f : -1f;
        dashDirection = new Vector2(directionX, 0f);

        yield return new WaitForSeconds(dashPauseDuration);

        isPreparingDash = false;
        isDashing = true;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        currentVelocity = Vector2.zero;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    public void FreezeMovement()
    {
        isFrozen = true;
        currentVelocity = Vector2.zero;
    }

    public void UnfreezeMovement(Vector2 restoredVelocity)
    {
        currentVelocity = restoredVelocity;
        isFrozen = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Vector2 leftTarget = (Vector2)player.position + Vector2.left * sideOffset;
        Vector2 rightTarget = (Vector2)player.position + Vector2.right * sideOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftTarget, 0.2f);
        Gizmos.DrawWireSphere(rightTarget, 0.2f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, leftTarget);
        Gizmos.DrawLine(transform.position, rightTarget);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5f, player.position.y + horizontalAlignmentTolerance, 0f),
            new Vector3(player.position.x + 5f, player.position.y + horizontalAlignmentTolerance, 0f)
        );
        Gizmos.DrawLine(
            new Vector3(player.position.x - 5f, player.position.y - horizontalAlignmentTolerance, 0f),
            new Vector3(player.position.x + 5f, player.position.y - horizontalAlignmentTolerance, 0f)
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(player.position.x - sideOffset, player.position.y - 3f, 0f),
            new Vector3(player.position.x - sideOffset, player.position.y + 3f, 0f)
        );
        Gizmos.DrawLine(
            new Vector3(player.position.x + sideOffset, player.position.y - 3f, 0f),
            new Vector3(player.position.x + sideOffset, player.position.y + 3f, 0f)
        );
    }
}