using UnityEngine;

public struct CharacterStatus
{
    public string characterId;
    public string displayName;
    public int level;
    public int currentHP;
    public int maxHP;
    public int currentMP;
    public int maxMP;
    public int attack;
    public int defense;
    public int speed;
}

/// <summary>Resolves a character's display stats by its stable data id.</summary>
public static class CharacterStatusService
{
    public static bool TryGet(string characterId, out CharacterStatus status)
    {
        status = default(CharacterStatus);

        GameManager manager = GameManager.instance;
        if (manager == null) manager = Object.FindObjectOfType<GameManager>();
        if (manager == null) return false;

        if (string.IsNullOrEmpty(characterId)) characterId = manager.playerDataId;
        if (string.IsNullOrEmpty(characterId)) return false;

        if (characterId == manager.playerDataId)
        {
            EquipmentManager equipment = EquipmentManager.GetOrCreate();
            status = new CharacterStatus
            {
                characterId = manager.playerDataId,
                displayName = manager.playerName,
                level = manager.level,
                currentHP = manager.currentHP,
                maxHP = manager.maxHP,
                currentMP = manager.currentMP,
                maxMP = manager.maxMP,
                attack = equipment.TotalAttack(characterId),
                defense = equipment.TotalDefense(characterId),
                speed = manager.speed
            };
            return true;
        }

        PlayerUnit[] units = Object.FindObjectsOfType<PlayerUnit>();
        for (int i = 0; i < units.Length; i++)
        {
            PlayerUnit unit = units[i];
            if (unit == null || unit.playerData == null || unit.playerData.dataId != characterId) continue;

            status = new CharacterStatus
            {
                characterId = characterId,
                displayName = unit.unitName,
                level = unit.level,
                currentHP = unit.currentHP,
                maxHP = unit.maxHP,
                currentMP = unit.currentMP,
                maxMP = unit.maxMP,
                attack = unit.EffectiveAttack,
                defense = unit.EffectiveDefense,
                speed = unit.speed
            };
            return true;
        }

        return false;
    }
}
