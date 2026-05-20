using UnityEngine;

/// <summary>
/// Handles health logic for an enemy.
/// 
/// Responsibilities:
/// - tracks current and maximum health,
/// - applies damage and healing,
/// - clamps health within valid bounds,
/// - triggers visual feedback when damaged,
/// - persists death across scene transitions when enemyId is set,
/// - opens linked encounter doors when killed,
/// - destroys the enemy when health reaches zero.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;

    [Header("Persistence")]
    public string enemyId;

    [Header("Encounter Door")]
    public string encounterDoorIdOnDeath;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private EnemyDamageFlash damageFlash;

    private void Awake()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<EnemyDamageFlash>();
    }

    private void Start()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsEnemyKilled(enemyId))
        {
            Destroy(gameObject);
        }
    }

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

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterKilledEnemy(enemyId);
        }

        OpenLinkedEncounterDoors();

        Destroy(gameObject);
    }

    private void OpenLinkedEncounterDoors()
    {
        if (string.IsNullOrWhiteSpace(encounterDoorIdOnDeath))
            return;

        EncounterDoor[] doors = FindObjectsByType<EncounterDoor>();

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].encounterId == encounterDoorIdOnDeath)
            {
                doors[i].OpenDoor();
            }
        }
    }
}