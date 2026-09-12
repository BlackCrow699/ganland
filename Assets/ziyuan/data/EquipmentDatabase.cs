using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Equipment Database", fileName = "EquipmentDatabase")]
public sealed class EquipmentDatabase : ScriptableObject
{
    public List<EquipmentData> items = new List<EquipmentData>();

    public EquipmentData Find(string id)
    {
        if (items == null || string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].equipmentId == id)
            {
                return items[i];
            }
        }
        return null;
    }
}
