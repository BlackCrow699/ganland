using UnityEngine;

public enum EquipmentSlot
{
    Weapon,
    Armor
}

[CreateAssetMenu(menuName = "Game Data/Equipment", fileName = "EquipmentData")]
public sealed class EquipmentData : ScriptableObject
{
    [Header("Identity")]
    public string equipmentId = "equipment_id";
    public string displayName = "Equipment";
    [TextArea(2, 4)] public string description;

    [Header("Slot")]
    public EquipmentSlot slot = EquipmentSlot.Weapon;

    [Header("Stats")]
    [Min(0)] public int attackBonus;
    [Min(0)] public int defenseBonus;

    [Header("Shop")]
    [Min(0)] public int price = 100;
}
