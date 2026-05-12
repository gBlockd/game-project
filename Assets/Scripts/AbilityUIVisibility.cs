using UnityEngine;

/// <summary>
/// Shows or hides ability-related UI based on whether the player has unlocked
/// the matching abilities.
/// </summary>
public class AbilityUIVisibility : MonoBehaviour
{
    [Header("UI Roots")]
    public GameObject flightUIRoot;
    public GameObject dashUIRoot;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (GameStateManager.Instance == null)
            return;

        if (flightUIRoot != null)
        {
            flightUIRoot.SetActive(GameStateManager.Instance.HasFlightAbility);
        }

        if (dashUIRoot != null)
        {
            dashUIRoot.SetActive(GameStateManager.Instance.HasDashAbility);
        }
    }
}