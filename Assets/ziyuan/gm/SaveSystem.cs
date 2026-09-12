using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>File-only save service. Game rules stay in GameManager; this class only serializes them.</summary>
public static class SaveSystem
{
    [Serializable]
    public class CharacterLoadoutSave
    {
        public string characterId;
        public string weaponId;
        public string armorId;
    }

    [Serializable]
    public class SaveData
    {
        public string sceneName;
        public float playerX;
        public float playerY;
        public float playerZ;
        public string playerName;
        public int level;
        public int currentExp;
        public int maxHP;
        public int currentHP;
        public int maxMP;
        public int currentMP;
        public int attackPower;
        public int defense;
        public int speed;
        public string[] unlockedSkillIds;
        public string[] inventoryIds;
        public int[] inventoryQuantities;
        public string[] defeatedEncounterIds;
        public Vector3[] defeatedEncounterPositions;
        public int gold;
        public string[] ownedEquipmentIds;
        public List<CharacterLoadoutSave> loadouts;
        public long savedAtTicks;
    }

    public static string FilePath(string slot = "slot1")
    {
        string safeSlot = string.IsNullOrEmpty(slot) ? "slot1" : slot;
        return Path.Combine(Application.persistentDataPath, safeSlot + ".json");
    }

    public static void Save(string slot, string sceneName, Vector3 playerPosition, GameManager manager)
    {
        if (manager == null) throw new InvalidOperationException("GameManager is not available.");

        InventoryManager inventory = InventoryManager.instance;
        if (inventory == null) inventory = InventoryManager.GetOrCreate();
        EquipmentManager equipment = EquipmentManager.GetOrCreate();
        List<string> inventoryIds = new List<string>();
        List<int> inventoryQuantities = new List<int>();
        if (inventory != null && inventory.items != null)
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                InventoryManager.Item item = inventory.items[i];
                if (item == null || string.IsNullOrEmpty(item.id)) continue;
                inventoryIds.Add(item.id);
                inventoryQuantities.Add(Mathf.Max(0, item.quantity));
            }
        }

        List<string> ownedEquipmentIds = new List<string>();
        if (equipment.ownedEquipment != null)
        {
            for (int i = 0; i < equipment.ownedEquipment.Count; i++)
            {
                EquipmentData item = equipment.ownedEquipment[i];
                if (item != null && !string.IsNullOrEmpty(item.equipmentId))
                {
                    ownedEquipmentIds.Add(item.equipmentId);
                }
            }
        }

        List<CharacterLoadoutSave> loadoutSaves = new List<CharacterLoadoutSave>();
        if (equipment.loadouts != null)
        {
            for (int i = 0; i < equipment.loadouts.Count; i++)
            {
                CharacterLoadout loadout = equipment.loadouts[i];
                if (loadout == null || string.IsNullOrEmpty(loadout.characterId)) continue;
                loadoutSaves.Add(new CharacterLoadoutSave
                {
                    characterId = loadout.characterId,
                    weaponId = loadout.weapon != null ? loadout.weapon.equipmentId : "",
                    armorId = loadout.armor != null ? loadout.armor.equipmentId : ""
                });
            }
        }

        SaveData data = new SaveData
        {
            sceneName = sceneName,
            playerX = playerPosition.x,
            playerY = playerPosition.y,
            playerZ = playerPosition.z,
            playerName = manager.playerName,
            level = manager.level,
            currentExp = manager.currentExp,
            maxHP = manager.maxHP,
            currentHP = manager.currentHP,
            maxMP = manager.maxMP,
            currentMP = manager.currentMP,
            attackPower = manager.attackPower,
            defense = manager.defense,
            speed = manager.speed,
            unlockedSkillIds = manager.unlockedSkillIds != null ? manager.unlockedSkillIds.ToArray() : new string[0],
            inventoryIds = inventoryIds.ToArray(),
            inventoryQuantities = inventoryQuantities.ToArray(),
            defeatedEncounterIds = manager.defeatedEncounterIds != null ? manager.defeatedEncounterIds.ToArray() : new string[0],
            defeatedEncounterPositions = manager.defeatedEncounterPositions != null ? manager.defeatedEncounterPositions.ToArray() : new Vector3[0],
            gold = equipment.gold,
            ownedEquipmentIds = ownedEquipmentIds.ToArray(),
            loadouts = loadoutSaves,
            savedAtTicks = DateTime.UtcNow.Ticks
        };

        File.WriteAllText(FilePath(slot), JsonUtility.ToJson(data, true));
        Debug.Log("Game saved to: " + FilePath(slot));
    }

    public static bool TryLoad(string slot, out SaveData data)
    {
        string path = FilePath(slot);
        data = null;
        if (!File.Exists(path)) return false;

        try
        {
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            return data != null;
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not load save: " + exception.Message);
            return false;
        }
    }

    public static void ApplyToManager(SaveData data, GameManager manager)
    {
        if (data == null || manager == null) return;
        manager.playerName = data.playerName;
        manager.level = data.level;
        manager.currentExp = data.currentExp;
        manager.maxHP = data.maxHP;
        manager.currentHP = data.currentHP;
        manager.maxMP = data.maxMP > 0 ? data.maxMP : 50;
        manager.currentMP = Mathf.Clamp(data.currentMP, 0, manager.maxMP);
        manager.attackPower = data.attackPower;
        manager.defense = data.defense;
        manager.speed = data.speed;
        manager.unlockedSkillIds = data.unlockedSkillIds != null ? new List<string>(data.unlockedSkillIds) : new List<string>();
        manager.hasPlayerRuntimeState = true;

        InventoryManager inventory = InventoryManager.GetOrCreate();
        if (inventory != null && data.inventoryIds != null)
        {
            for (int i = 0; i < data.inventoryIds.Length; i++)
            {
                InventoryManager.Item item = inventory.FindItem(data.inventoryIds[i]);
                if (item != null)
                {
                    int quantity = data.inventoryQuantities != null && i < data.inventoryQuantities.Length ? data.inventoryQuantities[i] : 0;
                    item.quantity = Mathf.Max(0, quantity);
                }
            }
        }

        if (data.defeatedEncounterIds != null)
        {
            manager.defeatedEncounterIds = new List<string>(data.defeatedEncounterIds);
        }
        if (data.defeatedEncounterPositions != null)
        {
            manager.defeatedEncounterPositions = new List<Vector3>(data.defeatedEncounterPositions);
        }

        EquipmentManager equipment = EquipmentManager.GetOrCreate();
        equipment.gold = data.gold;

        if (equipment.ownedEquipment == null) equipment.ownedEquipment = new List<EquipmentData>();
        equipment.ownedEquipment.Clear();
        EquipmentDatabase database = GameManager.instance != null ? GameManager.instance.equipmentDatabase : null;
        if (database != null && data.ownedEquipmentIds != null)
        {
            for (int i = 0; i < data.ownedEquipmentIds.Length; i++)
            {
                EquipmentData item = database.Find(data.ownedEquipmentIds[i]);
                if (item != null) equipment.ownedEquipment.Add(item);
            }
        }
        if (equipment.loadouts == null) equipment.loadouts = new List<CharacterLoadout>();
        equipment.loadouts.Clear();
        if (database != null && data.loadouts != null)
        {
            for (int i = 0; i < data.loadouts.Count; i++)
            {
                CharacterLoadoutSave save = data.loadouts[i];
                if (save == null || string.IsNullOrEmpty(save.characterId)) continue;
                CharacterLoadout loadout = new CharacterLoadout { characterId = save.characterId };
                loadout.weapon = database.Find(save.weaponId);
                loadout.armor = database.Find(save.armorId);
                equipment.loadouts.Add(loadout);
            }
        }
    }

    public static bool HasSave(string slot = "slot1")
    {
        return File.Exists(FilePath(slot));
    }
}
