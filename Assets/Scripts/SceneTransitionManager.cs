using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles room-to-room scene transitions by remembering which entrance
/// the player should use when the next scene loads.
/// 
/// Transition flow:
/// - fade to black,
/// - hold on black,
/// - load the destination scene,
/// - move the player to the requested entrance,
/// - fade back to gameplay.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Timing")]
    public float blackScreenDuration = 1f;

    [Header("References")]
    public ScreenFader screenFader;

    private string pendingEntranceId;
    private bool hasPendingEntrance;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void TransitionToScene(string sceneName, string entranceId)
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        StartCoroutine(TransitionRoutine(sceneName, entranceId));
    }

    private IEnumerator TransitionRoutine(string sceneName, string entranceId)
    {
        isTransitioning = true;

        pendingEntranceId = entranceId;
        hasPendingEntrance = true;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            FreezePlayer(player.gameObject);
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeToBlack();
        }

        yield return new WaitForSecondsRealtime(blackScreenDuration);

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FinishTransitionAfterSceneLoad());
    }

    private IEnumerator FinishTransitionAfterSceneLoad()
    {
        if (!isTransitioning)
            yield break;

        yield return null;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player == null)
        {
            isTransitioning = false;
            hasPendingEntrance = false;
            pendingEntranceId = string.Empty;
            yield break;
        }

        if (hasPendingEntrance)
        {
            SceneEntrance[] entrances = FindObjectsByType<SceneEntrance>();

            for (int i = 0; i < entrances.Length; i++)
            {
                if (entrances[i].entranceId == pendingEntranceId)
                {
                    player.transform.position = entrances[i].transform.position;

                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }

                    PlayerMovement movement = player.GetComponent<PlayerMovement>();
                    if (movement != null)
                    {
                        movement.ResetMomentum();
                    }

                    hasPendingEntrance = false;
                    pendingEntranceId = string.Empty;
                    break;
                }
            }

            if (hasPendingEntrance)
            {
                Debug.LogWarning("No SceneEntrance found with ID: " + pendingEntranceId);
                hasPendingEntrance = false;
                pendingEntranceId = string.Empty;
            }
        }

        UnfreezePlayer(player.gameObject);

        if (screenFader != null)
        {
            yield return screenFader.FadeFromBlack();
        }

        isTransitioning = false;
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
}