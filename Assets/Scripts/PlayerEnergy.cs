using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tracks player energy, converts full energy bars into spendable charges,
/// and spends those charges on healing, berserk attack boost, or extra dashes.
/// </summary>
public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy")]
    public int energyPerCharge = 100;
    public int maxCharges = 1;
    public int meleeHitEnergy = 12;
    public int projectileHitEnergy = 5;
    public int markedComboBonusEnergy = 6;

    [Header("Healing")]
    public int healAmountPerTick = 1;
    public float healTickInterval = 0.2f;
    public float healDuration = 10f;

    [Header("Berserk")]
    public float berserkDuration = 5f;
    public Color berserkColor = Color.red;

    private int currentEnergy;
    private bool isHealing;
    private bool isBerserkActive;
    private Coroutine healCoroutine;
    private Coroutine berserkCoroutine;

    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int CurrentEnergy => currentEnergy;
    public int EnergyPerCharge => energyPerCharge;
    public int MaxCharges => maxCharges;
    public int MaxEnergy => energyPerCharge * maxCharges;
    public int CurrentCharges => energyPerCharge <= 0 ? 0 : currentEnergy / energyPerCharge;
    public float CurrentChargeFill => energyPerCharge <= 0 ? 0f : (currentEnergy % energyPerCharge) / (float)energyPerCharge;
    public bool HasFullCharge => CurrentCharges > 0;
    public bool IsHealing => isHealing;
    public bool IsBerserkActive => isBerserkActive;
    public bool CanGainEnergy => !isBerserkActive;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Input System callback for spending one charge to heal over time.
    /// Bind this action to E.
    /// </summary>
    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TryStartHealing();
        }
    }

    /// <summary>
    /// Input System callback for spending one charge to enter berserk mode.
    /// Bind this action to R.
    /// </summary>
    public void OnBoostAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TryStartBerserk();
        }
    }

    /// <summary>
    /// Adds energy from combat rewards unless energy gain is currently blocked.
    /// Energy is clamped to the current maximum, which is based on maxCharges.
    /// </summary>
    public void AddEnergy(int amount)
    {
        if (amount <= 0 || !CanGainEnergy)
            return;

        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, MaxEnergy);
    }

    /// <summary>
    /// Spends exactly one full charge, preserving any extra partial energy.
    /// </summary>
    public bool TryConsumeCharge()
    {
        if (!HasFullCharge)
            return false;

        currentEnergy = Mathf.Max(0, currentEnergy - energyPerCharge);
        return true;
    }

    /// <summary>
    /// Attempts to spend one charge and begin healing over time.
    /// Healing is disabled while berserk is active.
    /// </summary>
    public bool TryStartHealing()
    {
        if (isHealing || isBerserkActive || playerHealth == null)
            return false;

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            return false;

        if (!TryConsumeCharge())
            return false;

        healCoroutine = StartCoroutine(HealOverTimeRoutine());
        return true;
    }

    /// <summary>
    /// Attempts to spend one charge and enter berserk mode.
    /// </summary>
    public bool TryStartBerserk()
    {
        if (isBerserkActive)
            return false;

        if (!TryConsumeCharge())
            return false;

        berserkCoroutine = StartCoroutine(BerserkRoutine());
        return true;
    }

    private IEnumerator HealOverTimeRoutine()
    {
        isHealing = true;

        float elapsed = 0f;
        while (elapsed < healDuration)
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmountPerTick);
            }

            yield return new WaitForSeconds(healTickInterval);
            elapsed += healTickInterval;
        }

        isHealing = false;
        healCoroutine = null;
    }

    private IEnumerator BerserkRoutine()
    {
        isBerserkActive = true;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = berserkColor;
        }

        yield return new WaitForSeconds(berserkDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isBerserkActive = false;
        berserkCoroutine = null;
    }
}
