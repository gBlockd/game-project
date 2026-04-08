using System.Collections;
using UnityEngine;

/// <summary>
/// Applies a brief horizontal-only knockback to crawler enemies hit by melee.
/// 
/// Behavior:
/// - freezes crawler movement temporarily,
/// - pushes the enemy left or right away from the attacker,
/// - keeps the crawler on the horizontal axis only,
/// - restores normal crawling afterward.
/// </summary>
public class CrawlerEnemyKnockback : MonoBehaviour
{
    [Header("Knockback")]
    public float knockbackDistance = 0.75f;
    public float knockbackDuration = 0.08f;

    private CrawlerEnemy crawlerEnemy;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        crawlerEnemy = GetComponent<CrawlerEnemy>();
    }

    public void ApplyKnockback(Vector2 attackerPosition)
    {
        if (crawlerEnemy == null)
            return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(attackerPosition));
    }

    private IEnumerator KnockbackRoutine(Vector2 attackerPosition)
    {
        Vector2 startPosition = transform.position;

        float directionX = transform.position.x >= attackerPosition.x ? 1f : -1f;
        Vector2 targetPosition = new Vector2(
            startPosition.x + directionX * knockbackDistance,
            startPosition.y
        );

        crawlerEnemy.SetFrozen(true);

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        crawlerEnemy.SetFrozen(false);
        knockbackCoroutine = null;
    }
}