using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player death, stage reset, fade-to-black, and respawn.
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