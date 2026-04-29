using UnityEngine;

/// <summary>
/// Damages the player when this enemy's trigger collider overlaps them.
/// 
/// Some enemies, such as ProjectileChargerEnemy, only allow contact damage
/// during specific attack windows.
/// </summary>
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    public int contactDamage = 10;
    public float damageCooldown = 0.5f;

    private float lastDamageTime = -999f;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            return;

        ProjectileChargerEnemy projectileCharger = GetComponentInParent<ProjectileChargerEnemy>();
        if (projectileCharger != null && !projectileCharger.CanDealContactDamage)
            return;

        TryDamagePlayer(playerHealth);
    }

    public void TryDamagePlayer(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
            return;

        if (Time.time < lastDamageTime + damageCooldown)
            return;

        playerHealth.TakeDamage(contactDamage);
        lastDamageTime = Time.time;
    }
}