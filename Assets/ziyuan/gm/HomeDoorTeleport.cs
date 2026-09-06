using UnityEngine;

/// <summary>Teleports the player between two points in the current scene.</summary>
public class HomeDoorTeleport : MonoBehaviour
{
    [Header("Destination")]
    public Transform destinationPoint;
    public KeyCode interactKey = KeyCode.F;
    public bool requirePlayerTag = true;

    [Header("Prompt")]
    public bool showPrompt = true;
    public string promptText = "Press F to enter home";
    public int promptFontSize = 28;
    public Vector2 promptArea = new Vector2(520f, 60f);

    private bool playerInside;
    private Transform currentPlayer;

    void Update()
    {
        if (!playerInside || !Input.GetKeyDown(interactKey)) return;
        TeleportPlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = true;
        currentPlayer = GetPlayerTransform(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = false;
        currentPlayer = null;
    }

    bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (!requirePlayerTag) return true;
        return other.CompareTag("Player") || (other.transform.root != null && other.transform.root.CompareTag("Player"));
    }

    Transform GetPlayerTransform(Collider other)
    {
        if (other.CompareTag("Player")) return other.transform;
        return other.transform.root;
    }

    public void TeleportPlayer()
    {
        if (destinationPoint == null)
        {
            Debug.LogError("HomeDoorTeleport: Destination Point is not assigned.");
            return;
        }
        if (currentPlayer == null)
        {
            Debug.LogError("HomeDoorTeleport: Player is not inside the trigger.");
            return;
        }

        currentPlayer.position = destinationPoint.position;
        Rigidbody body = currentPlayer.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.position = destinationPoint.position;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        playerInside = false;
        currentPlayer = null;
    }

    void OnGUI()
    {
        if (!showPrompt || !playerInside) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.Max(1, promptFontSize);
        style.alignment = TextAnchor.MiddleCenter;
        Rect area = new Rect(Screen.width * 0.5f - promptArea.x * 0.5f,
            Screen.height - promptArea.y - 30f, promptArea.x, promptArea.y);
        GUI.Label(area, promptText, style);
    }
}
