using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Hand-wired inventory screen in the pause menu.</summary>
[ExecuteAlways]
public class InventoryMenuController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject pausePanel;
    public TMP_Text titleText;
    public TMP_Text itemListText;
    public Button inventoryButton;
    public Button backButton;
    public Button potionButton;
    public Button etherButton;

    public bool IsInventoryOpen { get { return inventoryPanel != null && inventoryPanel.activeSelf; } }

    void Awake()
    {
        if (inventoryButton != null) inventoryButton.onClick.AddListener(OpenInventory);
        if (potionButton != null) potionButton.onClick.AddListener(UsePotion);
        if (etherButton != null) etherButton.onClick.AddListener(UseEther);
        if (backButton != null) backButton.onClick.AddListener(CloseInventory);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (Application.isPlaying) RefreshInventory();
    }

    public void OpenInventory()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void RefreshInventory()
    {
        if (itemListText != null) itemListText.text = InventoryManager.GetOrCreate().BuildSummary();
    }

    public void UsePotion()
    {
        UseItem("potion");
    }

    public void UseEther()
    {
        UseItem("ether");
    }

    void UseItem(string id)
    {
        string message;
        if (!InventoryManager.GetOrCreate().TryUseItemOnManager(id, out message))
        {
            Debug.Log(message);
            return;
        }
        Debug.Log(message);
        RefreshInventory();
    }
}
