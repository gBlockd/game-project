using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles melee hit detection for the player's attack hitbox.
/// 
/// This script:
/// - damages enemies that overlap the hitbox,
/// - prevents the same enemy from being hit multiple times during one attack,
/// - applies knockback to eligible enemies,
/// - consumes enemy marks when present,
/// - applies bonus damage for consuming a mark,
/// - refreshes projectile cooldown when a marked enemy is hit by melee.
/// </summary>
public class AttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    public int markedBonusDamage = 30;

    [Header("References")]
    public PlayerProjectileAttack playerProjectileAttack;

    private readonly HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();
    private Collider2D hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        enemiesHit.Clear();
        CheckForOverlappingEnemies();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other);
    }

    private void CheckForOverlappingEnemies()
    {
        if (hitboxCollider == null)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int count = hitboxCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            TryDamageEnemy(results[i]);
        }
    }

    private void TryDamageEnemy(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy == null)
            return;

        if (enemiesHit.Contains(enemy))
            return;

        Vector2 playerPosition = transform.parent.position;

        FlyingEnemyKnockback flyingKnockback = other.GetComponent<FlyingEnemyKnockback>();
        if (flyingKnockback != null)
        {
            flyingKnockback.ApplyKnockback(playerPosition);
        }

        CrawlerEnemyKnockback crawlerKnockback = other.GetComponent<CrawlerEnemyKnockback>();
        if (crawlerKnockback != null)
        {
            crawlerKnockback.ApplyKnockback(playerPosition);
        }

        enemy.TakeDamage(damage);
        enemiesHit.Add(enemy);

        EnemyMark enemyMark = other.GetComponent<EnemyMark>();
        if (enemyMark != null && enemyMark.ConsumeMark())
        {
            enemy.TakeDamage(markedBonusDamage);

            if (playerProjectileAttack != null)
            {
                playerProjectileAttack.RefreshProjectileCooldown();
                Debug.Log("Marked enemy hit by melee. Projectile cooldown refreshed.");
            }
        }
    }
}