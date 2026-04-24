using UnityEngine;

/// <summary>
/// Designer-placed point used by PlayerHazardRecovery.
/// 
/// When the player leaves the ground, the nearest HazardRecoveryPoint
/// becomes their current hazard recovery position.
/// </summary>
public class HazardRecoveryPoint : MonoBehaviour
{
    [Header("Recovery Point")]
    public string recoveryPointId;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}