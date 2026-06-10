using UnityEngine;

/// <summary>
/// Collectible object that permanently unlocks a player ability.
/// 
/// Examples:
/// - Flight pickup unlocks flight.
/// - Dash pickup unlocks dash.
/// 
/// Once the matching ability has already been unlocked, this pickup removes itself
/// so it does not reappear after scene transitions.
/// </summary>
public class AbilityPickup : MonoBehaviour
{
    [Header("Ability")]
    public PlayerAbilityType abilityType;

    [Header("Pickup")]
    public string pickupId;

    /// <summary>
    /// Removes the pickup from the scene if this ability has already been collected.
    /// </summary>
    private void Start()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsAbilityUnlocked(abilityType))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Unlocks the configured ability when the player touches the pickup.
    ///
    /// The player movement script is refreshed immediately so the new ability can
    /// be used without reloading the scene. Upgrade feedback plays when available.
    /// </summary>
    /// <param name="other">The collider that entered this pickup trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockAbility(abilityType);
        }

        playerMovement.RefreshUnlockedAbilityState();

        PlayerUpgradeFeedback upgradeFeedback = other.GetComponent<PlayerUpgradeFeedback>();
        if (upgradeFeedback != null)
        {
            upgradeFeedback.PlayUpgradeFeedback();
        }

        Destroy(gameObject);
    }
}
