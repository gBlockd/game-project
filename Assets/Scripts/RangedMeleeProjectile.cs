using UnityEngine;

/// <summary>
/// Moves a spawned melee hitbox forward for a very short ranged attack.
///
/// The hitbox starts fast, rapidly loses speed, and destroys itself after its lifetime.
/// Hit detection remains on AttackHitbox, which this script asks to rescan after movement.
/// </summary>
public class RangedMeleeProjectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float currentSpeed;
    private float deceleration;
    private float lifetime;
    private float elapsed;
    private AttackHitbox attackHitbox;

    private void Awake()
    {
        attackHitbox = GetComponent<AttackHitbox>();
    }

    /// <summary>
    /// Sets the projectile's movement values and starts its lifetime timer.
    /// </summary>
    public void Initialize(Vector2 direction, float initialSpeed, float decelerationRate, float projectileLifetime)
    {
        moveDirection = direction.normalized;

        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            moveDirection = Vector2.right;
        }

        currentSpeed = initialSpeed;
        deceleration = decelerationRate;
        lifetime = projectileLifetime;
        elapsed = 0f;

        if (attackHitbox == null)
        {
            attackHitbox = GetComponent<AttackHitbox>();
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        if (attackHitbox != null)
        {
            attackHitbox.CheckForOverlappingObjects();
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
