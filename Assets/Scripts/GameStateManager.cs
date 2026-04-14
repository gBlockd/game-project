using UnityEngine;

/// <summary>
/// Stores persistent player state across scene transitions.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Player State")]
    public int currentHealth = 100;
    public int maxHealth = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
    }

    public void ResetHealthToFull()
    {
        currentHealth = maxHealth;
    }
}