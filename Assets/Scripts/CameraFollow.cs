using UnityEngine;

/// <summary>
/// Follows a target while remaining confined within a defined rectangular play area.
/// 
/// The camera tracks the target normally, but its position is clamped so it cannot
/// move beyond the room bounds. This allows the camera to stop following horizontally
/// at walls and vertically at floors/ceilings.
/// 
/// Also supports dynamic zoom changes.
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

    [Header("Zoom")]
    public float zoomLerpSpeed = 5f;

    private Camera cam;

    private float targetZoom;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomLerpSpeed * Time.deltaTime
        );

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(target.position.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, minY + halfHeight, maxY - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, -10f);
    }

    /// <summary>
    /// Changes the camera zoom level.
    /// Lower values = zoomed in.
    /// Higher values = zoomed out.
    /// </summary>
    public void SetZoom(float newZoom)
    {
        targetZoom = Mathf.Max(0.1f, newZoom);
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