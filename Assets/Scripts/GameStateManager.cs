using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores persistent game state across scene transitions.
/// 
/// Currently stores:
/// - player health,
/// - unlocked player abilities,
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

    [Header("Unlocked Abilities")]
    [SerializeField] private bool flightUnlocked;
    [SerializeField] private bool dashUnlocked;

    private bool hasInitialSpawn;
    private string initialSpawnSceneName;
    private Vector3 initialSpawnPosition;

    private bool hasCheckpoint;
    private string checkpointSceneName;
    private Vector3 checkpointPosition;

    private readonly HashSet<string> activatedButtons = new HashSet<string>();
    private readonly HashSet<string> killedEnemyIds = new HashSet<string>();

    private readonly Dictionary<string, List<LockedDoor>> registeredDoors = new Dictionary<string, List<LockedDoor>>();

    public bool HasFlightAbility => flightUnlocked;
    public bool HasDashAbility => dashUnlocked;

    public bool HasInitialSpawn => hasInitialSpawn;
    public string InitialSpawnSceneName => initialSpawnSceneName;
    public Vector3 InitialSpawnPosition => initialSpawnPosition;

    public bool HasCheckpoint => hasCheckpoint;
    public string CheckpointSceneName => checkpointSceneName;
    public Vector3 CheckpointPosition => checkpointPosition;

    /// <summary>
    /// Keeps one persistent game state object alive across scene loads.
    /// Duplicate managers destroy themselves so state is not split between objects.
    /// </summary>
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

    /// <summary>
    /// Stores the player's current and maximum health, clamping current health into range.
    /// </summary>
    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
    }

    /// <summary>
    /// Restores the stored player health value to the current maximum.
    /// </summary>
    public void ResetHealthToFull()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Permanently unlocks the flight ability for the current run.
    /// </summary>
    public void UnlockFlight()
    {
        flightUnlocked = true;
    }

    /// <summary>
    /// Permanently unlocks the dash ability for the current run.
    /// </summary>
    public void UnlockDash()
    {
        dashUnlocked = true;
    }

    /// <summary>
    /// Checks whether a specific player ability has already been unlocked.
    /// </summary>
    public bool IsAbilityUnlocked(PlayerAbilityType abilityType)
    {
        if (abilityType == PlayerAbilityType.Flight)
            return flightUnlocked;

        if (abilityType == PlayerAbilityType.Dash)
            return dashUnlocked;

        return false;
    }

    /// <summary>
    /// Unlocks the requested ability type using the matching ability-specific method.
    /// </summary>
    public void UnlockAbility(PlayerAbilityType abilityType)
    {
        if (abilityType == PlayerAbilityType.Flight)
        {
            UnlockFlight();
        }
        else if (abilityType == PlayerAbilityType.Dash)
        {
            UnlockDash();
        }
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

    /// <summary>
    /// Stores the latest checkpoint scene and position for future respawns.
    /// </summary>
    public void SetCheckpoint(string sceneName, Vector3 respawnPosition)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        checkpointSceneName = sceneName;
        checkpointPosition = respawnPosition;
        hasCheckpoint = true;
    }

    /// <summary>
    /// Marks a button as activated and opens any registered doors linked to that button ID.
    /// </summary>
    public void SetButtonActivated(string buttonId)
    {
        if (string.IsNullOrWhiteSpace(buttonId))
            return;

        if (activatedButtons.Contains(buttonId))
            return;

        activatedButtons.Add(buttonId);
        OpenRegisteredDoors(buttonId);
    }

    /// <summary>
    /// Checks whether a button ID has already been activated in this run.
    /// </summary>
    public bool IsButtonActivated(string buttonId)
    {
        if (string.IsNullOrWhiteSpace(buttonId))
            return false;

        return activatedButtons.Contains(buttonId);
    }

    /// <summary>
    /// Registers a locked door so it can be opened when its linked button activates.
    /// </summary>
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

    /// <summary>
    /// Updates every registered door linked to a newly activated button.
    /// Null entries are removed because doors can disappear during scene changes.
    /// </summary>
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

    /// <summary>
    /// Records an enemy ID as killed so it can stay gone until enemy state is cleared.
    /// </summary>
    public void RegisterKilledEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
            return;

        killedEnemyIds.Add(enemyId);
    }

    /// <summary>
    /// Checks whether an enemy ID has already been killed.
    /// </summary>
    public bool IsEnemyKilled(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
            return false;

        return killedEnemyIds.Contains(enemyId);
    }

    /// <summary>
    /// Clears temporary enemy-death state, allowing enemies to respawn after player death.
    /// </summary>
    public void ClearKilledEnemies()
    {
        killedEnemyIds.Clear();
    }
}
