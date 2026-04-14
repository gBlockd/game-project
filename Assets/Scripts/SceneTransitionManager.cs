using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles room-to-room scene transitions by remembering which entrance
/// the player should use when the next scene loads.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private string pendingEntranceId;
    private bool hasPendingEntrance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        pendingEntranceId = entranceId;
        hasPendingEntrance = true;

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingEntrance)
            return;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player == null)
            return;

        SceneEntrance[] entrances = FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None);

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
                return;
            }
        }

        Debug.LogWarning("No SceneEntrance found with ID: " + pendingEntranceId);
        hasPendingEntrance = false;
        pendingEntranceId = string.Empty;
    }
}