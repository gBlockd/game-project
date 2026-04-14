using UnityEngine;

/// <summary>
/// Handles health logic for the player and syncs it with persistent game state.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private PlayerDamageFlash damageFlash;

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

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        SyncHealthToGameState();

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (currentHealth <= 0)
        {
            isDead = true;

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.HandlePlayerDeath(this);
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        SyncHealthToGameState();
    }

    public void ResetToFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        SyncHealthToGameState();
    }

    private void SyncHealthToGameState()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}