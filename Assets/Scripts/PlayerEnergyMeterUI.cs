using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a filled UI Image to show the player's current energy within one charge.
///
/// Set the Image Type to Filled in the inspector. This script writes to fillAmount,
/// matching the same setup used by the existing health and stamina bars.
/// </summary>
public class PlayerEnergyMeterUI : MonoBehaviour
{
    public PlayerEnergy playerEnergy;
    public Image fillImage;

    [Header("Colors")]
    public Color chargingColor = Color.white;
    public Color chargeReadyColor = Color.yellow;

    private void Update()
    {
        if (playerEnergy == null || fillImage == null)
            return;

        fillImage.fillAmount = playerEnergy.CurrentChargeFill;
        fillImage.color = playerEnergy.HasFullCharge ? chargeReadyColor : chargingColor;
    }
}
