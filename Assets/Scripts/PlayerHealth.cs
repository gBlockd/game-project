using UnityEngine;
using System.Collections;

/// <summary>
/// Handles health logic for the player and syncs it with persistent game state.
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

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    private void SyncHealthToGameState()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}