using System.Collections;
using UnityEngine;

/// <summary>
/// Door used for gated combat encounters.
/// 
/// Starts open. When closed, it slides downward by slideDistance.
/// When opened again, it slides back to its original position.
/// </summary>
public class EncounterDoor : MonoBehaviour
{
    [Header("Encounter Link")]
    public string encounterId;

    [Header("Movement")]
    public float slideDistance = 3f;
    public float slideDuration = 0.35f;

    private Vector3 openPosition;
    private Vector3 closedPosition;
    private Coroutine moveCoroutine;

    private bool isClosed;

    private void Awake()
    {
        openPosition = transform.position;
        closedPosition = openPosition + Vector3.down * slideDistance;
    }

    public void CloseDoor()
    {
        if (isClosed)
            return;

        isClosed = true;
        MoveToPosition(closedPosition);
    }

    public void OpenDoor()
    {
        if (!isClosed)
            return;

        isClosed = false;
        MoveToPosition(openPosition);
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveRoutine(targetPosition));
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        moveCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 open = Application.isPlaying ? openPosition : transform.position;
        Vector3 closed = open + Vector3.down * slideDistance;

        Gizmos.DrawWireCube(open, transform.localScale);
        Gizmos.DrawWireCube(closed, transform.localScale);
        Gizmos.DrawLine(open, closed);
    }
}