using UnityEngine;

/// <summary>
/// Simple spike hazard.
/// 
/// On contact with the player:
/// - deals damage,
/// - triggers hazard recovery,
/// - sends the player back to their last grounded position after a brief pause.
/// </summary>
public class SpikeHazard : MonoBehaviour
{
    [Header("Spike Damage")]
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHazardRecovery hazardRecovery = other.GetComponent<PlayerHazardRecovery>();
        if (hazardRecovery == null)
            return;

        hazardRecovery.HandleHazardHit(damage);
    }
}