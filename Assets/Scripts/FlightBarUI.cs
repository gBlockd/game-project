using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the flight meter UI matched to the player's remaining flight time.
///
/// The Image should be configured as a filled image. Each frame, this script
/// converts the player's current flight time into a 0-1 fill amount.
/// </summary>
public class FlightBarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Image fillImage;

    /// <summary>
    /// Updates the UI fill level every frame when both required references are assigned.
    /// </summary>
    private void Update()
    {
        if (playerMovement == null || fillImage == null)
            return;

        float fillPercent = playerMovement.CurrentFlightTime / playerMovement.MaxFlightTime;
        fillImage.fillAmount = fillPercent;
    }
}
