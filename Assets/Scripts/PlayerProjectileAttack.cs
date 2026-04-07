using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's projectile firing behavior.
/// 
/// Responsibilities:
/// - receives projectile fire input,
/// - aims toward the mouse cursor,
/// - spawns and initializes projectiles,
/// - enforces projectile cooldown,
/// - allows external systems to refresh that cooldown.
/// </summary>
public class PlayerProjectileAttack : MonoBehaviour
{
    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileCooldown = 5f;

    // Cached reference to the main camera, used to convert mouse position
    // from screen space to world space.
    private UnityEngine.Camera mainCamera;

    // Set when fire input is received, then consumed in Update().
    private bool firePressed;

    // Tracks whether the player is currently allowed to fire a projectile.
    private bool canFireProjectile = true;

    // Tracks the currently running cooldown coroutine so it can be interrupted or refreshed.
    private Coroutine cooldownCoroutine;

    // Public read-only access for UI or other gameplay systems.
    public bool CanFireProjectile => canFireProjectile;

    /// <summary>
    /// Caches the main camera reference.
    /// </summary>
    private void Awake()
    {
        mainCamera = UnityEngine.Camera.main;
    }

    /// <summary>
    /// Input System callback for projectile fire input.
    /// 
    /// When the fire action is performed, this sets a request flag
    /// that is consumed during Update().
    /// </summary>
    /// <param name="context">Input callback context from the new Input System.</param>
    public void OnFireProjectile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            firePressed = true;
        }
    }

    /// <summary>
    /// Checks for a queued projectile fire input and fires if allowed.
    /// 
    /// The input flag is always cleared after being processed so each click
    /// only requests one projectile.
    /// </summary>
    private void Update()
    {
        if (!firePressed)
            return;

        firePressed = false;

        if (canFireProjectile)
        {
            FireProjectile();
        }
    }

    /// <summary>
    /// Spawns a projectile at the configured spawn point and initializes it
    /// to travel toward the mouse cursor.
    /// 
    /// Also starts the projectile cooldown after a successful fire.
    /// </summary>
    private void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || mainCamera == null)
            return;

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 direction = (mouseWorldPosition - projectileSpawnPoint.position).normalized;

        // Fallback direction in the rare case that the mouse is effectively
        // at the spawn point, which would otherwise result in a zero vector.
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction);
        }

        StartProjectileCooldown();
    }

    /// <summary>
    /// Starts or restarts the projectile cooldown.
    /// 
    /// If a cooldown is already running, it is stopped and replaced
    /// with a fresh cooldown timer.
    /// </summary>
    private void StartProjectileCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine = StartCoroutine(ProjectileCooldownRoutine());
    }

    /// <summary>
    /// Handles the projectile cooldown timer.
    /// 
    /// While active:
    /// - projectile firing is disabled,
    /// - after the configured duration, firing is re-enabled.
    /// </summary>
    private IEnumerator ProjectileCooldownRoutine()
    {
        canFireProjectile = false;

        yield return new WaitForSeconds(projectileCooldown);

        canFireProjectile = true;
        cooldownCoroutine = null;
    }

    /// <summary>
    /// Instantly refreshes the projectile cooldown, allowing the player to fire again immediately.
    /// 
    /// Used by other systems, such as consuming a mark with a melee attack.
    /// </summary>
    public void RefreshProjectileCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        canFireProjectile = true;
    }
}