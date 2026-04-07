using UnityEngine;
using UnityEngine.UI;

public class FlightBarUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Image fillImage;

    private void Update()
    {
        if (playerMovement == null || fillImage == null)
            return;

        float fillPercent = playerMovement.CurrentFlightTime / playerMovement.MaxFlightTime;
        fillImage.fillAmount = fillPercent;
    }
}