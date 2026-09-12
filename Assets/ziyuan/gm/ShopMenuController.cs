using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Auto-built shop screen on the persistent pause Canvas.</summary>
public class ShopMenuController : MonoBehaviour
{
    private static ShopMenuController instance;

    public GameObject shopPanel;
    public TMP_Text goldText;
    public TMP_Text itemListText;
    public Button closeButton;
    public RectTransform buyButtonRoot;
    public Vector2 panelSize = new Vector2(760f, 520f);

    private List<EquipmentData> currentItems = new List<EquipmentData>();
    private readonly List<Button> buyButtons = new List<Button>();

    private static readonly Color ButtonColor = new Color(0.12f, 0.16f, 0.24f, 1f);

    public static void Open(List<EquipmentData> items)
    {
        if (instance == null) instance = FindObjectOfType<ShopMenuController>();
        if (instance == null)
        {
            GameObject holder = new GameObject("ShopMenuController");
            holder.AddComponent<ShopMenuController>();
            instance = holder.GetComponent<ShopMenuController>();
        }
        instance.OpenShop(items);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OpenShop(List<EquipmentData> items)
    {
        currentItems = items != null ? items : new List<EquipmentData>();
        EnsureBuilt();
        Refresh();
        Time.timeScale = 0f;
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    void EnsureBuilt()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            PauseMenu pause = FindObjectOfType<PauseMenu>();
            if (pause != null) canvas = pause.GetComponentInParent<Canvas>();
        }
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        if (shopPanel == null)
        {
            shopPanel = new GameObject("ShopPanel_Auto");
            shopPanel.transform.SetParent(canvas.transform, false);
            Image image = shopPanel.AddComponent<Image>();
            image.color = new Color(0.03f, 0.05f, 0.1f, 0.98f);
            RectTransform rect = shopPanel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = panelSize;

            UIHelper.CreateText(shopPanel.transform, "SHOP", 28, new Vector2(0f, 210f), new Vector2(680f, 50f), TextAlignmentOptions.Center);
            goldText = UIHelper.CreateText(shopPanel.transform, "", 20, new Vector2(0f, 150f), new Vector2(680f, 40f), TextAlignmentOptions.Center);
            itemListText = UIHelper.CreateText(shopPanel.transform, "", 18, new Vector2(0f, 30f), new Vector2(680f, 220f), TextAlignmentOptions.TopLeft);

            GameObject root = new GameObject("BuyButtons");
            root.transform.SetParent(shopPanel.transform, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, -80f);
            rootRect.sizeDelta = new Vector2(680f, 220f);
            buyButtonRoot = rootRect;

            closeButton = UIHelper.CreateButton(shopPanel.transform, "CLOSE", new Vector2(0f, -230f), new Vector2(300f, 46f), new Vector2(280f, 40f), 16, ButtonColor);
            closeButton.onClick.AddListener(CloseShop);
        }
    }

    void Refresh()
    {
        if (goldText != null) goldText.text = "Gold: " + EquipmentManager.GetOrCreate().gold;

        if (itemListText != null)
        {
            StringBuilder text = new StringBuilder("ITEMS\n");
            for (int i = 0; i < currentItems.Count; i++)
            {
                EquipmentData item = currentItems[i];
                if (item == null) continue;
                text.Append(i + 1).Append(". ").Append(item.displayName).Append("   ").Append(item.price).Append("G\n");
                if (!string.IsNullOrEmpty(item.description)) text.Append("   ").Append(item.description).Append("\n");
            }
            itemListText.text = text.ToString();
        }

        RebuildBuyButtons();
    }

    void RebuildBuyButtons()
    {
        for (int i = 0; i < buyButtons.Count; i++)
        {
            if (buyButtons[i] != null) Destroy(buyButtons[i].gameObject);
        }
        buyButtons.Clear();
        if (buyButtonRoot == null) return;

        for (int i = 0; i < currentItems.Count; i++)
        {
            EquipmentData item = currentItems[i];
            if (item == null) continue;
            Button button = UIHelper.CreateButton(buyButtonRoot, "BUY " + item.displayName, new Vector2(0f, 40f - i * 55f), new Vector2(300f, 46f), new Vector2(280f, 40f), 16, ButtonColor);
            int index = i;
            button.onClick.AddListener(() => BuyItem(index));
            buyButtons.Add(button);
        }
    }

    public void BuyItem(int index)
    {
        if (index < 0 || index >= currentItems.Count) return;
        EquipmentData item = currentItems[index];
        Debug.Log(EquipmentManager.GetOrCreate().BuyEquipment(item) ? "Bought " + item.displayName : "Cannot buy " + item.displayName);
        Refresh();
    }

    public void CloseShop()
    {
        Time.timeScale = 1f;
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}
