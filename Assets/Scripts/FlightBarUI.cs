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

    private Image[] meterImages;
    private bool hasAppliedVisibility;
    private bool isMeterVisible;

    private void Awake()
    {
        meterImages = GetComponentsInChildren<Image>(true);
    }

    /// <summary>
    /// Updates the UI fill level every frame when both required references are assigned.
    /// </summary>
    private void Update()
    {
        if (playerMovement == null || fillImage == null)
            return;

        bool shouldShowMeter = playerMovement.HasFlightAbility;
        SetMeterVisible(shouldShowMeter);

        if (!shouldShowMeter)
            return;

        float fillPercent = playerMovement.MaxFlightTime <= 0f
            ? 0f
            : playerMovement.CurrentFlightTime / playerMovement.MaxFlightTime;

        fillImage.fillAmount = Mathf.Clamp01(fillPercent);
    }

    private void SetMeterVisible(bool shouldBeVisible)
    {
        if (hasAppliedVisibility && isMeterVisible == shouldBeVisible)
            return;

        if (meterImages != null)
        {
            for (int i = 0; i < meterImages.Length; i++)
            {
                if (meterImages[i] != null)
                {
                    meterImages[i].enabled = shouldBeVisible;
                }
            }
        }

        isMeterVisible = shouldBeVisible;
        hasAppliedVisibility = true;
    }
}
