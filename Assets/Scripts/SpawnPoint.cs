using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Marker component used to identify the player's initial respawn location.
/// 
/// The first SpawnPoint encountered becomes the player's permanent default respawn
/// until a checkpoint is activated. After a checkpoint is reached, SpawnPoint is
/// no longer used for respawning.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetInitialSpawn(
                SceneManager.GetActiveScene().name,
                transform.position
            );
        }
    }
}