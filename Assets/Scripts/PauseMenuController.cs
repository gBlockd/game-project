using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the gameplay pause menu.
///
/// Pausing freezes scaled gameplay time, shows the pause UI, unlocks the cursor,
/// and disables gameplay PlayerInput components so normal player actions are not
/// processed while the menu is open.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject defaultPausePanel;
    public GameObject optionsPanel;

    [Header("Buttons")]
    public GameObject firstPauseButton;
    public GameObject firstOptionsButton;

    [Header("Scene Flow")]
    public string mainMenuSceneName = "Start_Menu";

    [Header("Input")]
    public bool listenForEscapeKey = true;
    public bool disableGameplayPlayerInput = true;

    private readonly List<PlayerInput> disabledGameplayInputs = new List<PlayerInput>();

    private bool isPaused;
    private float previousTimeScale = 1f;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        HidePauseMenuImmediate();
    }

    private void Update()
    {
        if (!listenForEscapeKey || Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Optional Input System callback for a Pause action.
    ///
    /// This is useful if you add a Pause action to PlayerControls. Leave
    /// listenForEscapeKey enabled unless this script is guaranteed to keep
    /// receiving the Pause action while gameplay PlayerInput is disabled.
    /// </summary>
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        ClearGameplayInputBuffers();

        if (disableGameplayPlayerInput)
        {
            DisableGameplayInput();
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowPauseRoot();
        ShowDefaultPauseMenu();
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        HidePauseMenuImmediate();
        RestoreGameplayInput();
        ClearGameplayInputBuffers();

        Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;

        isPaused = false;
    }

    public void ShowDefaultPauseMenu()
    {
        if (defaultPausePanel != null)
        {
            defaultPausePanel.SetActive(true);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        SelectButton(firstPauseButton);
    }

    public void ShowOptions()
    {
        if (defaultPausePanel != null)
        {
            defaultPausePanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }

        SelectButton(firstOptionsButton);
    }

    public void ExitToMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("Main Menu Scene Name must be set before exiting from the pause menu.");
            return;
        }

        HidePauseMenuImmediate();
        RestoreGameplayInput();
        ClearGameplayInputBuffers();

        Time.timeScale = 1f;
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
        isPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        if (!isPaused)
            return;

        RestoreGameplayInput();
        Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
    }

    private void ShowPauseRoot()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogWarning("Pause Menu Panel is not assigned on PauseMenuController.");
            return;
        }

        pauseMenuPanel.SetActive(true);
    }

    private void HidePauseMenuImmediate()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (defaultPausePanel != null)
        {
            defaultPausePanel.SetActive(true);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void DisableGameplayInput()
    {
        disabledGameplayInputs.Clear();

        PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput playerInput in playerInputs)
        {
            if (playerInput == null || !playerInput.enabled)
                continue;

            playerInput.enabled = false;
            disabledGameplayInputs.Add(playerInput);
        }
    }

    private void RestoreGameplayInput()
    {
        foreach (PlayerInput playerInput in disabledGameplayInputs)
        {
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }

        disabledGameplayInputs.Clear();
    }

    private void ClearGameplayInputBuffers()
    {
        PlayerMovement[] movementScripts = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement movement in movementScripts)
        {
            if (movement != null)
            {
                movement.ResetMomentum();
            }
        }

        PlayerAttack[] attackScripts = FindObjectsOfType<PlayerAttack>();
        foreach (PlayerAttack attack in attackScripts)
        {
            if (attack != null)
            {
                attack.SendMessage("ClearInputState", SendMessageOptions.DontRequireReceiver);
            }
        }

        PlayerProjectileAttack[] projectileAttackScripts = FindObjectsOfType<PlayerProjectileAttack>();
        foreach (PlayerProjectileAttack projectileAttack in projectileAttackScripts)
        {
            if (projectileAttack != null)
            {
                projectileAttack.SendMessage("ClearInputState", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void SelectButton(GameObject button)
    {
        if (button == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
}
