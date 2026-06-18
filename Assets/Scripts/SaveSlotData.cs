using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data for one saved run.
///
/// This class stores only plain progression data so Unity can write it to JSON.
/// Runtime-only values, such as current health and current energy, are restored
/// to full when a saved run loads at its checkpoint.
/// </summary>
[Serializable]
public class SaveSlotData
{
    public int slotIndex;
    public bool isInUse;
    public string lastSavedUtc;

    public int maxHealth = 100;

    public bool flightUnlocked;
    public bool dashUnlocked;

    public string newGameStartSceneName;

    public bool hasInitialSpawn;
    public string initialSpawnSceneName;
    public Vector3 initialSpawnPosition;

    public bool hasCheckpoint;
    public string checkpointSceneName;
    public Vector3 checkpointPosition;

    public List<string> activatedButtonIds = new List<string>();

    // Reserved for later enemies that should stay defeated across sessions.
    public List<string> persistentDefeatedEnemyIds = new List<string>();

    public void EnsureListsExist()
    {
        if (activatedButtonIds == null)
        {
            activatedButtonIds = new List<string>();
        }

        if (persistentDefeatedEnemyIds == null)
        {
            persistentDefeatedEnemyIds = new List<string>();
        }
    }
}
