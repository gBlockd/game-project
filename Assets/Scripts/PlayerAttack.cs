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
/// - enforces attack cooldown timing.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public GameObject attackHitboxObject;
    public float attackRange = 1.2f;
    public float attackDuration = 0.1f;
    public float attackCooldown = 0.5f;

    // Cached reference to the main camera, used to convert mouse position
    // from screen space to world space.
    private Camera mainCamera;

    // Set when attack input is received, then consumed in Update().
    private bool attackPressed;

    // Prevents attacks from being started while the current attack/cooldown is active.
    private bool canAttack = true;

    /// <summary>
    /// Caches the main camera reference.
    /// </summary>
    private void Awake()
    {
        mainCamera = Camera.main;
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
        if (context.performed)
        {
            attackPressed = true;
        }
    }

    /// <summary>
    /// Checks for a queued attack input and starts the attack if allowed.
    /// 
    /// The input flag is always cleared after being processed so each click
    /// only requests one attack.
    /// </summary>
    private void Update()
    {
        if (attackPressed && canAttack)
        {
            attackPressed = false;
            StartCoroutine(PerformAttack());
        }
        else
        {
            attackPressed = false;
        }
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

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 attackDirection = (mouseWorldPosition - transform.position).normalized;

        // Fallback direction in the rare case that the mouse is effectively
        // at the player's position, which would otherwise result in a zero vector.
        if (attackDirection.sqrMagnitude < 0.0001f)
        {
            attackDirection = Vector2.right;
        }

        attackHitboxObject.transform.position = (Vector2)transform.position + attackDirection * attackRange;

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        attackHitboxObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        attackHitboxObject.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}