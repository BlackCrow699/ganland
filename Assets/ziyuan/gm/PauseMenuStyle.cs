using UnityEngine;

// Kept for backwards compatibility with existing scene references.
// Visual styling is authored in the Unity editor/assets and is never
// overwritten at runtime.
public class PauseMenuStyle : MonoBehaviour
{
    public Color normalColor = new Color(0.10f, 0.13f, 0.22f, 1f);
    public Color highlightedColor = new Color(0.18f, 0.24f, 0.38f, 1f);
    public Color pressedColor = new Color(0.07f, 0.09f, 0.16f, 1f);
    public Color textColor = new Color(0.92f, 0.95f, 1f, 1f);
    public Color borderColor = new Color(0.95f, 0.72f, 0.28f, 1f);
    public Vector2 buttonSize = new Vector2(220f, 56f);
    public int fontSize = 20;


    // Intentionally empty. Configure all visual properties in the editor.
    public void ApplyStyles() { }
}
