using System.Collections.Generic;
using UnityEngine;

/// <summary>Opens the shop when the player is nearby and presses the interact key.</summary>
public class ShopKeeper : MonoBehaviour
{
    [Header("Shop")]
    public List<EquipmentData> shopItems = new List<EquipmentData>();

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;
    public bool requirePlayerTag = true;

    [Header("Prompt")]
    public bool showPrompt = true;
    public string promptText = "Press F to open shop";
    public int promptFontSize = 28;
    public Vector2 promptArea = new Vector2(520f, 60f);

    private bool playerInside;

    void Update()
    {
        if (playerInside && Input.GetKeyDown(interactKey))
        {
            ShopMenuController.Open(shopItems);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other)) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other)) playerInside = false;
    }

    bool IsPlayer(Collider other)
    {
        return InteractionPrompt.IsPlayer(other, requirePlayerTag);
    }

    void OnGUI()
    {
        InteractionPrompt.Draw(showPrompt && playerInside, promptText, promptFontSize, promptArea);
    }
}
