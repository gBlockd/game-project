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
/// - refreshes projectile cooldown when a marked enemy is hit by melee,
/// - heals the player when a marked enemy is hit by melee,
/// - activates checkpoints hit by the player's melee attack,
/// - activates buttons hit by the player's melee attack.
/// </summary>
public class AttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    public int markedBonusDamage = 30;

    [Header("Marked Hit Reward")]
    public int markedHitHealing = 5;

    [Header("References")]
    public PlayerProjectileAttack playerProjectileAttack;

    private readonly HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();
    private readonly HashSet<Checkpoint> checkpointsHit = new HashSet<Checkpoint>();
    private readonly HashSet<DoorButton> buttonsHit = new HashSet<DoorButton>();

    private Collider2D hitboxCollider;
    private Transform ownerTransform;
    private PlayerHealth ownerHealth;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Assigns the player references used by detached hitboxes, such as ranged melee attacks.
    /// Regular child hitboxes can still fall back to GetComponentInParent.
    /// </summary>
    public void ConfigureOwner(Transform newOwnerTransform, PlayerHealth newOwnerHealth, PlayerProjectileAttack newPlayerProjectileAttack)
    {
        ownerTransform = newOwnerTransform;
        ownerHealth = newOwnerHealth;

        if (newPlayerProjectileAttack != null)
        {
            playerProjectileAttack = newPlayerProjectileAttack;
        }
    }

    private void OnEnable()
    {
        enemiesHit.Clear();
        checkpointsHit.Clear();
        buttonsHit.Clear();
        CheckForOverlappingObjects();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryInteract(other);
    }

    private void CheckForOverlappingObjects()
    {
        if (hitboxCollider == null)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int count = hitboxCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            TryInteract(results[i]);
        }
    }

    private void TryInteract(Collider2D other)
    {
        TryActivateCheckpoint(other);
        TryActivateButton(other);
        TryDamageEnemy(other);
    }

    private void TryActivateCheckpoint(Collider2D other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint == null)
            return;

        if (checkpointsHit.Contains(checkpoint))
            return;

        PlayerHealth playerHealth = GetOwnerHealth();
        if (playerHealth == null)
            return;

        checkpoint.Activate(playerHealth);
        checkpointsHit.Add(checkpoint);
    }

    private void TryActivateButton(Collider2D other)
    {
        DoorButton button = other.GetComponent<DoorButton>();
        if (button == null)
            return;

        if (buttonsHit.Contains(button))
            return;

        button.Activate();
        buttonsHit.Add(button);
    }

    private void TryDamageEnemy(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy == null)
            return;

        if (enemiesHit.Contains(enemy))
            return;

        Vector2 playerPosition = GetOwnerPosition();

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
            }

            PlayerHealth playerHealth = GetOwnerHealth();
            if (playerHealth != null)
            {
                playerHealth.Heal(markedHitHealing);
            }
        }
    }

    private PlayerHealth GetOwnerHealth()
    {
        if (ownerHealth != null)
            return ownerHealth;

        return GetComponentInParent<PlayerHealth>();
    }

    private Vector2 GetOwnerPosition()
    {
        if (ownerTransform != null)
            return ownerTransform.position;

        if (transform.parent != null)
            return transform.parent.position;

        return transform.position;
    }
}
