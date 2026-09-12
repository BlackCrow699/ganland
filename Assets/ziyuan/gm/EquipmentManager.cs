using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterLoadout
{
    public string characterId;
    public EquipmentData weapon;
    public EquipmentData armor;
}

/// <summary>Persistent wallet, shared equipment pool, and per-character loadouts.</summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Wallet")]
    public int gold = 150;

    [Header("Shared Pool")]
    public List<EquipmentData> ownedEquipment = new List<EquipmentData>();

    [Header("Character Loadouts")]
    public List<CharacterLoadout> loadouts = new List<CharacterLoadout>();

    public static EquipmentManager GetOrCreate()
    {
        if (instance != null) return instance;
        instance = FindObjectOfType<EquipmentManager>();
        if (instance != null) return instance;
        GameObject holder = new GameObject("EquipmentManager");
        instance = holder.AddComponent<EquipmentManager>();
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
    }

    public bool OwnsEquipment(EquipmentData equipment)
    {
        return equipment != null && ownedEquipment != null && ownedEquipment.Contains(equipment);
    }

    public bool BuyEquipment(EquipmentData equipment)
    {
        if (equipment == null || OwnsEquipment(equipment)) return false;
        if (gold < equipment.price) return false;

        gold -= equipment.price;
        if (ownedEquipment == null) ownedEquipment = new List<EquipmentData>();
        ownedEquipment.Add(equipment);
        return true;
    }

    public CharacterLoadout GetOrCreateLoadout(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) characterId = "craven";
        for (int i = 0; i < loadouts.Count; i++)
        {
            if (loadouts[i] != null && loadouts[i].characterId == characterId) return loadouts[i];
        }
        CharacterLoadout loadout = new CharacterLoadout { characterId = characterId };
        loadouts.Add(loadout);
        return loadout;
    }

    public int EquipmentAttackBonus(string characterId)
    {
        CharacterLoadout loadout = GetLoadout(characterId);
        return loadout != null && loadout.weapon != null ? loadout.weapon.attackBonus : 0;
    }

    public int EquipmentDefenseBonus(string characterId)
    {
        CharacterLoadout loadout = GetLoadout(characterId);
        return loadout != null && loadout.armor != null ? loadout.armor.defenseBonus : 0;
    }

    public bool Equip(string characterId, EquipmentData equipment)
    {
        if (!OwnsEquipment(equipment)) return false;
        CharacterLoadout loadout = GetOrCreateLoadout(characterId);
        if (equipment.slot == EquipmentSlot.Weapon) loadout.weapon = equipment;
        else loadout.armor = equipment;
        return true;
    }

    public bool Unequip(string characterId, EquipmentSlot slot)
    {
        CharacterLoadout loadout = GetOrCreateLoadout(characterId);
        if (slot == EquipmentSlot.Weapon) loadout.weapon = null;
        else loadout.armor = null;
        return true;
    }

    public int TotalAttack(string characterId)
    {
        return (GameManager.instance != null ? GameManager.instance.attackPower : 0) + EquipmentAttackBonus(characterId);
    }

    public int TotalDefense(string characterId)
    {
        return (GameManager.instance != null ? GameManager.instance.defense : 0) + EquipmentDefenseBonus(characterId);
    }

    CharacterLoadout GetLoadout(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) return null;
        for (int i = 0; i < loadouts.Count; i++)
        {
            if (loadouts[i] != null && loadouts[i].characterId == characterId) return loadouts[i];
        }
        return null;
    }
}
