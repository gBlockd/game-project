using System.Collections;
using UnityEngine;

/// <summary>
/// Simple ranged flying enemy.
///
/// Behavior:
/// - Starts idle until the player enters activation range.
/// - Once active, moves toward a randomly chosen point above-left or above-right of the player.
/// - Uses acceleration/deceleration so movement has some weight.
/// - Periodically switches its target side at a random interval.
/// - When within attack range of the player, pauses and fires a projectile toward the player's current position.
/// - After firing, resumes chase behavior.
/// - Can be temporarily frozen by knockback, unless currently preparing or firing.
/// 
/// This enemy ignores physics and collision and moves by directly changing
/// its transform position.
/// </summary>
public class RangedFlyingEnemy : MonoBehaviour, IFlyingEnemyMovement
{
    [Header("Activation")]
    public float activationRange = 10f;

    [Header("Target")]
    public Transform player;
    public float sideOffset = 2f;
    public float heightOffset = 2f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float stoppingDistance = 0.05f;

    [Header("Target Switching")]
    public float minTargetSwitchTime = 2f;
    public float maxTargetSwitchTime = 5f;

    [Header("Ranged Attack")]
    public float attackRange = 4f;
    public float attackPauseDuration = 0.4f;
    public float attackCooldown = 1.5f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    private bool isActive;
    private bool isPreparingAttack;
    private bool isAttacking;
    private bool isFrozen;
    private bool canAttack = true;
    private bool targetingRightSide;

    private Vector2 currentVelocity;
    private Coroutine targetSwitchCoroutine;
    private Camera mainCamera;

    public bool CanReceiveKnockback => !isPreparingAttack && !isAttacking && !isFrozen;
    public Vector2 CurrentVelocity => currentVelocity;

    private void Awake()
    {
        mainCamera = Camera.main;
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

        if (isFrozen || isPreparingAttack || isAttacking)
            return;

        TryStartAttack();

        if (!isPreparingAttack && !isAttacking)
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
            PickRandomTargetSide();

            if (targetSwitchCoroutine == null)
            {
                targetSwitchCoroutine = StartCoroutine(TargetSwitchRoutine());
            }
        }
    }

    private void HandleChaseMovement()
    {
        Vector2 chosenTarget = targetingRightSide
            ? (Vector2)player.position + new Vector2(sideOffset, heightOffset)
            : (Vector2)player.position + new Vector2(-sideOffset, heightOffset);

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

    private void TryStartAttack()
    {
        if (!canAttack)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator TargetSwitchRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTargetSwitchTime, maxTargetSwitchTime);
            yield return new WaitForSeconds(waitTime);

            PickRandomTargetSide();
        }
    }

    private void PickRandomTargetSide()
    {
        targetingRightSide = Random.value > 0.5f;
    }

    private IEnumerator AttackSequence()
    {
        canAttack = false;
        isPreparingAttack = true;

        currentVelocity = Vector2.zero;

        yield return new WaitForSeconds(attackPauseDuration);

        isPreparingAttack = false;
        isAttacking = true;

        FireProjectileAtPlayer();

        isAttacking = false;
        currentVelocity = Vector2.zero;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void FireProjectileAtPlayer()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || player == null)
            return;

        Vector2 direction = ((Vector2)player.position - (Vector2)projectileSpawnPoint.position).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.left;
        }

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

        Vector2 leftTarget = (Vector2)player.position + new Vector2(-sideOffset, heightOffset);
        Vector2 rightTarget = (Vector2)player.position + new Vector2(sideOffset, heightOffset);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftTarget, 0.2f);
        Gizmos.DrawWireSphere(rightTarget, 0.2f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}