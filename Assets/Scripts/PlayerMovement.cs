using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's core movement systems.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 20f;
    public float deceleration = 25f;

    [Header("Jump")]
    public float jumpSpeed = 10f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
    public float coyoteTimeDuration = 0.1f;

    [Header("Flight")]
    public float maxFlightTime = 5f;
    public float groundTakeoffSpeed = 12f;
    public float flightAcceleration = 30f;
    public float maxRiseSpeed = 10f;

    [Header("Ground Check")]
    public Collider2D bodyCollider;
    public float groundCheckDistance = 0.08f;
    public float groundCheckHeight = 0.06f;
    [Range(0.1f, 1f)] public float groundCheckWidthMultiplier = 0.9f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.08f;
    [Range(0.1f, 1f)] public float wallCheckHeightMultiplier = 0.8f;

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

    [Header("Damage Knockback")]
    public float damageKnockbackMoveDuration = 0.25f;
    public float damageKnockbackHorizontalSpeed = 14f;
    public float damageKnockbackUpSpeed = 9f;
    public float damageKnockbackDecay = 4f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private PlayerEnergy playerEnergy;

    private float horizontalInput;
    private bool jumpPressed;
    private bool flyHeld;
    private bool diveHeld;
    private bool dashPressed;
    private bool isInFlightMode;

    private bool hasFlightAbility;
    private bool hasDashAbility;

    private float currentHorizontalSpeed;
    private float currentFlightTime;

    private bool isGrounded;
    private bool wasGrounded;

    private bool hasStartedJump;
    private float coyoteTimeCounter;

    private bool wasGliding;

    private bool isDashing;
    private bool canDash = true;
    private int currentDashCharges;
    private Vector2 dashDirection;

    private float normalGravityScale;
    private int facingDirection = 1;
    private bool movementDisabled;
    private Coroutine dashCoroutine;
    private Coroutine movementDisableCoroutine;

    public float CurrentFlightTime => currentFlightTime;
    public float MaxFlightTime => maxFlightTime;

    public int CurrentDashCharges => currentDashCharges;
    public int MaxDashCharges => maxDashCharges;

    public bool HasFlightAbility => hasFlightAbility;
    public bool HasDashAbility => hasDashAbility;
    public int FacingDirection => facingDirection;
    public bool IsMovementDisabled => movementDisabled;

    private Vector2 lastGroundedPosition;
    public Vector2 LastGroundedPosition => lastGroundedPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;
        playerEnergy = GetComponent<PlayerEnergy>();
        normalGravityScale = rb.gravityScale;
        lastGroundedPosition = transform.position;

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        RefreshUnlockedAbilityState();
    }

    private void Start()
    {
        RefreshUnlockedAbilityState();
    }

    public void RefreshUnlockedAbilityState()
    {
        hasFlightAbility = GameStateManager.Instance != null && GameStateManager.Instance.HasFlightAbility;
        hasDashAbility = GameStateManager.Instance != null && GameStateManager.Instance.HasDashAbility;

        currentFlightTime = hasFlightAbility ? maxFlightTime : 0f;
        currentDashCharges = hasDashAbility ? maxDashCharges : 0;

        if (!hasFlightAbility)
        {
            flyHeld = false;
            isInFlightMode = false;
        }

        if (!hasDashAbility)
        {
            dashPressed = false;
            isDashing = false;
            dashDirection = Vector2.zero;
        }
    }

    public void ResetMomentum()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        if (movementDisableCoroutine != null)
        {
            StopCoroutine(movementDisableCoroutine);
            movementDisableCoroutine = null;
        }

        movementDisabled = false;
        currentHorizontalSpeed = 0f;
        horizontalInput = 0f;
        jumpPressed = false;
        flyHeld = false;
        diveHeld = false;
        hasStartedJump = false;
        coyoteTimeCounter = 0f;
        dashPressed = false;
        isDashing = false;
        canDash = true;
        dashDirection = Vector2.zero;
        isInFlightMode = false;
        wasGliding = false;

        if (rb != null)
        {
            rb.gravityScale = normalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void ClearInputState()
    {
        horizontalInput = 0f;
        jumpPressed = false;
        flyHeld = false;
        diveHeld = false;
        dashPressed = false;
    }

    public void OnMoveHorizontal(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();

        if (horizontalInput > 0.01f)
        {
            facingDirection = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            facingDirection = -1;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (movementDisabled)
            return;

        if (context.performed)
        {
            jumpPressed = true;
        }
        else if (context.canceled)
        {
            if (hasStartedJump && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * jumpCutMultiplier
                );
            }
        }
    }

    public void OnFly(InputAction.CallbackContext context)
    {
        flyHeld = !movementDisabled && hasFlightAbility && context.ReadValueAsButton();
    }

    public void OnDive(InputAction.CallbackContext context)
    {
        diveHeld = !movementDisabled && context.ReadValueAsButton();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!hasDashAbility || movementDisabled)
            return;

        if (context.performed)
        {
            dashPressed = true;
        }
    }

    /// <summary>
    /// Cancels current movement control and pushes the player opposite their facing direction.
    /// </summary>
    public void ApplyDamageKnockback()
    {
        if (rb == null)
            return;

        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        if (movementDisableCoroutine != null)
        {
            StopCoroutine(movementDisableCoroutine);
            movementDisableCoroutine = null;
        }

        ClearInputState();
        currentHorizontalSpeed = 0f;
        hasStartedJump = false;
        coyoteTimeCounter = 0f;
        isDashing = false;
        canDash = true;
        dashDirection = Vector2.zero;
        isInFlightMode = false;
        wasGliding = false;
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = Vector2.zero;

        Vector2 knockbackVelocity = new Vector2(
            -facingDirection * damageKnockbackHorizontalSpeed,
            damageKnockbackUpSpeed
        );

        movementDisableCoroutine = StartCoroutine(DamageKnockbackRoutine(knockbackVelocity));
    }

    private void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (isGrounded)
        {
            lastGroundedPosition = transform.position;
            hasStartedJump = false;
            isInFlightMode = false;
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (isGrounded && !wasGrounded)
        {
            if (hasFlightAbility)
            {
                currentFlightTime = maxFlightTime;
            }

            if (hasDashAbility)
            {
                currentDashCharges = maxDashCharges;
            }
        }

        if (movementDisabled)
        {
            jumpPressed = false;
            dashPressed = false;
            return;
        }

        if (hasDashAbility && dashPressed && canDash && !isDashing)
        {
            if (currentDashCharges > 0)
            {
                dashPressed = false;
                dashCoroutine = StartCoroutine(PerformDash(true));
            }
            else if (playerEnergy != null && playerEnergy.TryConsumeCharge())
            {
                dashPressed = false;
                dashCoroutine = StartCoroutine(PerformDash(false));
            }
            else
            {
                dashPressed = false;
            }
        }
        else
        {
            dashPressed = false;
        }
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        Vector2 boxSize = new Vector2(
            bounds.size.x * groundCheckWidthMultiplier,
            groundCheckHeight
        );

        Vector2 boxCenter = new Vector2(
            bounds.center.x,
            bounds.min.y - groundCheckDistance
        );

        return Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            0f,
            groundLayer
        );
    }

    private bool CheckWall(int direction)
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        Vector2 boxSize = new Vector2(
            wallCheckDistance,
            bounds.size.y * wallCheckHeightMultiplier
        );

        float xPosition = direction > 0
            ? bounds.max.x + wallCheckDistance * 0.5f
            : bounds.min.x - wallCheckDistance * 0.5f;

        Vector2 boxCenter = new Vector2(
            xPosition,
            bounds.center.y
        );

        return Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            0f,
            groundLayer
        );
    }

    private void FixedUpdate()
    {
        if (movementDisabled || isDashing)
            return;

        HandleHorizontalMovement();
        HandleJump();
        HandleFlight();
        ApplyBetterFallGravity();
        LimitFallSpeed();
    }

    private void HandleHorizontalMovement()
    {
        bool pressingLeftIntoWall = horizontalInput < 0f && CheckWall(-1);
        bool pressingRightIntoWall = horizontalInput > 0f && CheckWall(1);
        bool pressingIntoWall = pressingLeftIntoWall || pressingRightIntoWall;

        if (pressingIntoWall)
        {
            currentHorizontalSpeed = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isInFlightMode)
        {
            float targetSpeed = horizontalInput * moveSpeed * 2f;
            float rate = Mathf.Abs(targetSpeed) > 0.01f
                ? acceleration * 2f
                : deceleration * 2f;

            currentHorizontalSpeed = Mathf.MoveTowards(
                currentHorizontalSpeed,
                targetSpeed,
                rate * Time.fixedDeltaTime
            );
        }
        else
        {
            currentHorizontalSpeed = horizontalInput * moveSpeed;
        }

        rb.linearVelocity = new Vector2(currentHorizontalSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        bool flightTakesPriority = hasFlightAbility && flyHeld;

        if (!jumpPressed || flightTakesPriority)
        {
            jumpPressed = false;
            return;
        }

        bool canUseJump = isGrounded || coyoteTimeCounter > 0f;

        if (!canUseJump)
        {
            jumpPressed = false;
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        hasStartedJump = true;
        coyoteTimeCounter = 0f;
        jumpPressed = false;
    }

    private void HandleFlight()
    {
        if (!hasFlightAbility)
            return;

        if (isGrounded)
        {
            if (flyHeld)
            {
                isInFlightMode = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, groundTakeoffSpeed);
            }

            return;
        }

        if (flyHeld && currentFlightTime > 0f)
        {
            isInFlightMode = true;

            currentFlightTime -= Time.fixedDeltaTime;
            currentFlightTime = Mathf.Max(currentFlightTime, 0f);

            float newYVelocity = rb.linearVelocity.y + flightAcceleration * Time.fixedDeltaTime;
            newYVelocity = Mathf.Min(newYVelocity, maxRiseSpeed);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYVelocity);
        }
    }

    private void ApplyBetterFallGravity()
    {
        bool isFalling = rb.linearVelocity.y < 0f;
        bool isOutOfFlight = currentFlightTime <= 0f;

        bool isGliding = hasFlightAbility && !isGrounded && flyHeld && isFalling && isOutOfFlight;
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

    private void LimitFallSpeed()
    {
        float currentMaxFallSpeed = maxFallSpeed;

        bool isFalling = rb.linearVelocity.y < 0f;
        bool isOutOfFlight = currentFlightTime <= 0f;

        bool isGliding = hasFlightAbility && !isGrounded && flyHeld && isFalling && isOutOfFlight;
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

    private void OnDrawGizmosSelected()
    {
        if (bodyCollider == null)
            return;

        Bounds bounds = bodyCollider.bounds;

        Gizmos.color = Color.yellow;

        Vector3 groundBoxSize = new Vector3(
            bounds.size.x * groundCheckWidthMultiplier,
            groundCheckHeight,
            0f
        );

        Vector3 groundBoxCenter = new Vector3(
            bounds.center.x,
            bounds.min.y - groundCheckDistance,
            0f
        );

        Gizmos.DrawWireCube(groundBoxCenter, groundBoxSize);

        Gizmos.color = Color.red;

        Vector3 leftWallBoxSize = new Vector3(
            wallCheckDistance,
            bounds.size.y * wallCheckHeightMultiplier,
            0f
        );

        Vector3 leftWallBoxCenter = new Vector3(
            bounds.min.x - wallCheckDistance * 0.5f,
            bounds.center.y,
            0f
        );

        Vector3 rightWallBoxCenter = new Vector3(
            bounds.max.x + wallCheckDistance * 0.5f,
            bounds.center.y,
            0f
        );

        Gizmos.DrawWireCube(leftWallBoxCenter, leftWallBoxSize);
        Gizmos.DrawWireCube(rightWallBoxCenter, leftWallBoxSize);
    }

    private IEnumerator DamageKnockbackRoutine(Vector2 initialVelocity)
    {
        movementDisabled = true;
        rb.gravityScale = 0f;

        float moveDuration = Mathf.Max(0f, damageKnockbackMoveDuration);
        float decay = Mathf.Max(0f, damageKnockbackDecay);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float progress = moveDuration > 0f ? Mathf.Clamp01(elapsed / moveDuration) : 1f;
            float speedMultiplier = Mathf.Exp(-decay * progress);
            rb.linearVelocity = initialVelocity * speedMultiplier;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = normalGravityScale;
        movementDisabled = false;
        RefreshHeldHorizontalInput();
        movementDisableCoroutine = null;
    }

    private void RefreshHeldHorizontalInput()
    {
        if (Keyboard.current == null)
            return;

        float heldHorizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            heldHorizontalInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            heldHorizontalInput += 1f;
        }

        horizontalInput = heldHorizontalInput;

        if (horizontalInput > 0.01f)
        {
            facingDirection = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            facingDirection = -1;
        }
    }

    private IEnumerator PerformDash(bool consumeNormalDashCharge)
    {
        if (!hasDashAbility || mainCamera == null || !canDash)
        {
            dashCoroutine = null;
            yield break;
        }

        if (consumeNormalDashCharge)
        {
            if (currentDashCharges <= 0)
            {
                dashCoroutine = null;
                yield break;
            }

            currentDashCharges--;
        }

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
        dashCoroutine = null;
    }
}
