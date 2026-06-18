using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stores the currently loaded run state across scene transitions.
///
/// GameStateManager keeps the live in-memory state that gameplay scripts read
/// and write. SaveSystem turns that live state into one JSON file per save slot.
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

    private int activeSaveSlotIndex = -1;
    private string newGameStartSceneName;
    private bool shouldApplySavedSpawnOnNextSceneLoad;

    private bool hasInitialSpawn;
    private string initialSpawnSceneName;
    private Vector3 initialSpawnPosition;

    private bool hasCheckpoint;
    private string checkpointSceneName;
    private Vector3 checkpointPosition;

    private readonly HashSet<string> activatedButtons = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> killedEnemyIds = new HashSet<string>(StringComparer.Ordinal);

    private readonly Dictionary<string, List<LockedDoor>> registeredDoors = new Dictionary<string, List<LockedDoor>>();

    public int ActiveSaveSlotIndex => activeSaveSlotIndex;
    public bool HasActiveSaveSlot => activeSaveSlotIndex > 0;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Checks whether the requested slot already has a save file.
    /// </summary>
    public bool SaveSlotExists(int slotIndex)
    {
        return SaveSystem.SaveSlotExists(slotIndex);
    }

    /// <summary>
    /// Loads an existing slot or creates a new run in that slot.
    ///
    /// The fallback scene is used for a brand-new run, before any initial spawn
    /// or checkpoint has been recorded.
    /// </summary>
    public void LoadOrCreateSaveSlot(int slotIndex, string fallbackStartSceneName)
    {
        if (slotIndex <= 0)
        {
            Debug.LogWarning("Save slot index must be greater than zero.");
            return;
        }

        activeSaveSlotIndex = slotIndex;
        newGameStartSceneName = fallbackStartSceneName;

        SaveSlotData data = SaveSystem.LoadSlot(slotIndex);

        if (data == null)
        {
            ResetRunState(fallbackStartSceneName);
            data = SaveSystem.CreateNewSlot(slotIndex, fallbackStartSceneName, maxHealth);
        }
        else if (string.IsNullOrWhiteSpace(data.newGameStartSceneName) && !string.IsNullOrWhiteSpace(fallbackStartSceneName))
        {
            data.newGameStartSceneName = fallbackStartSceneName;
            SaveSystem.SaveSlot(data);
        }

        ApplySaveData(data);
    }

    /// <summary>
    /// Returns the scene that should load for the currently selected save slot.
    /// </summary>
    public string GetSceneNameForCurrentSave(string fallbackStartSceneName)
    {
        if (hasCheckpoint && !string.IsNullOrWhiteSpace(checkpointSceneName))
            return checkpointSceneName;

        if (hasInitialSpawn && !string.IsNullOrWhiteSpace(initialSpawnSceneName))
            return initialSpawnSceneName;

        if (!string.IsNullOrWhiteSpace(newGameStartSceneName))
            return newGameStartSceneName;

        return fallbackStartSceneName;
    }

    /// <summary>
    /// Tells the manager to place the player at the saved spawn after the next scene load.
    /// This is meant for loading from the main menu and avoids interfering with room transitions.
    /// </summary>
    public void ApplySavedSpawnAfterNextSceneLoad()
    {
        shouldApplySavedSpawnOnNextSceneLoad = true;
    }

    /// <summary>
    /// Writes the current run state into the active save slot.
    /// </summary>
    public bool SaveCurrentSlot()
    {
        if (!HasActiveSaveSlot)
            return false;

        SaveSystem.SaveSlot(CreateSaveDataFromCurrentState());
        return true;
    }

    /// <summary>
    /// Stores the player's current and maximum health, clamping current health into range.
    /// </summary>
    public void SetHealth(int newCurrentHealth, int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
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

        SaveCurrentSlot();
    }

    /// <summary>
    /// Stores the latest checkpoint scene and position, then saves the active slot.
    /// </summary>
    public void SetCheckpoint(string sceneName, Vector3 respawnPosition)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        checkpointSceneName = sceneName;
        checkpointPosition = respawnPosition;
        hasCheckpoint = true;

        SaveCurrentSlot();
    }

    /// <summary>
    /// Marks a button as activated and opens any registered doors linked to that button ID.
    /// Button state is written to disk the next time a checkpoint saves the run.
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
    /// Records an enemy ID as killed for the current in-memory life only.
    /// These enemies still reset on player death and are not written to save files yet.
    /// </summary>
    public void RegisterKilledEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
            return;

        killedEnemyIds.Add(enemyId);
    }

    /// <summary>
    /// Checks whether an enemy ID has already been killed in the current in-memory life.
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

    private void ResetRunState(string fallbackStartSceneName)
    {
        currentHealth = Mathf.Max(1, maxHealth);
        maxHealth = Mathf.Max(1, maxHealth);

        flightUnlocked = false;
        dashUnlocked = false;

        newGameStartSceneName = fallbackStartSceneName;

        hasInitialSpawn = false;
        initialSpawnSceneName = string.Empty;
        initialSpawnPosition = Vector3.zero;

        hasCheckpoint = false;
        checkpointSceneName = string.Empty;
        checkpointPosition = Vector3.zero;

        activatedButtons.Clear();
        killedEnemyIds.Clear();
        registeredDoors.Clear();
    }

    private void ApplySaveData(SaveSlotData data)
    {
        if (data == null)
            return;

        data.EnsureListsExist();

        activeSaveSlotIndex = data.slotIndex;
        newGameStartSceneName = data.newGameStartSceneName;

        maxHealth = Mathf.Max(1, data.maxHealth);
        currentHealth = Mathf.Clamp(data.currentHealth, 0, maxHealth);

        flightUnlocked = data.flightUnlocked;
        dashUnlocked = data.dashUnlocked;

        hasInitialSpawn = data.hasInitialSpawn;
        initialSpawnSceneName = data.initialSpawnSceneName;
        initialSpawnPosition = data.initialSpawnPosition;

        hasCheckpoint = data.hasCheckpoint;
        checkpointSceneName = data.checkpointSceneName;
        checkpointPosition = data.checkpointPosition;

        activatedButtons.Clear();
        AddIdsToSet(data.activatedButtonIds, activatedButtons);

        killedEnemyIds.Clear();
        registeredDoors.Clear();
    }

    private SaveSlotData CreateSaveDataFromCurrentState()
    {
        return new SaveSlotData
        {
            slotIndex = activeSaveSlotIndex,
            isInUse = true,
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            flightUnlocked = flightUnlocked,
            dashUnlocked = dashUnlocked,
            newGameStartSceneName = newGameStartSceneName,
            hasInitialSpawn = hasInitialSpawn,
            initialSpawnSceneName = initialSpawnSceneName,
            initialSpawnPosition = initialSpawnPosition,
            hasCheckpoint = hasCheckpoint,
            checkpointSceneName = checkpointSceneName,
            checkpointPosition = checkpointPosition,
            activatedButtonIds = CopyIdsToList(activatedButtons)
        };
    }

    private void AddIdsToSet(List<string> ids, HashSet<string> targetSet)
    {
        if (ids == null || targetSet == null)
            return;

        for (int i = 0; i < ids.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(ids[i]))
            {
                targetSet.Add(ids[i]);
            }
        }
    }

    private List<string> CopyIdsToList(HashSet<string> sourceSet)
    {
        List<string> ids = new List<string>();

        if (sourceSet == null)
            return ids;

        foreach (string id in sourceSet)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                ids.Add(id);
            }
        }

        ids.Sort(StringComparer.Ordinal);
        return ids;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!shouldApplySavedSpawnOnNextSceneLoad)
            return;

        shouldApplySavedSpawnOnNextSceneLoad = false;
        StartCoroutine(ApplySavedSpawnAfterSceneLoad(scene));
    }

    private IEnumerator ApplySavedSpawnAfterSceneLoad(Scene scene)
    {
        yield return null;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player == null)
            yield break;

        Vector3 spawnPosition;
        bool hasSpawnPosition = TryGetSavedSpawnPosition(scene.name, out spawnPosition);

        if (!hasSpawnPosition)
        {
            SpawnPoint defaultSpawnPoint = FindAnyObjectByType<SpawnPoint>();
            if (defaultSpawnPoint != null)
            {
                spawnPosition = defaultSpawnPoint.transform.position;
                hasSpawnPosition = true;
            }
        }

        if (!hasSpawnPosition)
            yield break;

        player.transform.position = spawnPosition;
        ResetPlayerMotion(player.gameObject);
    }

    private bool TryGetSavedSpawnPosition(string sceneName, out Vector3 spawnPosition)
    {
        if (hasCheckpoint && string.Equals(sceneName, checkpointSceneName, StringComparison.Ordinal))
        {
            spawnPosition = checkpointPosition;
            return true;
        }

        if (hasInitialSpawn && string.Equals(sceneName, initialSpawnSceneName, StringComparison.Ordinal))
        {
            spawnPosition = initialSpawnPosition;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private void ResetPlayerMotion(GameObject playerObject)
    {
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ResetMomentum();
            movement.RefreshUnlockedAbilityState();
        }
    }
}
