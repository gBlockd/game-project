using UnityEngine;

/// <summary>
/// Shared movement contract for flying enemies that can be interrupted by knockback.
///
/// Knockback scripts use this interface so they can freeze, move, and restore
/// different flying enemy types without needing to know each enemy's exact class.
/// </summary>
public interface IFlyingEnemyMovement
{
    /// <summary>
    /// Whether this enemy can currently be interrupted by knockback.
    /// Returns false during attacks, wind-ups, freezes, or other protected states.
    /// </summary>
    bool CanReceiveKnockback { get; }

    /// <summary>
    /// The enemy's current movement velocity before knockback starts.
    /// Knockback stores this so normal movement can continue smoothly afterward.
    /// </summary>
    Vector2 CurrentVelocity { get; }

    /// <summary>
    /// Temporarily stops the enemy's own movement logic so knockback can control position.
    /// </summary>
    void FreezeMovement();

    /// <summary>
    /// Restores normal movement after knockback and gives the enemy back its previous velocity.
    /// </summary>
    /// <param name="restoredVelocity">Velocity to resume with after knockback ends.</param>
    void UnfreezeMovement(Vector2 restoredVelocity);
}
