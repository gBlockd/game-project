using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image fillImage;

    private void Update()
    {
        if (playerHealth == null || fillImage == null)
            return;

        float fillPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        fillImage.fillAmount = fillPercent;
    }
}