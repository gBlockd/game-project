using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotPanel;
    public GameObject optionsPanel;

    [Header("Save Slots")]
    public string firstGameplaySceneName;

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        saveSlotPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void ShowSaveSlots()
    {
        mainMenuPanel.SetActive(false);
        saveSlotPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        saveSlotPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void StartSaveSlot1()
    {
        StartSaveSlot(1);
    }

    public void StartSaveSlot2()
    {
        StartSaveSlot(2);
    }

    public void StartSaveSlot3()
    {
        StartSaveSlot(3);
    }

    public void StartFreshSaveSlot1()
    {
        StartFreshSaveSlot(1);
    }

    public void StartFreshSaveSlot2()
    {
        StartFreshSaveSlot(2);
    }

    public void StartFreshSaveSlot3()
    {
        StartFreshSaveSlot(3);
    }

    public void ClearSaveSlot1()
    {
        ClearSaveSlot(1);
    }

    public void ClearSaveSlot2()
    {
        ClearSaveSlot(2);
    }

    public void ClearSaveSlot3()
    {
        ClearSaveSlot(3);
    }

    /// <summary>
    /// Loads an existing save slot, or creates a new save file if the slot is empty.
    /// </summary>
    public void StartSaveSlot(int slotIndex)
    {
        if (!HasFirstGameplayScene())
            return;

        GameStateManager gameStateManager = GetOrCreateGameStateManager();
        gameStateManager.LoadOrCreateSaveSlot(slotIndex, firstGameplaySceneName);
        gameStateManager.ApplySavedSpawnAfterNextSceneLoad();

        string sceneToLoad = gameStateManager.GetSceneNameForCurrentSave(firstGameplaySceneName);
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Deletes the selected slot first, then starts that slot as a brand-new run.
    /// </summary>
    public void StartFreshSaveSlot(int slotIndex)
    {
        if (!HasFirstGameplayScene())
            return;

        if (!DeleteSaveSlotData(slotIndex))
            return;

        StartSaveSlot(slotIndex);
    }

    /// <summary>
    /// Deletes the selected slot without leaving the save-slot menu.
    /// </summary>
    public void ClearSaveSlot(int slotIndex)
    {
        DeleteSaveSlotData(slotIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool HasFirstGameplayScene()
    {
        if (!string.IsNullOrWhiteSpace(firstGameplaySceneName))
            return true;

        Debug.LogWarning("First Gameplay Scene Name must be set on MainMenuController before loading a save slot.");
        return false;
    }

    private bool DeleteSaveSlotData(int slotIndex)
    {
        if (GameStateManager.Instance != null)
        {
            return GameStateManager.Instance.DeleteSaveSlot(slotIndex);
        }

        return SaveSystem.TryDeleteSlot(slotIndex);
    }

    private GameStateManager GetOrCreateGameStateManager()
    {
        if (GameStateManager.Instance != null)
        {
            return GameStateManager.Instance;
        }

        GameObject managerObject = new GameObject("GameStateManager");
        return managerObject.AddComponent<GameStateManager>();
    }
}
