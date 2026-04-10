using UnityEngine;

/// <summary>
/// A door that disappears permanently once its matching button is activated.
/// 
/// Behavior:
/// - checks the RespawnManager for its linked button state,
/// - disables itself when that button has been activated,
/// - remains open after respawn/scene reload.
/// </summary>
public class LockedDoor : MonoBehaviour
{
    [Header("Door")]
    public string buttonId;

    private void Start()
    {
        UpdateDoorState();
    }

    public void UpdateDoorState()
    {
        if (RespawnManager.Instance != null && RespawnManager.Instance.IsButtonActivated(buttonId))
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        gameObject.SetActive(false);
    }
}