using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's current dash charges using a set of UI circles.
/// 
/// Each image represents one dash charge:
/// - full alpha = charge available
/// - dim alpha = charge spent
/// 
/// The dash charge images stay hidden until the dash ability is unlocked.
/// </summary>
public class DashChargesUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Image[] chargeImages;

    [Header("Appearance")]
    [Range(0f, 1f)] public float activeAlpha = 1f;
    [Range(0f, 1f)] public float inactiveAlpha = 0.3f;

    private Image[] meterImages;
    private bool hasAppliedVisibility;
    private bool isMeterVisible;

    private void Awake()
    {
        meterImages = GetComponentsInChildren<Image>(true);
    }

    private void Update()
    {
        if (playerMovement == null || chargeImages == null)
            return;

        bool shouldShowMeter = playerMovement.HasDashAbility;
        SetMeterVisible(shouldShowMeter);

        if (!shouldShowMeter)
            return;

        int currentCharges = playerMovement.CurrentDashCharges;

        for (int i = 0; i < chargeImages.Length; i++)
        {
            if (chargeImages[i] == null)
                continue;

            Color color = chargeImages[i].color;
            color.a = i < currentCharges ? activeAlpha : inactiveAlpha;
            chargeImages[i].color = color;
        }
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