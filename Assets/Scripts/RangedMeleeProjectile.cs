using UnityEngine;

/// <summary>
/// Moves a spawned melee hitbox forward for a very short ranged attack.
///
/// The hitbox starts fast, rapidly loses speed, and destroys itself after its lifetime.
/// This script only handles movement and cleanup; hit detection remains on AttackHitbox.
/// </summary>
public class RangedMeleeProjectile : MonoBehaviour
{
    private Vector2 moveDirection;
    private float currentSpeed;
    private float deceleration;
    private float lifetime;
    private float elapsed;

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
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
