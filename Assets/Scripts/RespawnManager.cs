using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player death, stage reset, fade-to-black, respawn,
/// and persistent stage state such as activated buttons.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn Timing")]
    public float deathFreezeDuration = 1f;
    public float blackScreenDuration = 2f;

    [Header("References")]
    public ScreenFader screenFader;

    private bool isRespawning;
    private bool hasCustomRespawnPoint;
    private Vector3 currentRespawnPoint;

    // Persistent state that survives scene reloads.
    private readonly HashSet<string> activatedButtons = new HashSet<string>();

    // Runtime door registry by button ID.
    private readonly Dictionary<string, List<LockedDoor>> registeredDoors = new Dictionary<string, List<LockedDoor>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void HandlePlayerDeath(PlayerHealth deadPlayer)
    {
        if (isRespawning || deadPlayer == null)
            return;

        StartCoroutine(RespawnRoutine(deadPlayer));
    }

    public void SetRespawnPoint(Vector3 respawnPosition)
    {
        currentRespawnPoint = respawnPosition;
        hasCustomRespawnPoint = true;
    }

    public void SetButtonActivated(string buttonId)
    {
        if (string.IsNullOrWhiteSpace(buttonId))
            return;

        if (activatedButtons.Contains(buttonId))
            return;

        activatedButtons.Add(buttonId);
        OpenRegisteredDoors(buttonId);
    }

    public bool IsButtonActivated(string buttonId)
    {
        if (string.IsNullOrWhiteSpace(buttonId))
            return false;

        return activatedButtons.Contains(buttonId);
    }

    public void RegisterDoor(LockedDoor door)
    {
        if (door == null || string.IsNullOrWhiteSpace(door.buttonId))
            return;

        if (!registeredDoors.ContainsKey(door.buttonId))
        {
            registeredDoors[door.buttonId] = new List<LockedDoor>();
        }

        if (!registeredDoors[door.buttonId].Contains(door))
        {
            registeredDoors[door.buttonId].Add(door);
        }
    }

    private void OpenRegisteredDoors(string buttonId)
    {
        if (!registeredDoors.TryGetValue(buttonId, out List<LockedDoor> doors))
            return;

        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i] != null)
            {
                doors[i].UpdateDoorState();
            }
        }
    }

    private IEnumerator RespawnRoutine(PlayerHealth deadPlayer)
    {
        isRespawning = true;

        FreezePlayer(deadPlayer.gameObject);
        yield return new WaitForSeconds(deathFreezeDuration);

        SetPlayerVisible(deadPlayer.gameObject, false);

        if (screenFader != null)
        {
            yield return screenFader.FadeToBlack();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        yield return null;

        PlayerHealth newPlayer = FindAnyObjectByType<PlayerHealth>();
        SpawnPoint defaultSpawnPoint = FindAnyObjectByType<SpawnPoint>();

        if (newPlayer != null)
        {
            FreezePlayer(newPlayer.gameObject);
            SetPlayerVisible(newPlayer.gameObject, false);

            if (hasCustomRespawnPoint)
            {
                newPlayer.transform.position = currentRespawnPoint;
            }
            else if (defaultSpawnPoint != null)
            {
                newPlayer.transform.position = defaultSpawnPoint.transform.position;
            }

            newPlayer.ResetToFullHealth();
        }

        yield return new WaitForSeconds(blackScreenDuration);

        if (newPlayer != null)
        {
            SetPlayerVisible(newPlayer.gameObject, true);
            UnfreezePlayer(newPlayer.gameObject);
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeFromBlack();
        }

        isRespawning = false;
    }

    private void FreezePlayer(GameObject playerObject)
    {
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerAttack playerAttack = playerObject.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        PlayerProjectileAttack projectileAttack = playerObject.GetComponent<PlayerProjectileAttack>();
        if (projectileAttack != null)
        {
            projectileAttack.enabled = false;
        }
    }

    private void UnfreezePlayer(GameObject playerObject)
    {
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }

        PlayerAttack playerAttack = playerObject.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        PlayerProjectileAttack projectileAttack = playerObject.GetComponent<PlayerProjectileAttack>();
        if (projectileAttack != null)
        {
            projectileAttack.enabled = true;
        }
    }

    private void SetPlayerVisible(GameObject playerObject, bool visible)
    {
        SpriteRenderer[] renderers = playerObject.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }
    }
}