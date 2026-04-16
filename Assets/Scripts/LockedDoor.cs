using UnityEngine;

/// <summary>
/// A door that disappears permanently once its matching button is activated.
/// 
/// Behavior:
/// - registers itself with the GameStateManager,
/// - checks whether its linked button has already been activated,
/// - disables itself when that button is activated,
/// - remains open after respawn/scene reload.
/// </summary>
public class LockedDoor : MonoBehaviour
{
    [Header("Door")]
    public string buttonId;

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterDoor(this);
            UpdateDoorState();
        }
    }

    public void UpdateDoorState()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsButtonActivated(buttonId))
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        gameObject.SetActive(false);
    }
}