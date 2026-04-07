using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles melee hit detection for the player's attack hitbox.
/// 
/// This script:
/// - damages enemies that overlap the hitbox,
/// - prevents the same enemy from being hit multiple times during one attack,
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

    // Tracks which enemies have already been hit during the current attack activation.
    private readonly HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();

    // Cached collider used for overlap checks when the hitbox is enabled.
    private Collider2D hitboxCollider;

    /// <summary>
    /// Caches the hitbox collider attached to this object.
    /// </summary>
    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Called whenever the hitbox is activated.
    /// Clears the current attack's hit list, then immediately checks for
    /// enemies already overlapping the hitbox.
    /// 
    /// This ensures attacks still register even if the player is standing still
    /// and the enemy is already inside the hitbox area.
    /// </summary>
    private void OnEnable()
    {
        enemiesHit.Clear();
        CheckForOverlappingEnemies();
    }

    /// <summary>
    /// Called when a collider enters the trigger hitbox.
    /// Attempts to damage the overlapping object if it is a valid enemy.
    /// </summary>
    /// <param name="other">The collider entering the hitbox.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageEnemy(other);
    }

    /// <summary>
    /// Checks for colliders already overlapping the hitbox when it is enabled.
    /// This supports reliable melee hits even when no fresh trigger-enter event occurs.
    /// </summary>
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

    /// <summary>
    /// Attempts to apply melee effects to the given collider.
    /// 
    /// If the collider belongs to an enemy that has not already been hit
    /// during this attack activation:
    /// - base melee damage is applied,
    /// - the enemy is recorded as hit,
    /// - any active mark is consumed,
    /// - bonus damage is applied if a mark was consumed,
    /// - projectile cooldown is refreshed if available.
    /// </summary>
    /// <param name="other">The collider being evaluated for damage.</param>
    private void TryDamageEnemy(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy == null)
            return;

        if (enemiesHit.Contains(enemy))
            return;

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