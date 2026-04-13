using UnityEngine;

/// <summary>
/// Follows a target while remaining confined within a defined rectangular play area.
/// 
/// The camera tracks the target normally, but its position is clamped so it cannot
/// move beyond the room bounds. This allows the camera to stop following horizontally
/// at walls and vertically at floors/ceilings.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Bounds")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(target.position.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, minY + halfHeight, maxY - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, -10f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 bottomLeft = new Vector3(minX, minY, 0f);
        Vector3 bottomRight = new Vector3(maxX, minY, 0f);
        Vector3 topLeft = new Vector3(minX, maxY, 0f);
        Vector3 topRight = new Vector3(maxX, maxY, 0f);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}