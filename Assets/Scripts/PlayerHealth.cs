using UnityEngine;
using System.Collections;

/// <summary>
/// Handles health logic for the player and syncs it with persistent game state.
///
/// Responsibilities:
/// - initializes health from GameStateManager when available,
/// - applies damage and healing,
/// - prevents repeated damage during invincibility frames,
/// - triggers player damage feedback,
/// - starts the respawn flow when health reaches zero,
/// - writes health changes back to GameStateManager.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    [Header("I-Frames")]
    public float invincibilityDuration = 0.5f;

    private int currentHealth;
    private bool isDead;
    private bool isInvincible;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private PlayerDamageFlash damageFlash;

    /// <summary>
    /// Loads health from persistent game state when possible; otherwise starts at full health.
    /// </summary>
    private void Awake()
    {
        damageFlash = GetComponent<PlayerDamageFlash>();

        if (GameStateManager.Instance != null)
        {
            maxHealth = GameStateManager.Instance.maxHealth;
            currentHealth = GameStateManager.Instance.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    /// <summary>
    /// Applies damage if the player is alive and not currently invincible.
    ///
    /// Damage is clamped at zero, synced to persistent state, and can start the
    /// respawn flow if health reaches zero.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        if (isDead || isInvincible)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        SyncHealthToGameState();

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        StartCoroutine(InvincibilityRoutine());

        if (currentHealth <= 0)
        {
            isDead = true;

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.HandlePlayerDeath(this);
            }
        }
    }

    /// <summary>
    /// Restores health while the player is alive, clamping at max health.
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        SyncHealthToGameState();
    }

    /// <summary>
    /// Fully restores health and clears the dead state after respawn or checkpoint activation.
    /// </summary>
    public void ResetToFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        SyncHealthToGameState();
    }

    /// <summary>
    /// Temporarily blocks additional damage after the player is hit.
    /// </summary>
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    /// <summary>
    /// Copies the local health values into GameStateManager when persistent state exists.
    /// </summary>
    private void SyncHealthToGameState()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}
