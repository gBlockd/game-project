using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player feedback when collecting an important upgrade.
/// 
/// Behavior:
/// - briefly freezes the player,
/// - flashes green several times,
/// - restores control afterward.
/// </summary>
public class PlayerUpgradeFeedback : MonoBehaviour
{
    [Header("Upgrade Feedback")]
    public float freezeDuration = 0.75f;
    public Color flashColor = Color.green;
    public int flashCount = 3;
    public float flashInterval = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isPlayingFeedback;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void PlayUpgradeFeedback()
    {
        if (isPlayingFeedback)
            return;

        StartCoroutine(UpgradeFeedbackRoutine());
    }

    private IEnumerator UpgradeFeedbackRoutine()
    {
        isPlayingFeedback = true;

        FreezePlayer();

        float elapsed = 0f;
        int flashesCompleted = 0;

        while (elapsed < freezeDuration)
        {
            if (spriteRenderer != null && flashesCompleted < flashCount)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);

                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);

                elapsed += flashInterval * 2f;
                flashesCompleted++;
            }
            else
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        UnfreezePlayer();

        isPlayingFeedback = false;
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

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
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

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = true;
        }

        PlayerProjectileAttack projectileAttack = GetComponent<PlayerProjectileAttack>();
        if (projectileAttack != null)
        {
            projectileAttack.enabled = true;
        }
    }
}