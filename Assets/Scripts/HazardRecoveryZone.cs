using UnityEngine;

public class HazardRecoveryZone : MonoBehaviour
{
    [Header("Linked Recovery Point")]
    public string recoveryPointId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHazardRecovery hazardRecovery = other.GetComponent<PlayerHazardRecovery>();
        if (hazardRecovery == null)
            return;

        hazardRecovery.SetRecoveryPointById(recoveryPointId);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}