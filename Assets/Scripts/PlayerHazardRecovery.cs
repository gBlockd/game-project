using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles non-lethal hazard recovery for the player.
/// 
/// Behavior:
/// - applies hazard damage,
/// - briefly freezes the player,
/// - returns them to their last grounded position,
/// - restores control afterward.
/// 
/// If the damage kills the player, normal death handling takes over instead.
/// </summary>
public class PlayerHazardRecovery : MonoBehaviour
{
    [Header("Hazard Recovery")]
    public float recoveryPauseDuration = 1f;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private bool isRecovering;

    public bool IsRecovering => isRecovering;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void HandleHazardHit(int damage)
    {
        if (isRecovering)
            return;

        if (playerHealth == null || playerHealth.IsDead)
            return;

        StartCoroutine(HazardRecoveryRoutine(damage));
    }

    private IEnumerator HazardRecoveryRoutine(int damage)
    {
        isRecovering = true;

        Vector2 returnPosition = playerMovement != null
            ? playerMovement.LastGroundedPosition
            : transform.position;

        playerHealth.TakeDamage(damage);

        // If the hit killed the player, let the normal death/respawn flow handle it.
        if (playerHealth.IsDead)
        {
            isRecovering = false;
            yield break;
        }

        FreezePlayer();

        yield return new WaitForSeconds(recoveryPauseDuration);

        transform.position = returnPosition;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerMovement != null)
        {
            playerMovement.ResetMomentum();
        }

        UnfreezePlayer();

        isRecovering = false;
    }

    private void FreezePlayer()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        PlayerProjectileAttack projectileAttack = GetComponent<PlayerProjectileAttack>();
        if (projectileAttack != null)
        {
            projectileAttack.enabled = false;
        }
    }

    private void UnfreezePlayer()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        PlayerProjectileAttack projectileAttack = GetComponent<PlayerProjectileAttack>();
        if (projectileAttack != null)
        {
            projectileAttack.enabled = true;
        }
    }
}