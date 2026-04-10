using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's current dash charges using a set of UI circles.
/// 
/// Each image represents one dash charge:
/// - full alpha = charge available
/// - dim alpha = charge spent
/// </summary>
public class DashChargesUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Image[] chargeImages;

    [Header("Appearance")]
    [Range(0f, 1f)] public float activeAlpha = 1f;
    [Range(0f, 1f)] public float inactiveAlpha = 0.3f;

    private void Update()
    {
        if (playerMovement == null || chargeImages == null)
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
}