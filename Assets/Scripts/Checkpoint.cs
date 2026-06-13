using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Checkpoint object that can be activated by the player's melee attack.
/// 
/// When activated:
/// - heals the player to full,
/// - refills player energy to full,
/// - updates the player's respawn point in the GameStateManager,
/// - flashes green for visual feedback.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint")]
    public Transform respawnPoint;

    private CheckpointFlash checkpointFlash;

    private void Awake()
    {
        checkpointFlash = GetComponent<CheckpointFlash>();
    }

    public void Activate(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
            return;

        playerHealth.ResetToFullHealth();

        PlayerEnergy playerEnergy = playerHealth.GetComponent<PlayerEnergy>();
        if (playerEnergy != null)
        {
            playerEnergy.FillEnergyToMax();
        }

        Vector3 respawnPosition = respawnPoint != null
            ? respawnPoint.position
            : transform.position;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetCheckpoint(
                SceneManager.GetActiveScene().name,
                respawnPosition
            );
        }

        if (checkpointFlash != null)
        {
            checkpointFlash.Flash();
        }
    }
}
