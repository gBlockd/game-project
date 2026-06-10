using UnityEngine;

/// <summary>
/// Controls a basic projectile fired by an enemy.
///
/// Behavior:
/// - receives a travel direction when it is spawned,
/// - rotates to face that direction,
/// - moves in that direction every frame,
/// - destroys itself after its lifetime expires,
/// - damages the player on contact,
/// - destroys itself after hitting the player.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 12f;
    public float lifetime = 3f;
    public int damage = 10;

    // Normalized movement direction assigned by the enemy that spawned this projectile.
    private Vector2 moveDirection;

    /// <summary>
    /// Sets the projectile's travel direction and rotates the sprite/object to face it.
    /// This should be called immediately after the projectile is created.
    /// </summary>
    /// <param name="direction">Direction the projectile should travel.</param>
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Starts the automatic cleanup timer so missed shots do not remain forever.
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Moves the projectile in its initialized direction every frame.
    /// </summary>
    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// Damages the player if this projectile overlaps the player's collider.
    /// </summary>
    /// <param name="other">The collider this projectile entered.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
