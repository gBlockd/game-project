using UnityEngine;

/// <summary>
/// Handles health logic for an enemy.
/// 
/// Responsibilities:
/// - tracks current and maximum health,
/// - applies damage and healing,
/// - clamps health within valid bounds,
/// - triggers visual feedback when damaged,
/// - destroys the enemy when health reaches zero.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;

    // Current health value (clamped between 0 and maxHealth).
    private int currentHealth;

    // Public read-only accessors for external systems.
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // Optional reference for visual damage feedback.
    private EnemyDamageFlash damageFlash;

    /// <summary>
    /// Initializes health to maximum and caches required components.
    /// </summary>
    private void Awake()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<EnemyDamageFlash>();
    }

    /// <summary>
    /// Applies damage to the enemy.
    /// 
    /// - Reduces health,
    /// - clamps to a minimum of 0,
    /// - triggers a visual flash if available,
    /// - destroys the enemy if health reaches zero.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the enemy.
    /// 
    /// - Increases health,
    /// - clamps to a maximum of maxHealth.
    /// </summary>
    /// <param name="amount">Amount of healing to apply.</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    /// <summary>
    /// Removes the enemy from the scene.
    /// </summary>
    private void Die()
    {
        Destroy(gameObject);
    }
}