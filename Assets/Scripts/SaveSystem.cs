using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads and writes save-slot files.
///
/// Each slot is stored as a separate JSON file in Application.persistentDataPath,
/// which is the standard Unity location for user save data.
/// </summary>
public static class SaveSystem
{
    private const string SaveFilePrefix = "save_slot_";
    private const string SaveFileExtension = ".json";

    public static string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(
            Application.persistentDataPath,
            SaveFilePrefix + slotIndex + SaveFileExtension
        );
    }

    public static bool SaveSlotExists(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    public static SaveSlotData LoadSlot(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);

        if (!File.Exists(filePath))
            return null;

        try
        {
            string json = File.ReadAllText(filePath);
            SaveSlotData data = JsonUtility.FromJson<SaveSlotData>(json);

            if (data == null)
                return null;

            data.slotIndex = slotIndex;
            data.EnsureListsExist();
            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to load save slot " + slotIndex + ": " + exception.Message);
            return null;
        }
    }

    public static void SaveSlot(SaveSlotData data)
    {
        if (data == null || data.slotIndex <= 0)
            return;

        try
        {
            data.isInUse = true;
            data.lastSavedUtc = DateTime.UtcNow.ToString("o");
            data.EnsureListsExist();

            Directory.CreateDirectory(Application.persistentDataPath);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSaveFilePath(data.slotIndex), json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to save slot " + data.slotIndex + ": " + exception.Message);
        }
    }

    public static void DeleteSlot(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static SaveSlotData CreateNewSlot(int slotIndex, string newGameStartSceneName, int maxHealth)
    {
        SaveSlotData data = new SaveSlotData
        {
            slotIndex = slotIndex,
            isInUse = true,
            newGameStartSceneName = newGameStartSceneName,
            maxHealth = Mathf.Max(1, maxHealth)
        };

        SaveSlot(data);
        return data;
    }
}
