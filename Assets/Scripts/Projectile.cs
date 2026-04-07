using UnityEngine;

/// <summary>
/// Controls the behavior of a player-fired projectile.
/// 
/// Responsibilities:
/// - moves the projectile in a fixed initialized direction,
/// - destroys the projectile after a limited lifetime,
/// - destroys the projectile on contact with ground,
/// - damages enemies on contact,
/// - applies a temporary mark to enemies on hit,
/// - destroys itself after a successful enemy hit.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 18f;
    public float lifetime = 3f;
    public int damage = 10;
    public float markDuration = 2f;

    // Normalized movement direction assigned when the projectile is spawned.
    private Vector2 moveDirection;

    /// <summary>
    /// Initializes the projectile's movement direction and facing angle.
    /// This should be called immediately after the projectile is spawned.
    /// </summary>
    /// <param name="direction">The direction the projectile should travel in.</param>
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Starts the projectile's self-destruct timer.
    /// Prevents projectiles from existing indefinitely if they hit nothing.
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Moves the projectile every frame in its initialized direction.
    /// </summary>
    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// Handles projectile collision behavior.
    /// 
    /// Ground:
    /// - destroys the projectile immediately.
    /// 
    /// Enemy:
    /// - applies damage,
    /// - applies a mark if the enemy supports it,
    /// - destroys the projectile.
    /// </summary>
    /// <param name="other">The collider the projectile has entered.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);

            EnemyMark enemyMark = other.GetComponent<EnemyMark>();
            if (enemyMark != null)
            {
                enemyMark.ApplyMark(markDuration);
            }

            Destroy(gameObject);
        }
    }
}