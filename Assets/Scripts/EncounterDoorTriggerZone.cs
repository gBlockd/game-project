using UnityEngine;

/// <summary>
/// One-time trigger zone that closes matching encounter doors when the player enters.
/// </summary>
public class EncounterDoorTriggerZone : MonoBehaviour
{
    [Header("Encounter Link")]
    public string encounterId;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null)
            return;

        hasTriggered = true;
        CloseLinkedDoors();
    }

    private void CloseLinkedDoors()
    {
        EncounterDoor[] doors = FindObjectsByType<EncounterDoor>();

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].encounterId == encounterId)
            {
                doors[i].CloseDoor();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Collider2D col = GetComponent<Collider2D>();

        if (col is BoxCollider2D box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}