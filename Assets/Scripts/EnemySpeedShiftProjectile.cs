using UnityEngine;

/// <summary>
/// Enemy projectile that changes speed over time.
/// 
/// Behavior:
/// - starts at an initial speed,
/// - immediately decelerates,
/// - after a delay, accelerates again,
/// - damages the player on contact,
/// - destroys itself after hitting the player or after its lifetime expires.
/// </summary>
public class EnemySpeedShiftProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float initialSpeed = 18f;
    public float lifetime = 4f;
    public int damage = 10;

    [Header("Speed Shift")]
    public float decelerationRate = 20f;
    public float accelerationDelay = 0.75f;
    public float accelerationRate = 30f;
    public float maxSpeed = 24f;

    // Normalized movement direction assigned when the projectile is spawned.
    private Vector2 moveDirection;

    // Current speed after deceleration and acceleration have been applied.
    private float currentSpeed;

    // Time since the projectile became active, used to choose deceleration or acceleration.
    private float elapsedTime;

    /// <summary>
    /// Sets the projectile's travel direction, starting speed, and facing angle.
    /// A rightward fallback is used if the provided direction is too close to zero.
    /// </summary>
    /// <param name="direction">Direction the projectile should travel.</param>
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            moveDirection = Vector2.right;
        }

        currentSpeed = initialSpeed;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Starts the projectile's self-destruct timer.
    /// </summary>
    private void Start()
    {
        currentSpeed = initialSpeed;
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Applies the speed-shift curve and moves the projectile each frame.
    /// </summary>
    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime < accelerationDelay)
        {
            currentSpeed -= decelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }
        else
        {
            currentSpeed += accelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Damages the player and removes the projectile when it overlaps the player.
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
