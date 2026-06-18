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

    public void StartSaveSlot(int slotIndex)
    {
        if (string.IsNullOrWhiteSpace(firstGameplaySceneName))
        {
            Debug.LogWarning("First Gameplay Scene Name must be set on MainMenuController before loading a save slot.");
            return;
        }

        GameStateManager gameStateManager = GetOrCreateGameStateManager();
        gameStateManager.LoadOrCreateSaveSlot(slotIndex, firstGameplaySceneName);
        gameStateManager.ApplySavedSpawnAfterNextSceneLoad();

        string sceneToLoad = gameStateManager.GetSceneNameForCurrentSave(firstGameplaySceneName);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
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
