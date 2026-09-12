using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Pause menu controller for a hand-built Canvas panel.</summary>
[ExecuteAlways]
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public SaveLoadController saveLoadController;
    public bool pauseWithEscape = true;

    private bool isPaused;

    private static PauseMenu persistentPauseMenu;

    void Awake()
    {
        if (Application.isPlaying)
        {
            if (persistentPauseMenu != null && persistentPauseMenu != this)
            {
                Destroy(gameObject);
                return;
            }

            persistentPauseMenu = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        if (saveLoadController == null) saveLoadController = FindObjectOfType<SaveLoadController>();
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (Application.isPlaying)
        {
            EnsureSingleEventSystem();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isPlaying) return;
        EnsureSingleEventSystem();
    }

    void EnsureSingleEventSystem()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);
        EventSystem keep = null;

        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] != null && systems[i].gameObject.scene.name == "DontDestroyOnLoad")
            {
                keep = systems[i];
                break;
            }
        }
        if (keep == null && systems.Length > 0)
        {
            keep = systems[0];
        }

        if (keep == null)
        {
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            keep = go.GetComponent<EventSystem>();
        }

        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] != null && systems[i] != keep)
            {
                Destroy(systems[i].gameObject);
            }
        }

        DontDestroyOnLoad(keep.gameObject);
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

        EquipmentMenuController equipmentMenu = GetComponent<EquipmentMenuController>();
        if (equipmentMenu == null) equipmentMenu = FindObjectOfType<EquipmentMenuController>();
        if (equipmentMenu != null && equipmentMenu.TryCloseTopMost())
        {
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
