using UnityEngine;

/// <summary>
/// Controls the behavior of an enemy-fired projectile.
/// 
/// Behavior:
/// - moves in a fixed initialized direction,
/// - destroys itself after a lifetime,
/// - damages the player on contact,
/// - destroys itself on contact with ground,
/// - destroys itself after hitting the player.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 12f;
    public float lifetime = 3f;
    public int damage = 10;

    private Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}