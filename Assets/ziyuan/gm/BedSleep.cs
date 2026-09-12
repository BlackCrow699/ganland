using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Restores the player and respawns defeated encounters at a bed.</summary>
public class BedSleep : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;
    public bool requirePlayerTag = true;
    public bool showPrompt = true;
    public string promptText = "Press F to sleep";
    [Min(1)] public int promptFontSize = 32;
    public Vector3 interactionBoxSize = new Vector3(4f, 2f, 4f);

    private bool playerInside;
    private Transform currentPlayer;
    private bool sleeping;

    void Awake()
    {
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = interactionBoxSize;
    }

    void Update()
    {
        if (playerInside && !sleeping && Input.GetKeyDown(interactKey)) Sleep();
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

    public void Sleep()
    {
        GameManager manager = GameManager.instance;
        if (sleeping || manager == null || currentPlayer == null) return;

        sleeping = true;
        manager.currentHP = manager.maxHP;
        manager.currentMP = manager.maxMP;
        manager.ResetDefeatedEncounters();
        manager.QueuePlayerReturn(currentPlayer.position);
        Debug.Log("Player slept: HP/MP restored and encounters reset.");
        SceneManager.LoadScene("BattleForest");
    }

    void OnGUI()
    {
        if (showPrompt && playerInside && !sleeping)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(1, promptFontSize),
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(Screen.width * 0.5f - 220f, Screen.height - 130f, 440f, 60f), promptText, style);
        }
    }
}
