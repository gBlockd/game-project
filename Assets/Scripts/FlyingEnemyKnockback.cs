using System.Collections;
using UnityEngine;

/// <summary>
/// Applies a brief knockback to flying enemies hit by melee.
/// 
/// Behavior:
/// - freezes movement temporarily,
/// - pushes the enemy a short distance away from the attacker,
/// - holds the enemy still for a short beat after the shove finishes,
/// - restores the enemy's previous momentum afterward,
/// - respects knockback immunity from the movement script.
/// </summary>
public class FlyingEnemyKnockback : MonoBehaviour
{
    [Header("Knockback")]
    public float knockbackDistance = 1f;
    public float knockbackDuration = 0.08f;
    public float postKnockbackFreezeDuration = 0.15f;

    private IFlyingEnemyMovement flyingEnemyMovement;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        flyingEnemyMovement = GetComponent<IFlyingEnemyMovement>();
    }

    public void ApplyKnockback(Vector2 attackerPosition)
    {
        if (flyingEnemyMovement == null)
            return;

        if (!flyingEnemyMovement.CanReceiveKnockback)
            return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(attackerPosition));
    }

    private IEnumerator KnockbackRoutine(Vector2 attackerPosition)
    {
        Vector2 savedVelocity = flyingEnemyMovement.CurrentVelocity;
        Vector2 startPosition = transform.position;

        Vector2 direction = ((Vector2)transform.position - attackerPosition).normalized;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        Vector2 targetPosition = startPosition + direction * knockbackDistance;

        flyingEnemyMovement.FreezeMovement();

        float moveDuration = Mathf.Max(0f, knockbackDuration);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = moveDuration > 0f ? elapsed / moveDuration : 1f;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        float freezeDuration = Mathf.Max(0f, postKnockbackFreezeDuration);
        if (freezeDuration > 0f)
        {
            yield return new WaitForSeconds(freezeDuration);
        }

        flyingEnemyMovement.UnfreezeMovement(savedVelocity);
        knockbackCoroutine = null;
    }
}