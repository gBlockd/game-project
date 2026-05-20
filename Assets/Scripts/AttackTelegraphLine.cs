using System.Collections;
using UnityEngine;

/// <summary>
/// Visual warning line used to telegraph enemy attacks.
/// 
/// The line can either:
/// - stay fixed in one direction,
/// - or track from an origin transform toward a target transform in real time.
/// 
/// The line remains continuously visible until its duration expires.
/// </summary>
public class AttackTelegraphLine : MonoBehaviour
{
    [Header("Visuals")]
    public LineRenderer lineRenderer;

    private Transform trackingOrigin;
    private Transform trackingTarget;
    private float trackingLength;
    private float trackingAngleOffset;
    private bool isTracking;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    public void Initialize(Vector2 startPosition, Vector2 direction, float length, float duration)
    {
        if (lineRenderer == null)
            return;

        isTracking = false;

        direction = direction.normalized;

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition + direction * length);

        StartCoroutine(DestroyAfterDuration(duration));
    }

    public void InitializeTracking(Transform origin, Transform target, float length, float duration, float angleOffsetDegrees)
    {
        if (lineRenderer == null || origin == null || target == null)
            return;

        trackingOrigin = origin;
        trackingTarget = target;
        trackingLength = length;
        trackingAngleOffset = angleOffsetDegrees;
        isTracking = true;

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = true;

        StartCoroutine(DestroyAfterDuration(duration));
    }

    private void Update()
    {
        if (!isTracking || lineRenderer == null || trackingOrigin == null || trackingTarget == null)
            return;

        Vector2 startPosition = trackingOrigin.position;
        Vector2 direction = ((Vector2)trackingTarget.position - startPosition).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.down;
        }

        direction = RotateVector(direction, trackingAngleOffset);

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition + direction * trackingLength);
    }

    private IEnumerator DestroyAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private Vector2 RotateVector(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }
}