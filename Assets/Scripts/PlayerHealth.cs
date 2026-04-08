using UnityEngine;

/// <summary>
/// Handles health logic for the player.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private PlayerDamageFlash damageFlash;

    private void Awake()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<PlayerDamageFlash>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}