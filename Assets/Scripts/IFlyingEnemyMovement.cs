using UnityEngine;

public interface IFlyingEnemyMovement
{
    bool CanReceiveKnockback { get; }
    Vector2 CurrentVelocity { get; }

    void FreezeMovement();
    void UnfreezeMovement(Vector2 restoredVelocity);
}