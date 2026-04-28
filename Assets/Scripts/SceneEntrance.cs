using UnityEngine;

/// <summary>
/// Marks a valid player arrival point in a scene.
/// 
/// The entranceId is used by scene transitions to decide where the player
/// should appear after loading this scene.
/// </summary>
public class SceneEntrance : MonoBehaviour
{
    public string entranceId;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Upward marker
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.up * 1f
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.2f,
            $"Entrance: {entranceId}"
        );
    }
#endif
}