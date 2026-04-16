using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores persistent game state across scene transitions.
/// 
/// Currently stores:
/// - player health,
/// - initial spawn scene and position,
/// - active checkpoint scene and position,
/// - activated buttons,
/// - killed enemy IDs,
/// - registered locked doors.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Player State")]
    public int currentHealth = 100;
    public int maxHealth = 100;

    private bool hasInitialSpawn;
    private string initialSpawnSceneName;
    private Vector3 initialSpawnPosition;

    private bool hasCheckpoint;
    private string checkpointSceneName;
    private Vector3 checkpointPosition;

    private readonly HashSet<string> activatedButtons = new HashSet<string>();
    private readonly HashSet<string> killedEnemyIds = new HashSet<string>();

    private readonly Dictionary<string, List<LockedDoor>> registeredDoors = new Dictionary<string, List<LockedDoor>>();

    public bool HasInitialSpawn => hasInitialSpawn;
    public string InitialSpawnSceneName => initialSpawnSceneName;
    public Vector3 InitialSpawnPosition => initialSpawnPosition;

    public bool HasCheckpoint => hasCheckpoint;
    public string CheckpointSceneName => checkpointSceneName;
    public Vector3 CheckpointPosition => checkpointPosition;

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
    }

    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
    }

    public void ResetHealthToFull()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Registers the initial spawn point only once, and only before any checkpoint has been activated.
    /// </summary>
    public void SetInitialSpawn(string sceneName, Vector3 respawnPosition)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (hasInitialSpawn || hasCheckpoint)
            return;

        initialSpawnSceneName = sceneName;
        initialSpawnPosition = respawnPosition;
        hasInitialSpawn = true;
    }

    public void SetCheckpoint(string sceneName, Vector3 respawnPosition)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        checkpointSceneName = sceneName;
        checkpointPosition = respawnPosition;
        hasCheckpoint = true;
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

        for (int i = doors.Count - 1; i >= 0; i--)
        {
            if (doors[i] == null)
            {
                doors.RemoveAt(i);
                continue;
            }

            doors[i].UpdateDoorState();
        }
    }

    public void RegisterKilledEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
            return;

        killedEnemyIds.Add(enemyId);
    }

    public bool IsEnemyKilled(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
            return false;

        return killedEnemyIds.Contains(enemyId);
    }

    public void ClearKilledEnemies()
    {
        killedEnemyIds.Clear();
    }
}