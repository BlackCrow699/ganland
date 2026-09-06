using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>Persistent JRPG inventory: stackable consumables and simple item use.</summary>
public class InventoryManager : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public int quantity;
        public int maxStack = 99;
        public int healHP;
        public int healMP;
    }

    public static InventoryManager instance;
    public List<Item> items = new List<Item>();

    public static InventoryManager GetOrCreate()
    {
        if (instance != null) return instance;
        instance = FindObjectOfType<InventoryManager>();
        if (instance != null) return instance;
        GameObject holder = new GameObject("InventoryManager");
        instance = holder.AddComponent<InventoryManager>();
        return instance;
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
        EnsureDefaults();
    }

    public void EnsureDefaults()
    {
        if (items == null) items = new List<Item>();
        if (FindItem("potion") == null)
        {
            items.Add(new Item { id = "potion", displayName = "Potion", description = "Restores 30 HP.", quantity = 3, healHP = 30 });
        }
        if (FindItem("ether") == null)
        {
            items.Add(new Item { id = "ether", displayName = "Ether", description = "Restores 20 MP.", quantity = 2, healMP = 20 });
        }
    }

    public Item FindItem(string id)
    {
        if (items == null) return null;
        return items.Find(item => item != null && string.Equals(item.id, id, StringComparison.OrdinalIgnoreCase));
    }

    public int GetQuantity(string id)
    {
        Item item = FindItem(id);
        return item != null ? Mathf.Max(0, item.quantity) : 0;
    }

    public bool AddItem(string id, string displayName, string description, int amount, int healHP = 0, int healMP = 0)
    {
        if (amount <= 0) return false;
        Item item = FindItem(id);
        if (item == null)
        {
            item = new Item { id = id, displayName = displayName, description = description, healHP = healHP, healMP = healMP };
            items.Add(item);
        }
        item.quantity = Mathf.Clamp(item.quantity + amount, 0, Mathf.Max(1, item.maxStack));
        return true;
    }

    public bool TryUseHealingItem(PlayerUnit target, out string message)
    {
        message = "No usable item.";
        if (target == null) return false;
        EnsureDefaults();
        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            if (item == null || item.quantity <= 0 || (item.healHP <= 0 && item.healMP <= 0)) continue;
            int oldHP = target.currentHP;
            int oldMP = target.currentMP;
            target.currentHP = Mathf.Min(target.maxHP, target.currentHP + item.healHP);
            target.currentMP = Mathf.Min(target.maxMP, target.currentMP + item.healMP);
            int healedHP = target.currentHP - oldHP;
            int healedMP = target.currentMP - oldMP;
            if (healedHP <= 0 && healedMP <= 0)
            {
                message = target.unitName + " cannot benefit from " + item.displayName + ".";
                return false;
            }
            item.quantity--;
            target.SaveDataToGameManager();
            message = target.unitName + " uses " + item.displayName + " (" + (healedHP > 0 ? "+" + healedHP + " HP" : "+" + healedMP + " MP") + ").";
            return true;
        }
        return false;
    }

    public bool TryUseItemOnManager(string id, out string message)
    {
        message = "Item cannot be used.";
        GameManager manager = GameManager.instance;
        Item item = FindItem(id);
        if (manager == null || item == null || item.quantity <= 0) return false;
        int oldHP = manager.currentHP;
        int oldMP = manager.currentMP;
        manager.currentHP = Mathf.Min(manager.maxHP, manager.currentHP + item.healHP);
        manager.currentMP = Mathf.Min(manager.maxMP, manager.currentMP + item.healMP);
        int healedHP = manager.currentHP - oldHP;
        int healedMP = manager.currentMP - oldMP;
        if (healedHP <= 0 && healedMP <= 0)
        {
            message = manager.playerName + " cannot benefit from " + item.displayName + ".";
            return false;
        }
        item.quantity--;
        message = manager.playerName + " uses " + item.displayName + ".";
        return true;
    }

    public string BuildSummary()
    {
        EnsureDefaults();
        StringBuilder text = new StringBuilder();
        text.Append("ITEMS\n\n");
        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            if (item == null) continue;
            text.Append(item.displayName).Append("   x").Append(Mathf.Max(0, item.quantity)).Append("\n");
            if (!string.IsNullOrEmpty(item.description)) text.Append("  ").Append(item.description).Append("\n");
        }
        return text.ToString();
    }
}
