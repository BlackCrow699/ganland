using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillDefinition
{
    public string skillId = "power_strike";
    public string skillName = "Power Strike";
    [TextArea] public string description = "A powerful strike against one enemy.";
    [Min(0)] public int mpCost = 10;
    [Min(1)] public int unlockLevel = 1;
    public SkillType type = SkillType.SingleTargetDamage;
    [Min(0f)] public float power = 1.5f;
    public bool needsEnemyTarget = true;
    [Min(0)] public int aiPriority = 0;
}

public class PlayerUnit : CombatUnit
{
    [Header("External Data")]
    public PlayerData playerData;

    [Header("Level & EXP")]
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("Level-up Growth")]
    public int hpGrowth = 20;
    public int attackGrowth = 4;
    public int defenseGrowth = 2;
    public int speedGrowth = 1;
    public int maxMP = 50;
    public int currentMP = 50;
    public int mpGrowth = 5;
    [Header("Skills")]
    public List<SkillDefinition> skills = new List<SkillDefinition>();
    [SerializeField] private List<string> unlockedSkillIds = new List<string>();

    protected override void Awake()
    {
        ApplyPlayerData();
        if (GameManager.instance != null)
        {
            unitName = GameManager.instance.playerName;
            level = GameManager.instance.level;
            currentExp = GameManager.instance.currentExp;
            maxHP = GameManager.instance.maxHP;
            currentHP = GameManager.instance.currentHP;
            maxMP = GameManager.instance.maxMP;
            currentMP = GameManager.instance.currentMP;
            attackPower = GameManager.instance.attackPower;
            defense = GameManager.instance.defense;
            speed = GameManager.instance.speed;
            expToNextLevel = CalculateExpRequirement(level);
        }
        else
        {
            base.Awake();
        }
        if (skills == null || skills.Count == 0) EnsureDefaultSkills();
        RefreshUnlockedSkills();
    }

    void ApplyPlayerData()
    {
        if (playerData == null) return;
        unitName = playerData.displayName;
        maxHP = playerData.maxHP;
        currentHP = maxHP;
        maxMP = playerData.maxMP;
        currentMP = maxMP;
        attackPower = playerData.attackPower;
        defense = playerData.defense;
        speed = playerData.speed;
        hpGrowth = playerData.hpGrowth;
        mpGrowth = playerData.mpGrowth;
        attackGrowth = playerData.attackGrowth;
        defenseGrowth = playerData.defenseGrowth;
        speedGrowth = playerData.speedGrowth;
        expToNextLevel = CalculateExpRequirement(level);
        skills = RuntimeDataHelpers.ToRuntimeSkills(playerData.skills);
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel) LevelUp();
        SaveDataToGameManager();
    }

    public override bool TakeDamage(int damage)
    {
        bool died = base.TakeDamage(damage);
        SaveDataToGameManager();
        return died;
    }

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;
        maxHP += hpGrowth;
        currentHP = maxHP;
        maxMP += mpGrowth;
        currentMP = maxMP;
        attackPower += attackGrowth;
        defense += defenseGrowth;
        speed += speedGrowth;
        expToNextLevel = CalculateExpRequirement(level);
        RefreshUnlockedSkills();
        Debug.Log($"LEVEL UP! Lv.{level}, HP MAX: {maxHP}, MP MAX: {maxMP}, ATTACK: {attackPower}, DEFENSE: {defense}, SPEED: {speed}");
    }

    void EnsureDefaultSkills()
    {
        skills = new List<SkillDefinition>
        {
            new SkillDefinition { skillId = "normal_skill", skillName = "普通技能", description = "对一个敌人造成伤害。", mpCost = 10, unlockLevel = 1, type = SkillType.SingleTargetDamage, power = 1.5f, needsEnemyTarget = true },
            new SkillDefinition { skillId = "ultimate_skill", skillName = "大招", description = "对所有存活敌人造成伤害。", mpCost = 25, unlockLevel = 1, type = SkillType.AllEnemiesDamage, power = 2.0f, needsEnemyTarget = false },
            new SkillDefinition { skillId = "self_heal", skillName = "回血", description = "恢复主角自己的 HP。", mpCost = 12, unlockLevel = 1, type = SkillType.SelfHeal, power = 35f, needsEnemyTarget = false }
        };
    }

    public void RefreshUnlockedSkills()
    {
        if (unlockedSkillIds == null) unlockedSkillIds = new List<string>();
        if (GameManager.instance != null && GameManager.instance.unlockedSkillIds != null)
            unlockedSkillIds = new List<string>(GameManager.instance.unlockedSkillIds);
        for (int i = 0; i < skills.Count; i++)
        {
            SkillDefinition skill = skills[i];
            if (skill != null && level >= skill.unlockLevel && !unlockedSkillIds.Contains(skill.skillId)) unlockedSkillIds.Add(skill.skillId);
        }
        SaveSkillUnlocks();
    }

    void SaveSkillUnlocks()
    {
        if (GameManager.instance != null) GameManager.instance.unlockedSkillIds = new List<string>(unlockedSkillIds);
    }

    public bool IsSkillUnlocked(int index)
    {
        return index >= 0 && index < skills.Count && skills[index] != null && unlockedSkillIds.Contains(skills[index].skillId);
    }

    public List<SkillDefinition> GetUnlockedSkills()
    {
        List<SkillDefinition> result = new List<SkillDefinition>();
        for (int i = 0; i < skills.Count; i++) if (IsSkillUnlocked(i)) result.Add(skills[i]);
        return result;
    }

    int CalculateExpRequirement(int currentLvl)
    {
        int baseRequirement = playerData != null ? playerData.baseExpRequirement : 100;
        float multiplier = playerData != null ? playerData.expGrowthMultiplier : 1.2f;
        return Mathf.RoundToInt(baseRequirement * Mathf.Pow(multiplier, currentLvl - 1));
    }

    public void SaveDataToGameManager()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.level = level;
        GameManager.instance.currentExp = currentExp;
        GameManager.instance.maxHP = maxHP;
        GameManager.instance.currentHP = currentHP;
        GameManager.instance.maxMP = maxMP;
        GameManager.instance.currentMP = currentMP;
        GameManager.instance.attackPower = attackPower;
        GameManager.instance.defense = defense;
        GameManager.instance.speed = speed;
        SaveSkillUnlocks();
    }
}