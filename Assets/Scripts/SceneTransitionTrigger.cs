using UnityEngine;

/// <summary>
/// Loads another scene and sends the player to a specific entrance point in that scene.
/// </summary>
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Destination")]
    public string targetSceneName;
    public string targetEntranceId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            return;

        if (SceneTransitionManager.Instance != null && !SceneTransitionManager.Instance.IsTransitioning)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetEntranceId);
        }
    }
}