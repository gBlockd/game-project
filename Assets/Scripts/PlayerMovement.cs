using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's core movement systems.
/// 
/// Responsibilities:
/// - horizontal ground/air movement,
/// - flight with limited duration,
/// - glide behavior when flight is depleted,
/// - dive behavior while descending,
/// - fall gravity tuning and fall speed limits,
/// - dash behavior toward the mouse cursor,
/// - ground detection and flight/dash refills.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 20f;
    public float deceleration = 25f;

    [Header("Flight")]
    public float maxFlightTime = 5f;
    public float groundTakeoffSpeed = 12f;
    public float flightAcceleration = 30f;
    public float maxRiseSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Fall Settings")]
    public float maxFallSpeed = -12f;
    public float glideGravityMultiplier = 0.2f;
    public float glideFallSpeedMultiplier = 0.2f;
    public float diveGravityMultiplier = 2.5f;
    public float diveFallSpeedMultiplier = 1.5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.12f;
    public float dashEndSpeed = 16f;
    public int maxDashCharges = 3;
    public float dashCooldown = 0.5f;

    // Core component references.
    private Rigidbody2D rb;
    private Camera mainCamera;

    // Input state.
    private float horizontalInput;
    private bool flyHeld;
    private bool diveHeld;
    private bool dashPressed;

    // Horizontal movement state.
    private float currentHorizontalSpeed;

    // Flight state.
    private float currentFlightTime;

    // Ground state.
    private bool isGrounded;
    private bool wasGrounded;

    // Glide state.
    private bool wasGliding;

    // Dash state.
    private bool isDashing;
    private bool canDash = true;
    private int currentDashCharges;
    private Vector2 dashDirection;

    // Cached gravity value used when temporarily overriding gravity during dash.
    private float normalGravityScale;

    // Public read-only accessors for UI and other systems.
    public float CurrentFlightTime => currentFlightTime;
    public float MaxFlightTime => maxFlightTime;

    public int CurrentDashCharges => currentDashCharges;
    public int MaxDashCharges => maxDashCharges;

    private Vector2 lastGroundedPosition;
    public Vector2 LastGroundedPosition => lastGroundedPosition;

    /// <summary>
    /// Caches component references and initializes runtime values.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentFlightTime = maxFlightTime;
        currentDashCharges = maxDashCharges;

        mainCamera = Camera.main;
        normalGravityScale = rb.gravityScale;
        lastGroundedPosition = transform.position;
    }

    public void ResetMomentum()
    {
        currentHorizontalSpeed = 0f;
        dashPressed = false;
        isDashing = false;
        dashDirection = Vector2.zero;
    }

    /// <summary>
    /// Input System callback for horizontal movement.
    /// Reads a 1D axis value for left/right movement.
    /// </summary>
    public void OnMoveHorizontal(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();
    }

    /// <summary>
    /// Input System callback for flight input.
    /// Tracks whether the flight button is currently held.
    /// </summary>
    public void OnFly(InputAction.CallbackContext context)
    {
        flyHeld = context.ReadValueAsButton();
    }

    /// <summary>
    /// Input System callback for dive input.
    /// Tracks whether the dive button is currently held.
    /// </summary>
    public void OnDive(InputAction.CallbackContext context)
    {
        diveHeld = context.ReadValueAsButton();
    }

    /// <summary>
    /// Input System callback for dash input.
    /// Queues a dash request that will be processed in Update().
    /// </summary>
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashPressed = true;
        }
    }

    /// <summary>
    /// Updates ground state, refills flight and dash charges on landing,
    /// and starts a dash if requested.
    /// </summary>
    private void Update()
    {
        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            lastGroundedPosition = transform.position;
        }

        // Refill flight and dash charges instantly on landing.
        if (isGrounded && !wasGrounded)
        {
            currentFlightTime = maxFlightTime;
            currentDashCharges = maxDashCharges;
        }

        // Start dash if requested and available.
        if (dashPressed && canDash && currentDashCharges > 0 && !isDashing)
        {
            dashPressed = false;
            StartCoroutine(PerformDash());
        }
        else
        {
            dashPressed = false;
        }
    }

    /// <summary>
    /// Runs the player's physics-based movement logic.
    /// 
    /// While dashing, normal movement logic is skipped so the dash
    /// fully controls the player's velocity.
    /// </summary>
    private void FixedUpdate()
    {
        if (isDashing)
            return;

        HandleHorizontalMovement();
        HandleFlight();
        ApplyBetterFallGravity();
        LimitFallSpeed();
    }

    /// <summary>
    /// Handles horizontal movement acceleration/deceleration.
    /// 
    /// Air movement uses doubled speed and acceleration compared to grounded movement.
    /// </summary>
    private void HandleHorizontalMovement()
    {
        float speedMultiplier = isGrounded ? 1f : 2f;
        float accelMultiplier = isGrounded ? 1f : 2f;

        float targetSpeed = horizontalInput * moveSpeed * speedMultiplier;
        float rate = Mathf.Abs(targetSpeed) > 0.01f
            ? acceleration * accelMultiplier
            : deceleration * accelMultiplier;

        currentHorizontalSpeed = Mathf.MoveTowards(
            currentHorizontalSpeed,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(currentHorizontalSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Handles upward flight behavior.
    /// </summary>
    private void HandleFlight()
    {
        if (isGrounded)
        {
            if (flyHeld)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, groundTakeoffSpeed);
            }

            return;
        }

        if (flyHeld && currentFlightTime > 0f)
        {
            currentFlightTime -= Time.fixedDeltaTime;
            currentFlightTime = Mathf.Max(currentFlightTime, 0f);

            float newYVelocity = rb.linearVelocity.y + flightAcceleration * Time.fixedDeltaTime;
            newYVelocity = Mathf.Min(newYVelocity, maxRiseSpeed);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYVelocity);
        }
    }

    /// <summary>
    /// Applies state-based gravity tuning for falling, gliding, and diving.
    /// </summary>
    private void ApplyBetterFallGravity()
    {
        bool isFalling = rb.linearVelocity.y < 0f;
        bool isOutOfFlight = currentFlightTime <= 0f;

        bool isGliding = !isGrounded && flyHeld && isFalling && isOutOfFlight;
        bool isDiving = !isGrounded && diveHeld && !flyHeld;

        if (wasGliding && !isGliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }

        if (isGliding)
        {
            rb.gravityScale = 3f * glideGravityMultiplier;
        }
        else if (isDiving)
        {
            rb.gravityScale = 3f * diveGravityMultiplier;
        }
        else if (isFalling)
        {
            rb.gravityScale = 2f;
        }
        else
        {
            rb.gravityScale = 3f;
        }

        wasGliding = isGliding;
    }

    /// <summary>
    /// Limits downward velocity based on the current movement state.
    /// </summary>
    private void LimitFallSpeed()
    {
        float currentMaxFallSpeed = maxFallSpeed;

        bool isFalling = rb.linearVelocity.y < 0f;
        bool isOutOfFlight = currentFlightTime <= 0f;

        bool isGliding = !isGrounded && flyHeld && isFalling && isOutOfFlight;
        bool isDiving = !isGrounded && diveHeld && !flyHeld;

        if (isGliding)
        {
            currentMaxFallSpeed *= glideFallSpeedMultiplier;
        }
        else if (isDiving)
        {
            currentMaxFallSpeed *= diveFallSpeedMultiplier;
        }

        if (rb.linearVelocity.y < currentMaxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentMaxFallSpeed);
        }
    }

    /// <summary>
    /// Draws the ground check radius in the editor for easier tuning and debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    /// <summary>
    /// Performs a dash toward the mouse cursor.
    /// 
    /// Dash behavior:
    /// - spends one dash charge,
    /// - starts dash cooldown,
    /// - ignores previous momentum,
    /// - disables gravity during dash,
    /// - moves at dashSpeed for dashDuration,
    /// - ends by applying dashEndSpeed in the same direction.
    /// </summary>
    private System.Collections.IEnumerator PerformDash()
    {
        if (mainCamera == null || currentDashCharges <= 0 || !canDash)
            yield break;

        currentDashCharges--;
        canDash = false;
        isDashing = true;

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        dashDirection = (mouseWorldPosition - transform.position).normalized;

        if (dashDirection.sqrMagnitude < 0.0001f)
        {
            dashDirection = Vector2.right;
        }

        currentHorizontalSpeed = 0f;

        rb.gravityScale = 0f;
        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = dashDirection * dashEndSpeed;

        currentHorizontalSpeed = rb.linearVelocity.x;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}