using UnityEngine;

/// <summary>
/// Handles health logic for the player.
/// 
/// Responsibilities:
/// - tracks current and maximum health,
/// - applies damage and healing,
/// - clamps health within valid bounds.
/// 
/// Note:
/// This script currently does not handle death, UI updates, or visual feedback.
/// Those systems can be layered on later.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    // Current health value (clamped between 0 and maxHealth).
    private int currentHealth;

    // Public read-only accessors for external systems (UI, combat, etc.).
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    /// <summary>
    /// Initializes player health to maximum.
    /// </summary>
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to the player.
    /// 
    /// - Reduces health,
    /// - clamps to a minimum of 0.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
    }

    /// <summary>
    /// Heals the player.
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
}