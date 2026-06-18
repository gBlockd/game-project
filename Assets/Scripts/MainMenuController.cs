using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject saveSlotPanel;
    public GameObject optionsPanel;

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

    public void QuitGame()
    {
        Application.Quit();
    }
}