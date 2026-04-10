using System.Collections;
using UnityEngine;

/// <summary>
/// Simple ranged flying enemy.
///
/// Behavior:
/// - Starts idle until the player enters activation range.
/// - Once active, moves toward a single hover point above the player.
/// - The hover point's horizontal offset is randomly re-rolled every 2 seconds.
/// - Uses acceleration/deceleration so movement has some weight.
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
    public float heightOffset = 2f;
    public float minSideOffset = -5f;
    public float maxSideOffset = 5f;
    public float targetSwitchInterval = 2f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float stoppingDistance = 0.05f;

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

    private float currentSideOffset;
    private Vector2 currentVelocity;
    private Coroutine targetSwitchCoroutine;

    public bool CanReceiveKnockback => !isPreparingAttack && !isAttacking && !isFrozen;
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
            PickRandomSideOffset();

            if (targetSwitchCoroutine == null)
            {
                targetSwitchCoroutine = StartCoroutine(TargetSwitchRoutine());
            }
        }
    }

    private void HandleChaseMovement()
    {
        Vector2 chosenTarget = (Vector2)player.position + new Vector2(currentSideOffset, heightOffset);
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
            yield return new WaitForSeconds(targetSwitchInterval);
            PickRandomSideOffset();
        }
    }

    private void PickRandomSideOffset()
    {
        currentSideOffset = Random.Range(minSideOffset, maxSideOffset);
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

        Vector2 hoverTarget = (Vector2)player.position + new Vector2(currentSideOffset, heightOffset);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hoverTarget, 0.2f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}