using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Pause menu controller for a hand-built Canvas panel.</summary>
[ExecuteAlways]
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public SaveLoadController saveLoadController;
    public bool pauseWithEscape = true;

    private bool isPaused;

    void Awake()
    {
        if (saveLoadController == null) saveLoadController = FindObjectOfType<SaveLoadController>();
        if (FindObjectOfType<InventoryMenuController>() == null)
        {
            gameObject.AddComponent<InventoryMenuController>();
        }
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        if (!Application.isPlaying && GetComponent<InventoryMenuController>() == null)
        {
            gameObject.AddComponent<InventoryMenuController>();
        }
    }

    void Update()
    {
        if (!pauseWithEscape || !Input.GetKeyDown(KeyCode.Escape)) return;

        // Party is a sub-screen of the pause menu. Close that screen first so
        // Escape never leaves both Party and PausePanel visible at once.
        PartyMenuController partyMenu = GetComponent<PartyMenuController>();
        if (partyMenu == null) partyMenu = FindObjectOfType<PartyMenuController>();
        if (partyMenu != null && partyMenu.IsPartyOpen)
        {
            partyMenu.CloseParty();
            return;
        }

        InventoryMenuController inventoryMenu = GetComponent<InventoryMenuController>();
        if (inventoryMenu == null) inventoryMenu = FindObjectOfType<InventoryMenuController>();
        if (inventoryMenu != null && inventoryMenu.IsInventoryOpen)
        {
            inventoryMenu.CloseInventory();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        ClearUISelection();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        ClearUISelection();
        Time.timeScale = 1f;
    }

    void ClearUISelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
    }

    public void SaveGame()
    {
        if (saveLoadController != null) saveLoadController.SaveGame();
    }

    public void LoadGame()
    {
        if (saveLoadController == null) return;
        Time.timeScale = 1f;
        isPaused = false;
        saveLoadController.LoadGame();
    }

    public void QuitToDesktop()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
