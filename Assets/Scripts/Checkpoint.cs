using UnityEngine;

/// <summary>
/// Checkpoint object that can be activated by the player's melee attack.
/// 
/// When activated:
/// - heals the player to full,
/// - updates the player's respawn point in the RespawnManager,
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

        Vector3 respawnPosition = respawnPoint != null
            ? respawnPoint.position
            : transform.position;

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetRespawnPoint(respawnPosition);
        }

        // Trigger flash
        if (checkpointFlash != null)
        {
            checkpointFlash.Flash();
        }
    }
}