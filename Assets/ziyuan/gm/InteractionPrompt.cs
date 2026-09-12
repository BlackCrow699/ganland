using UnityEngine;

/// <summary>Shared player-trigger detection and on-screen prompt helpers.</summary>
public static class InteractionPrompt
{
    public static bool IsPlayer(Collider other, bool requirePlayerTag)
    {
        if (other == null) return false;
        return !requirePlayerTag || other.CompareTag("Player") ||
            (other.transform.root != null && other.transform.root.CompareTag("Player"));
    }

    public static Transform GetPlayerTransform(Collider other)
    {
        return other.CompareTag("Player") ? other.transform : other.transform.root;
    }

    public static void Draw(bool show, string text, int fontSize, Vector2 area)
    {
        if (!show) return;
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(1, fontSize),
            alignment = TextAnchor.MiddleCenter
        };
        Rect rect = new Rect(Screen.width * 0.5f - area.x * 0.5f,
            Screen.height - area.y - 30f, area.x, area.y);
        GUI.Label(rect, text, style);
    }
}
