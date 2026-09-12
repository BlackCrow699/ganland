#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One-time editor helper that creates the hand-wired equipment screen.</summary>
public static class EquipmentMenuBuilder
{
    private const string EquipmentButtonPrefabPath = "Assets/ziyuan/gm/EquipmentButton.prefab";

    private static readonly Color PanelColor = new Color(0.06f, 0.10f, 0.18f, 0.98f);
    private static readonly Color ButtonColor = new Color(0.13f, 0.20f, 0.34f, 1f);

    [MenuItem("Tools/Build Equipment Menu")]
    public static void Build()
    {
        Canvas canvas = FindPersistentCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Equipment Menu", "Could not find the persistent Canvas (PauseMenu).", "OK");
            return;
        }

        EquipmentMenuController controller = EnsureController(canvas);
        GameObject rolePanel = BuildRolePanel(canvas.transform);
        GameObject listPanel = BuildListPanel(canvas.transform);
        Button buttonPrefab = EnsureEquipmentButtonPrefab();

        AssignController(controller, rolePanel, listPanel, buttonPrefab);

        rolePanel.SetActive(false);
        listPanel.SetActive(false);
        EditorUtility.SetDirty(canvas.gameObject);
        Selection.activeGameObject = rolePanel;
        Debug.Log("Equipment menu UI built on " + canvas.name + ".");
    }

    static Canvas FindPersistentCanvas()
    {
        PauseMenu pause = Object.FindObjectOfType<PauseMenu>();
        if (pause != null)
        {
            Canvas canvas = pause.GetComponent<Canvas>();
            if (canvas != null) return canvas;
        }

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].GetComponent<PauseMenu>() != null) return canvases[i];
        }
        return null;
    }

    static EquipmentMenuController EnsureController(Canvas canvas)
    {
        EquipmentMenuController controller = canvas.GetComponent<EquipmentMenuController>();
        if (controller == null) controller = canvas.gameObject.AddComponent<EquipmentMenuController>();
        return controller;
    }

    static GameObject BuildRolePanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "RolePanel", new Vector2(760f, 480f));

        CreateText(panel.transform, "CharacterName", "Craven", 26, new Vector2(-230f, 175f), new Vector2(340f, 40f), TextAlignmentOptions.Left);
        CreateText(panel.transform, "Level", "LV 1", 20, new Vector2(-230f, 135f), new Vector2(340f, 34f), TextAlignmentOptions.Left);
        CreateText(panel.transform, "HP", "HP 100/100", 20, new Vector2(-230f, 98f), new Vector2(340f, 34f), TextAlignmentOptions.Left);
        CreateText(panel.transform, "MP", "MP 50/50", 20, new Vector2(-230f, 61f), new Vector2(340f, 34f), TextAlignmentOptions.Left);
        CreateText(panel.transform, "ATK", "ATK 20", 20, new Vector2(-230f, 24f), new Vector2(340f, 34f), TextAlignmentOptions.Left);
        CreateText(panel.transform, "DEF", "DEF 10", 20, new Vector2(-230f, -13f), new Vector2(340f, 34f), TextAlignmentOptions.Left);

        TMP_Text weaponLabel, armorLabel;
        CreateButton(panel.transform, "WeaponSlot", "None", new Vector2(160f, 90f), new Vector2(360f, 50f), out weaponLabel);
        CreateButton(panel.transform, "ArmorSlot", "None", new Vector2(160f, 10f), new Vector2(360f, 50f), out armorLabel);
        CreateButton(panel.transform, "Back", "BACK", new Vector2(0f, -200f), new Vector2(180f, 44f), out _);

        return panel;
    }

    static GameObject BuildListPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "EquipListPanel", new Vector2(560f, 420f));

        CreateText(panel.transform, "ListTitle", "WEAPON", 22, new Vector2(0f, 172f), new Vector2(440f, 40f), TextAlignmentOptions.Center);
        CreateText(panel.transform, "EmptyText", "No equipment owned.", 18, new Vector2(0f, 20f), new Vector2(440f, 40f), TextAlignmentOptions.Center);
        CreateButton(panel.transform, "Back", "BACK", new Vector2(0f, -180f), new Vector2(180f, 44f), out _);

        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0f, 15f);
        contentRect.sizeDelta = new Vector2(480f, 260f);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 8f;
        layout.padding = new RectOffset(6, 6, 6, 6);
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return panel;
    }

    static Button EnsureEquipmentButtonPrefab()
    {
        Button existing = AssetDatabase.LoadAssetAtPath<Button>(EquipmentButtonPrefabPath);
        if (existing != null) return existing;

        GameObject root = new GameObject("EquipmentButton");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(460f, 44f);
        Image image = root.AddComponent<Image>();
        image.color = ButtonColor;
        Button button = root.AddComponent<Button>();
        CreateText(root.transform, "Label", "Item", 18, Vector2.zero, new Vector2(440f, 36f), TextAlignmentOptions.Left);

        GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, EquipmentButtonPrefabPath);
        Object.DestroyImmediate(root);
        return asset != null ? asset.GetComponent<Button>() : null;
    }

    static void AssignController(
        EquipmentMenuController controller,
        GameObject rolePanel,
        GameObject listPanel,
        Button buttonPrefab)
    {
        controller.characterPanel = rolePanel;
        controller.characterNameText = rolePanel.transform.Find("CharacterName").GetComponent<TMP_Text>();
        controller.levelText = rolePanel.transform.Find("Level").GetComponent<TMP_Text>();
        controller.hpText = rolePanel.transform.Find("HP").GetComponent<TMP_Text>();
        controller.mpText = rolePanel.transform.Find("MP").GetComponent<TMP_Text>();
        controller.atkText = rolePanel.transform.Find("ATK").GetComponent<TMP_Text>();
        controller.defText = rolePanel.transform.Find("DEF").GetComponent<TMP_Text>();
        controller.weaponSlotButton = rolePanel.transform.Find("WeaponSlot").GetComponent<Button>();
        controller.weaponSlotText = rolePanel.transform.Find("WeaponSlot/Label").GetComponent<TMP_Text>();
        controller.armorSlotButton = rolePanel.transform.Find("ArmorSlot").GetComponent<Button>();
        controller.armorSlotText = rolePanel.transform.Find("ArmorSlot/Label").GetComponent<TMP_Text>();
        controller.backButton = rolePanel.transform.Find("Back").GetComponent<Button>();

        controller.equipmentListPanel = listPanel;
        controller.listTitleText = listPanel.transform.Find("ListTitle").GetComponent<TMP_Text>();
        controller.listContent = listPanel.transform.Find("Content").GetComponent<RectTransform>();
        controller.equipmentButtonPrefab = buttonPrefab;
        controller.listEmptyText = listPanel.transform.Find("EmptyText").GetComponent<TMP_Text>();
        controller.listBackButton = listPanel.transform.Find("Back").GetComponent<Button>();
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = PanelColor;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        return go;
    }

    static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        int fontSize,
        Vector2 position,
        Vector2 size,
        TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.font = TMP_Settings.defaultFontAsset;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }

    static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 size,
        out TMP_Text labelText)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = ButtonColor;
        Button button = go.AddComponent<Button>();
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        labelText = CreateText(go.transform, "Label", label, 20, Vector2.zero, size, TextAlignmentOptions.Center);
        return button;
    }
}
#endif
