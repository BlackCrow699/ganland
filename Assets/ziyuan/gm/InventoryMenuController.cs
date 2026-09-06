using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Simple English inventory screen used as a pause-menu sub-screen.</summary>
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
    public bool autoBuildIfMissing = true;
    public Vector2 panelSize = new Vector2(760f, 520f);

    public bool IsInventoryOpen { get { return inventoryPanel != null && inventoryPanel.activeSelf; } }

    void Awake()
    {
        if (autoBuildIfMissing) BuildMissingUI();
        if (inventoryButton != null) inventoryButton.onClick.AddListener(OpenInventory);
        if (potionButton != null) potionButton.onClick.AddListener(UsePotion);
        if (etherButton != null) etherButton.onClick.AddListener(UseEther);
        if (backButton != null) backButton.onClick.AddListener(CloseInventory);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (Application.isPlaying) RefreshInventory();
    }

    void OnEnable()
    {
        if (!Application.isPlaying && autoBuildIfMissing) BuildMissingUI();
    }

    void BuildMissingUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) return;
        if (pausePanel == null)
        {
            Transform pause = canvas.transform.Find("PausePanel");
            if (pause != null) pausePanel = pause.gameObject;
        }
        if (inventoryPanel == null)
        {
            Transform existing = canvas.transform.Find("InventoryPanel_Auto");
            if (existing != null) inventoryPanel = existing.gameObject;
        }
        if (inventoryPanel == null)
        {
            inventoryPanel = new GameObject("InventoryPanel_Auto");
            inventoryPanel.transform.SetParent(canvas.transform, false);
            Image image = inventoryPanel.AddComponent<Image>();
            image.color = new Color(0.025f, 0.045f, 0.09f, 0.98f);
            Outline outline = inventoryPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.72f, 0.28f, 1f);
            outline.effectDistance = new Vector2(3f, 3f);
            RectTransform rect = inventoryPanel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = panelSize;
            titleText = CreateText(inventoryPanel.transform, "INVENTORY", 28, new Vector2(0f, 205f), new Vector2(680f, 50f), TextAlignmentOptions.Center);
            itemListText = CreateText(inventoryPanel.transform, "", 18, new Vector2(0f, 15f), new Vector2(680f, 340f), TextAlignmentOptions.TopLeft);
            potionButton = CreateButton(inventoryPanel.transform, "USE POTION", new Vector2(-120f, -170f));
            etherButton = CreateButton(inventoryPanel.transform, "USE ETHER", new Vector2(120f, -170f));
            backButton = CreateButton(inventoryPanel.transform, "BACK", new Vector2(0f, -225f));
        }
        if (pausePanel != null && inventoryButton == null)
        {
            Transform existing = pausePanel.transform.Find("INVENTORYButton");
            inventoryButton = existing != null ? existing.GetComponent<Button>() : null;
            if (inventoryButton == null) inventoryButton = CreateButton(pausePanel.transform, "INVENTORY", new Vector2(0f, -205f));
        }
        inventoryPanel.SetActive(false);
    }

    TMP_Text CreateText(Transform parent, string value, int size, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = new Color(0.9f, 0.93f, 1f, 1f);
        text.alignment = alignment;
        text.enableWordWrapping = true;
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return text;
    }

    Button CreateButton(Transform parent, string label, Vector2 position)
    {
        GameObject obj = new GameObject(label + "Button");
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.1f, 0.13f, 0.22f, 1f);
        Button button = obj.AddComponent<Button>();
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(190f, 54f);
        CreateText(obj.transform, label, 18, Vector2.zero, new Vector2(180f, 48f), TextAlignmentOptions.Center);
        return button;
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
