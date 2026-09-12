using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared runtime UI construction helpers used by auto-built menus.</summary>
public static class UIHelper
{
    public static readonly Color TextColor = new Color(0.92f, 0.95f, 1f, 1f);

    public static TMP_Text CreateText(Transform parent, string value, int fontSize, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = TextColor;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return text;
    }

    public static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 buttonSize, Vector2 textSize, int fontSize, Color buttonColor)
    {
        GameObject obj = new GameObject(label + "Button");
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = buttonColor;
        Button button = obj.AddComponent<Button>();
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = buttonSize;
        CreateText(obj.transform, label, fontSize, Vector2.zero, textSize, TextAlignmentOptions.Center);
        return button;
    }
}
