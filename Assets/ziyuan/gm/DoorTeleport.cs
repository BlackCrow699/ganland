using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generic door/entrance teleporter.
/// - Leave Scene Name empty to teleport to a Destination Point in the current scene.
/// - Set Scene Name to load another scene and place the player at Spawn Position.
/// </summary>
public class DoorTeleport : MonoBehaviour
{
    [Header("Destination (same scene)")]
    [Tooltip("Where to move the player in the current scene. Used when Scene Name is empty.")]
    public Transform destinationPoint;

    [Header("Scene Loading (optional)")]
    [Tooltip("If set, loads this scene instead of using Destination Point.")]
    public string sceneName = "";
    [Tooltip("Spawn point id in the target scene.")]
    public string spawnPointId = "";

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;
    public bool requirePlayerTag = true;

    [Header("Prompt")]
    public bool showPrompt = true;
    public string promptText = "Press F to enter";
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
        return InteractionPrompt.IsPlayer(other, requirePlayerTag);
    }

    Transform GetPlayerTransform(Collider other)
    {
        return InteractionPrompt.GetPlayerTransform(other);
    }

    public void TeleportPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("DoorTeleport: no player inside the trigger.", this);
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            LoadSceneAndSpawn();
        }
        else
        {
            TeleportToDestination();
        }
    }

    void TeleportToDestination()
    {
        if (destinationPoint == null)
        {
            Debug.LogError("DoorTeleport: Destination Point is not assigned.", this);
            return;
        }

        SetPlayerPosition(destinationPoint.position);
        playerInside = false;
        currentPlayer = null;
    }

    void LoadSceneAndSpawn()
    {
        if (GameManager.instance != null)
        {
            if (string.IsNullOrEmpty(spawnPointId))
            {
                Debug.LogWarning("DoorTeleport: Scene Name is set but Spawn Point Id is empty.", this);
            }
            GameManager.instance.QueuePlayerSpawn(spawnPointId);
        }
        else
        {
            Debug.LogWarning("DoorTeleport: GameManager instance not found. Spawn position will not be restored.", this);
        }

        playerInside = false;
        currentPlayer = null;
        SceneManager.LoadScene(sceneName);
    }

    void SetPlayerPosition(Vector3 position)
    {
        currentPlayer.position = position;
        Rigidbody body = currentPlayer.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.position = position;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    void OnGUI()
    {
        InteractionPrompt.Draw(showPrompt && playerInside, promptText, promptFontSize, promptArea);
    }
}
