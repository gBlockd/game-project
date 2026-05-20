using UnityEngine;

/// <summary>
/// Trigger zone that changes the camera zoom when the player enters it.
/// 
/// Useful for:
/// - large arenas,
/// - tight corridors,
/// - boss rooms,
/// - platforming sections.
/// </summary>
public class CameraZoomZone : MonoBehaviour
{
    [Header("Zoom")]
    public float targetZoom = 8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player == null)
            return;

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (cameraFollow == null)
            return;

        cameraFollow.SetZoom(targetZoom);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.offset, box.size);
            }
        }
    }
}