using UnityEngine;

/// <summary>
/// A melee-activated button that opens a matching locked door.
/// 
/// Behavior:
/// - can be activated by the player's melee attack,
/// - only activates once,
/// - stores its activated state in the RespawnManager,
/// - remains activated after respawn/scene reload,
/// - flashes when struck to confirm hit detection.
/// </summary>
public class DoorButton : MonoBehaviour
{
    [Header("Button")]
    public string buttonId;

    private bool isActivated;
    private ButtonFlash buttonFlash;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        buttonFlash = GetComponent<ButtonFlash>();
    }

    private void Start()
    {
        if (RespawnManager.Instance != null && RespawnManager.Instance.IsButtonActivated(buttonId))
        {
            isActivated = true;
        }
    }

    public void Activate()
    {
        if (buttonFlash != null)
        {
            buttonFlash.Flash();
        }

        if (isActivated)
            return;

        isActivated = true;

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetButtonActivated(buttonId);
        }
    }
}