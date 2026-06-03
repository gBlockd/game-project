using UnityEngine;

/// <summary>
/// Coordinates two ProjectileChargerEnemy instances as a paired encounter.
/// 
/// If either member activates, the other activates as well.
/// If one member is destroyed, the remaining member falls back to the normal
/// single ProjectileChargerEnemy behavior.
/// 
/// Pair behavior:
/// - Pair member 1 begins orbit attacks above the player.
/// - Pair member 2 begins orbit attacks below the player.
/// - Both rotate clockwise around the player.
/// - If one dies, the other resets to standard single-charger behavior.
/// </summary>
public class PairedProjectileChargerEnemy : MonoBehaviour
{
    [Header("Pair Setup")]
    public ProjectileChargerEnemy selfCharger;
    public PairedProjectileChargerEnemy pairedCharger;

    [Range(1, 2)]
    public int pairNumber = 1;

    private bool isPairActive;
    private bool hasFallenBackToSingle;

    private void Awake()
    {
        if (selfCharger == null)
        {
            selfCharger = GetComponent<ProjectileChargerEnemy>();
        }
    }

    private void Update()
    {
        if (hasFallenBackToSingle || isPairActive)
            return;

        if (selfCharger == null || selfCharger.player == null)
            return;

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            selfCharger.player.position
        );

        if (distanceToPlayer <= selfCharger.activationRange)
        {
            ActivatePair();
        }
    }

    private void ActivatePair()
    {
        if (isPairActive)
            return;

        ActivateThisMember();

        if (pairedCharger != null)
        {
            pairedCharger.ActivateFromPartner();
        }
    }

    private void ActivateFromPartner()
    {
        if (isPairActive)
            return;

        ActivateThisMember();
    }

    private void ActivateThisMember()
    {
        isPairActive = true;

        if (selfCharger == null)
            return;

        selfCharger.ActivateExternally();
        selfCharger.ResetAttackLoop();

        Vector2 sideOffset = pairNumber == 1
            ? Vector2.left
            : Vector2.right;

        float orbitStartAngle = pairNumber == 1
            ? 90f
            : 270f;

        selfCharger.SetCircleTargetOffset(sideOffset);
        selfCharger.SetOrbitStartAngleDegrees(orbitStartAngle);
        selfCharger.SetOrbitClockwise(true);
        selfCharger.ForceOrbitPattern();
    }

    public void FallBackToSingleAI()
    {
        if (hasFallenBackToSingle)
            return;

        hasFallenBackToSingle = true;
        isPairActive = false;

        if (selfCharger != null)
        {
            selfCharger.ActivateExternally();

            // Standard single-charger orbit behavior starts above the player.
            selfCharger.SetOrbitStartAngleDegrees(90f);
            selfCharger.SetOrbitClockwise(true);

            selfCharger.ResetAttackLoop();
        }
    }

    private void OnDestroy()
    {
        if (!isPairActive || hasFallenBackToSingle)
            return;

        if (pairedCharger != null)
        {
            pairedCharger.FallBackToSingleAI();
        }
    }
}