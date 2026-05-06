using System.Collections;
using UnityEngine;

/// <summary>
/// Flying enemy with two attack patterns.
///
/// Pattern 1:
/// - Chooses a random target point on a circle centered on the player.
/// - Moves toward that point using acceleration/deceleration.
/// - Dashes on a fixed timer, regardless of distance from the target point.
/// - Before dashing, pauses and locks onto the player's current position.
/// - Fires two shifting projectiles, then dashes toward the locked player position.
/// - After dashing, quickly repositions toward its current target point for a short period.
/// - After four dashes, switches to Pattern 2.
///
/// Pattern 2:
/// - Rapidly moves to the top point of the circle.
/// - Locks to that point for a pause.
/// - Rotates once around the full circle over time, carrying the enemy with it.
/// - Fires projectiles every set interval during the rotation.
/// - Alternates between a regular single-projectile variant and a fast double-projectile variant.
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

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float stoppingDistance = 0.05f;

    [Header("Dash Attack")]
    public float dashInterval = 2f;
    public float dashPauseDuration = 0.4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public int dashesBeforeOrbitPattern = 4;

    [Header("Post-Dash Reposition")]
    public float postDashRepositionDuration = 1f;
    public float postDashMoveSpeed = 12f;
    public float postDashAcceleration = 80f;
    public float postDashDeceleration = 80f;

    [Header("Dash Contact Check")]
    public float dashDamageCheckRadius = 0.35f;

    [Header("Orbit Attack")]
    public float orbitApproachDuration = 0.35f;
    public float orbitStartPauseDuration = 1f;
    public float orbitDuration = 3f;
    public float orbitProjectileInterval = 0.5f;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public GameObject dashProjectilePrefab;
    public GameObject shiftingProjectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpreadAngle = 20f;

    private bool isActive;
    private bool isPreparingDash;
    private bool isDashing;
    private bool isPostDashRepositioning;
    private bool canDash = true;
    private bool isOrbitPatternRunning;
    private bool useFastOrbitVariant;

    private int completedDashCount;
    private float dashTimer;

    private AttackPattern currentPattern = AttackPattern.DashPattern;

    private Vector2 currentTargetOffset;
    private Vector2 dashDirection;
    private Vector2 currentVelocity;

    private EnemyContactDamage contactDamage;

    public bool CanDealContactDamage => isDashing;

    private void Awake()
    {
        contactDamage = GetComponentInChildren<EnemyContactDamage>();
    }

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

        if (isPreparingDash || isDashing || isPostDashRepositioning)
            return;

        HandleChaseMovement();
        UpdateDashTimer();
    }

    private void TryActivate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= activationRange)
        {
            isActive = true;
            dashTimer = dashInterval;
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

    private void HandlePostDashRepositionMovement()
    {
        Vector2 chosenTarget = GetCurrentCircleTarget();
        Vector2 toTarget = chosenTarget - (Vector2)transform.position;
        Vector2 desiredVelocity = Vector2.zero;

        if (toTarget.magnitude > stoppingDistance)
        {
            desiredVelocity = toTarget.normalized * postDashMoveSpeed;
        }

        float rate = desiredVelocity.sqrMagnitude > 0.001f
            ? postDashAcceleration
            : postDashDeceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            rate * Time.deltaTime
        );

        transform.position += (Vector3)(currentVelocity * Time.deltaTime);
    }

    private void UpdateDashTimer()
    {
        if (!canDash)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
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

        yield return new WaitForSeconds(dashPauseDuration);

        FireDashProjectiles();

        isPreparingDash = false;
        isDashing = true;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            Vector2 previousPosition = transform.position;
            Vector2 movement = dashDirection * dashSpeed * Time.deltaTime;
            Vector2 nextPosition = previousPosition + movement;

            CheckDashContactDamage(previousPosition, nextPosition);

            transform.position = nextPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        currentVelocity = Vector2.zero;

        completedDashCount++;

        if (completedDashCount >= dashesBeforeOrbitPattern)
        {
            currentPattern = AttackPattern.OrbitPattern;
            dashTimer = dashInterval;
            canDash = true;
            yield break;
        }

        PickRandomCircleTarget();

        yield return StartCoroutine(PostDashRepositionSequence());

        dashTimer = dashInterval;
        canDash = true;
    }

    private IEnumerator PostDashRepositionSequence()
    {
        isPostDashRepositioning = true;

        float elapsed = 0f;

        while (elapsed < postDashRepositionDuration)
        {
            HandlePostDashRepositionMovement();

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;
        isPostDashRepositioning = false;
    }

    private void CheckDashContactDamage(Vector2 previousPosition, Vector2 nextPosition)
    {
        if (contactDamage == null)
            return;

        Vector2 travel = nextPosition - previousPosition;
        float distance = travel.magnitude;

        if (distance <= 0f)
            return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            previousPosition,
            dashDamageCheckRadius,
            travel.normalized,
            distance
        );

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerHealth playerHealth = hits[i].collider.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                continue;

            contactDamage.TryDamagePlayer(playerHealth);
        }
    }

    private IEnumerator OrbitPatternSequence()
    {
        isOrbitPatternRunning = true;

        currentVelocity = Vector2.zero;

        Vector2 topOffset = Vector2.up * circleRadius;
        Vector2 startPosition = transform.position;

        float approachElapsed = 0f;

        while (approachElapsed < orbitApproachDuration)
        {
            if (player == null)
                break;

            Vector2 targetPosition = (Vector2)player.position + topOffset;
            float t = Mathf.Clamp01(approachElapsed / orbitApproachDuration);

            transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            approachElapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            transform.position = (Vector2)player.position + topOffset;
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
                FireOrbitProjectilePattern();
            }

            orbitElapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            transform.position = (Vector2)player.position + topOffset;
        }

        completedDashCount = 0;
        dashTimer = dashInterval;

        useFastOrbitVariant = !useFastOrbitVariant;

        PickRandomCircleTarget();

        currentPattern = AttackPattern.DashPattern;
        isOrbitPatternRunning = false;
        canDash = true;
    }

    private void FireDashProjectiles()
    {
        if (shiftingProjectilePrefab == null || projectileSpawnPoint == null)
            return;

        Vector2 upperDirection = RotateVector(dashDirection, projectileSpreadAngle);
        Vector2 lowerDirection = RotateVector(dashDirection, -projectileSpreadAngle);

        SpawnProjectile(shiftingProjectilePrefab, upperDirection);
        SpawnProjectile(shiftingProjectilePrefab, lowerDirection);
    }

    private void FireOrbitProjectilePattern()
    {
        if (useFastOrbitVariant)
        {
            FireFastOrbitProjectilesAtPlayer();
        }
        else
        {
            FireProjectileAtPlayer();
        }
    }

    private void FireFastOrbitProjectilesAtPlayer()
    {
        if (dashProjectilePrefab == null || projectileSpawnPoint == null || player == null)
            return;

        Vector2 direction = ((Vector2)player.position - (Vector2)projectileSpawnPoint.position).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        Vector2 upperDirection = RotateVector(direction, projectileSpreadAngle);
        Vector2 lowerDirection = RotateVector(direction, -projectileSpreadAngle);

        SpawnProjectile(dashProjectilePrefab, upperDirection);
        SpawnProjectile(dashProjectilePrefab, lowerDirection);
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

        SpawnProjectile(projectilePrefab, direction);
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

    private void SpawnProjectile(GameObject prefabToSpawn, Vector2 direction)
    {
        GameObject projectileObject = Instantiate(
            prefabToSpawn,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        EnemyProjectile regularProjectile = projectileObject.GetComponent<EnemyProjectile>();
        if (regularProjectile != null)
        {
            regularProjectile.Initialize(direction);
            return;
        }

        EnemySpeedShiftProjectile shiftingProjectile = projectileObject.GetComponent<EnemySpeedShiftProjectile>();
        if (shiftingProjectile != null)
        {
            shiftingProjectile.Initialize(direction);
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

        Vector2 topPoint = circleCenter + Vector2.up * circleRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(topPoint, 0.3f);
        Gizmos.DrawLine(transform.position, topPoint);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashDamageCheckRadius);

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