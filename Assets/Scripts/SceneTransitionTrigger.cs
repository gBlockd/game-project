using UnityEngine;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }

        // Draw forward direction arrow
        Gizmos.color = Color.cyan;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + transform.right * 1.5f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"To: {targetSceneName}\nEntrance: {targetEntranceId}"
        );
    }
#endif
}