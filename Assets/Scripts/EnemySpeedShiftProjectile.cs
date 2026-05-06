using UnityEngine;

/// <summary>
/// Enemy projectile that changes speed over time.
/// 
/// Behavior:
/// - starts at an initial speed,
/// - immediately decelerates,
/// - after a delay, accelerates again,
/// - damages the player on contact,
/// - destroys itself on ground contact or after its lifetime expires.
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

    private Vector2 moveDirection;
    private float currentSpeed;
    private float elapsedTime;

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

    private void Start()
    {
        currentSpeed = initialSpeed;
        Destroy(gameObject, lifetime);
    }

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