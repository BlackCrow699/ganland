using UnityEditor;
using UnityEngine;

public static class EquipmentSetup
{
    [MenuItem("Tools/Setup Equipment")]
    public static void CreateEquipmentAssets()
    {
        const string folder = "Assets/ziyuan/data/equipment";
        EnsureFolder(folder);

        EquipmentData blade = CreateEquipment(
            folder + "/CopperTwinBlades.asset",
            "copper_twin_blades", "铜制双刀", "商店出售的铜制双刀。", EquipmentSlot.Weapon, 5, 0, 60);

        EquipmentData armor = CreateEquipment(
            folder + "/ShopArmor.asset",
            "shop_armor", "商店盔甲", "商店出售的防具。", EquipmentSlot.Armor, 0, 5, 60);

        EquipmentDatabase db = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(folder + "/EquipmentDatabase.asset");
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<EquipmentDatabase>();
            AssetDatabase.CreateAsset(db, folder + "/EquipmentDatabase.asset");
        }
        db.items.Clear();
        db.items.Add(blade);
        db.items.Add(armor);
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Equipment", "已创建铜制双刀、商店盔甲和 EquipmentDatabase。", "OK");
    }

    static EquipmentData CreateEquipment(string path, string id, string name, string description, EquipmentSlot slot, int attack, int defense, int price)
    {
        EquipmentData data = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<EquipmentData>();
            AssetDatabase.CreateAsset(data, path);
        }
        data.equipmentId = id;
        data.displayName = name;
        data.description = description;
        data.slot = slot;
        data.attackBonus = attack;
        data.defenseBonus = defense;
        data.price = price;
        EditorUtility.SetDirty(data);
        return data;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string leaf = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
