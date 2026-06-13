using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's melee attack input and attack execution.
/// 
/// Responsibilities:
/// - receives attack input from the Input System,
/// - determines attack direction based on mouse position,
/// - positions and rotates the attack hitbox,
/// - activates the hitbox briefly,
/// - fires a short-lived ranged melee hitbox,
/// - enforces attack cooldown timing.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public GameObject attackHitboxObject;
    public float attackRange = 1.2f;
    public float attackDuration = 0.1f;
    public float attackCooldown = 0.5f;

    [Header("Ranged Melee")]
    public float rangedMeleeSpawnDistance = 1.2f;
    public float rangedMeleeLifetime = 0.5f;
    public float rangedMeleeInitialSpeed = 22f;
    public float rangedMeleeDeceleration = 80f;
    public float rangedMeleeCooldown = 0.5f;

    // Cached reference to the main camera, used to convert mouse position
    // from screen space to world space.
    private Camera mainCamera;

    // Set when attack input is received, then consumed in Update().
    private bool attackPressed;
    private bool rangedMeleePressed;

    // Prevents attacks from being started while the current attack/cooldown is active.
    private bool canAttack = true;
    private bool canRangedMelee = true;

    private PlayerHealth playerHealth;
    private PlayerProjectileAttack playerProjectileAttack;

    /// <summary>
    /// Caches references used by melee and ranged melee attacks.
    /// </summary>
    private void Awake()
    {
        mainCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealth>();
        playerProjectileAttack = GetComponent<PlayerProjectileAttack>();
    }

    private void OnEnable()
    {
        attackPressed = false;
        rangedMeleePressed = false;
    }

    /// <summary>
    /// Input System callback for melee attack input.
    /// 
    /// When the attack action is performed, this sets a request flag
    /// that is consumed during Update().
    /// </summary>
    /// <param name="context">Input callback context from the new Input System.</param>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            attackPressed = true;
        }
    }

    /// <summary>
    /// Input System callback for the test ranged melee action.
    ///
    /// This assumes the PlayerControls action is named RangedMelee and is bound
    /// to T while the feature is being tested.
    /// </summary>
    /// <param name="context">Input callback context from the new Input System.</param>
    public void OnRangedMelee(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            rangedMeleePressed = true;
        }
    }

    /// <summary>
    /// Checks for queued attack inputs and starts whichever attacks are allowed.
    /// 
    /// Each input flag is cleared after being processed so each press only
    /// requests one attack.
    /// </summary>
    private void Update()
    {
        if (attackPressed && canAttack)
        {
            StartCoroutine(PerformAttack());
        }

        if (rangedMeleePressed && canRangedMelee)
        {
            StartCoroutine(PerformRangedMeleeAttack());
        }

        attackPressed = false;
        rangedMeleePressed = false;
    }

    /// <summary>
    /// Executes a single melee attack sequence.
    /// 
    /// Steps:
    /// - validates required references,
    /// - blocks further attacks,
    /// - calculates attack direction from player to mouse,
    /// - positions and rotates the attack hitbox,
    /// - enables the hitbox for a short duration,
    /// - disables the hitbox,
    /// - waits out the attack cooldown,
    /// - re-enables attacking.
    /// </summary>
    private IEnumerator PerformAttack()
    {
        if (attackHitboxObject == null || mainCamera == null)
            yield break;

        canAttack = false;

        Vector2 attackDirection = GetAimDirection(transform.position);

        attackHitboxObject.transform.position = (Vector2)transform.position + attackDirection * attackRange;
        attackHitboxObject.transform.rotation = GetRotationFromDirection(attackDirection);

        AttackHitbox attackHitbox = attackHitboxObject.GetComponent<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.ConfigureOwner(transform, playerHealth, playerProjectileAttack);
        }

        attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        attackHitboxObject.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    /// <summary>
    /// Spawns a detached copy of the melee hitbox and launches it forward briefly.
    ///
    /// The spawned object reuses AttackHitbox for damage, marks, knockback,
    /// checkpoint activation, and button activation.
    /// </summary>
    private IEnumerator PerformRangedMeleeAttack()
    {
        if (attackHitboxObject == null || mainCamera == null)
            yield break;

        canRangedMelee = false;

        Vector2 attackDirection = GetAimDirection(transform.position);
        Vector2 spawnPosition = (Vector2)transform.position + attackDirection * rangedMeleeSpawnDistance;

        GameObject rangedHitboxObject = Instantiate(
            attackHitboxObject,
            spawnPosition,
            GetRotationFromDirection(attackDirection)
        );

        rangedHitboxObject.transform.SetParent(null);

        AttackHitbox attackHitbox = rangedHitboxObject.GetComponent<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.ConfigureOwner(transform, playerHealth, playerProjectileAttack);
        }

        RangedMeleeProjectile rangedProjectile = rangedHitboxObject.GetComponent<RangedMeleeProjectile>();
        if (rangedProjectile == null)
        {
            rangedProjectile = rangedHitboxObject.AddComponent<RangedMeleeProjectile>();
        }

        rangedProjectile.Initialize(
            attackDirection,
            rangedMeleeInitialSpeed,
            rangedMeleeDeceleration,
            rangedMeleeLifetime
        );

        rangedHitboxObject.SetActive(true);

        yield return new WaitForSeconds(rangedMeleeCooldown);

        canRangedMelee = true;
    }

    private Vector2 GetAimDirection(Vector2 origin)
    {
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 attackDirection = ((Vector2)mouseWorldPosition - origin).normalized;

        // Fallback direction in the rare case that the mouse is effectively
        // at the player's position, which would otherwise result in a zero vector.
        if (attackDirection.sqrMagnitude < 0.0001f)
        {
            attackDirection = Vector2.right;
        }

        return attackDirection;
    }

    private Quaternion GetRotationFromDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle);
    }
}
