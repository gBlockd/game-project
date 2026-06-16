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

    private Transform owner;
    private bool hasOwner;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    /// <summary>
    /// Assigns the enemy or object that created this telegraph.
    /// If that owner is destroyed before the timer ends, the line is removed too.
    /// </summary>
    public void SetOwner(Transform newOwner)
    {
        owner = newOwner;
        hasOwner = owner != null;
    }

    public void Initialize(Vector2 startPosition, Vector2 direction, float length, float duration)
    {
        Initialize(startPosition, direction, length, duration, Color.white, 0.08f);
    }

    public void Initialize(Vector2 startPosition, Vector2 direction, float length, float duration, Color color, float width)
    {
        if (lineRenderer == null)
            return;

        isTracking = false;

        direction = direction.normalized;

        ApplyVisuals(color, width);

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition + direction * length);

        StartCoroutine(DestroyAfterDuration(duration));
    }

    public void InitializeTracking(Transform origin, Transform target, float length, float duration, float angleOffsetDegrees)
    {
        InitializeTracking(origin, target, length, duration, angleOffsetDegrees, Color.white, 0.08f);
    }

    public void InitializeTracking(Transform origin, Transform target, float length, float duration, float angleOffsetDegrees, Color color, float width)
    {
        if (lineRenderer == null || origin == null || target == null)
            return;

        trackingOrigin = origin;
        trackingTarget = target;
        trackingLength = length;
        trackingAngleOffset = angleOffsetDegrees;
        isTracking = true;

        ApplyVisuals(color, width);

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = true;

        StartCoroutine(DestroyAfterDuration(duration));
    }

    private void ApplyVisuals(Color color, float width)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    private void Update()
    {
        if (hasOwner && owner == null)
        {
            Destroy(gameObject);
            return;
        }

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