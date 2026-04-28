using System.Collections;
using UnityEngine;

/// <summary>
/// Flying enemy with two attack patterns.
///
/// Pattern 1:
/// - Chooses a random target point on a circle centered on the player.
/// - Moves toward that point using acceleration/deceleration.
/// - When close enough, pauses, fires two spread projectiles, then dashes toward
///   the player's locked position.
/// - After four dashes, switches to Pattern 2.
///
/// Pattern 2:
/// - Moves directly to the top point of the circle.
/// - Locks to that point for a pause.
/// - Rotates once around the full circle over time, carrying the enemy with it.
/// - Fires directly at the player every set interval during the rotation.
/// - After the full circle, returns to Pattern 1.
///
/// This enemy does not receive knockback.
/// Contact damage should only be active while dashing.
/// </summary>
public class ProjectileChargerEnemy : MonoBehaviour
{
    private enum AttackPattern
    {
        DashPattern,
        OrbitPattern
    }

    [Header("Activation")]
    public float activationRange = 10f;

    [Header("Target")]
    public Transform player;
    public float circleRadius = 4f;
    public float targetReachDistance = 0.25f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float stoppingDistance = 0.05f;

    [Header("Dash Attack")]
    public float dashPauseDuration = 0.4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.2f;
    public int dashesBeforeOrbitPattern = 4;

    [Header("Orbit Attack")]
    public float orbitMoveSpeed = 6f;
    public float orbitStartPauseDuration = 1f;
    public float orbitDuration = 3f;
    public float orbitProjectileInterval = 0.5f;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpreadAngle = 20f;

    private bool isActive;
    private bool isPreparingDash;
    private bool isDashing;
    private bool canDash = true;
    private bool isOrbitPatternRunning;

    private int completedDashCount;

    private AttackPattern currentPattern = AttackPattern.DashPattern;

    private Vector2 currentTargetOffset;
    private Vector2 dashDirection;
    private Vector2 currentVelocity;

    public bool CanDealContactDamage => isDashing;

    private void Update()
    {
        if (player == null)
            return;

        if (!isActive)
        {
            TryActivate();
            return;
        }

        if (currentPattern == AttackPattern.OrbitPattern)
        {
            if (!isOrbitPatternRunning)
            {
                StartCoroutine(OrbitPatternSequence());
            }

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
            PickRandomCircleTarget();
        }
    }

    private void HandleChaseMovement()
    {
        Vector2 chosenTarget = GetCurrentCircleTarget();
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

    private void TryStartDash()
    {
        if (!canDash)
            return;

        Vector2 chosenTarget = GetCurrentCircleTarget();
        float distanceToTarget = Vector2.Distance(transform.position, chosenTarget);

        if (distanceToTarget <= targetReachDistance)
        {
            StartCoroutine(DashSequence());
        }
    }

    private Vector2 GetCurrentCircleTarget()
    {
        return (Vector2)player.position + currentTargetOffset;
    }

    private void PickRandomCircleTarget()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        currentTargetOffset = new Vector2(
            Mathf.Cos(randomAngle),
            Mathf.Sin(randomAngle)
        ) * circleRadius;
    }

    private IEnumerator DashSequence()
    {
        canDash = false;
        isPreparingDash = true;

        currentVelocity = Vector2.zero;

        Vector2 lockedPlayerPosition = player.position;
        Vector2 toLockedPlayerPosition = lockedPlayerPosition - (Vector2)transform.position;

        dashDirection = toLockedPlayerPosition.normalized;

        if (dashDirection.sqrMagnitude < 0.0001f)
        {
            dashDirection = Vector2.right;
        }

        FireDashProjectiles();

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

        completedDashCount++;

        if (completedDashCount >= dashesBeforeOrbitPattern)
        {
            currentPattern = AttackPattern.OrbitPattern;
        }
        else
        {
            PickRandomCircleTarget();
        }

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private IEnumerator OrbitPatternSequence()
    {
        isOrbitPatternRunning = true;

        currentVelocity = Vector2.zero;

        Vector2 topOffset = Vector2.up * circleRadius;

        while (player != null)
        {
            Vector2 targetPosition = (Vector2)player.position + topOffset;
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distanceToTarget <= targetReachDistance)
                break;

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                orbitMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        float pauseElapsed = 0f;

        while (pauseElapsed < orbitStartPauseDuration)
        {
            if (player != null)
            {
                transform.position = (Vector2)player.position + topOffset;
            }

            pauseElapsed += Time.deltaTime;
            yield return null;
        }

        float orbitElapsed = 0f;
        float projectileTimer = 0f;

        while (orbitElapsed < orbitDuration)
        {
            if (player == null)
                break;

            float t = orbitElapsed / orbitDuration;
            float angle = Mathf.Lerp(90f, 90f - 360f, t);
            float radians = angle * Mathf.Deg2Rad;

            Vector2 orbitOffset = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            ) * circleRadius;

            transform.position = (Vector2)player.position + orbitOffset;

            projectileTimer += Time.deltaTime;

            if (projectileTimer >= orbitProjectileInterval)
            {
                projectileTimer = 0f;
                FireProjectileAtPlayer();
            }

            orbitElapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            transform.position = (Vector2)player.position + topOffset;
        }

        completedDashCount = 0;
        PickRandomCircleTarget();

        currentPattern = AttackPattern.DashPattern;
        isOrbitPatternRunning = false;
    }

    private void FireDashProjectiles()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return;

        Vector2 upperDirection = RotateVector(dashDirection, projectileSpreadAngle);
        Vector2 lowerDirection = RotateVector(dashDirection, -projectileSpreadAngle);

        SpawnProjectile(upperDirection);
        SpawnProjectile(lowerDirection);
    }

    private void FireProjectileAtPlayer()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || player == null)
            return;

        Vector2 direction = ((Vector2)player.position - (Vector2)projectileSpawnPoint.position).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        SpawnProjectile(direction);
    }

    private Vector2 RotateVector(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Vector2 circleCenter = player.position;
        Vector2 currentTarget = Application.isPlaying
            ? GetCurrentCircleTarget()
            : circleCenter + Vector2.right * circleRadius;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(circleCenter, circleRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(currentTarget, 0.25f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, currentTarget);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTarget, targetReachDistance);

        Vector2 topPoint = circleCenter + Vector2.up * circleRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(topPoint, 0.3f);
        Gizmos.DrawLine(transform.position, topPoint);

        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;

            Vector2 previewDirection = Application.isPlaying && dashDirection.sqrMagnitude > 0.0001f
                ? dashDirection
                : ((Vector2)player.position - (Vector2)transform.position).normalized;

            if (previewDirection.sqrMagnitude < 0.0001f)
            {
                previewDirection = Vector2.right;
            }

            Vector2 previewUp = RotateVector(previewDirection, projectileSpreadAngle);
            Vector2 previewDown = RotateVector(previewDirection, -projectileSpreadAngle);

            Gizmos.DrawLine(projectileSpawnPoint.position, projectileSpawnPoint.position + (Vector3)(previewUp * 1.5f));
            Gizmos.DrawLine(projectileSpawnPoint.position, projectileSpawnPoint.position + (Vector3)(previewDown * 1.5f));
        }
    }
}