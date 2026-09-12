using System.Collections.Generic;
using UnityEngine;

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
    public List<SkillData> skills = new List<SkillData>();
    [SerializeField] private List<string> unlockedSkillIds = new List<string>();

    protected override void Awake()
    {
        ApplyPlayerData();
        if (GameManager.instance != null)
        {
            InitializeNewPlayerStateIfNeeded();
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
        if (skills == null || skills.Count == 0)
            Debug.LogWarning($"{name} has no skills in PlayerData. Configure skills in the data asset.");
        RefreshUnlockedSkills();
    }

    public override int EffectiveAttack
    {
        get
        {
            return base.EffectiveAttack + EquipmentManager.GetOrCreate().EquipmentAttackBonus(playerData != null ? playerData.dataId : "");
        }
    }

    public override int EffectiveDefense
    {
        get
        {
            return base.EffectiveDefense + EquipmentManager.GetOrCreate().EquipmentDefenseBonus(playerData != null ? playerData.dataId : "");
        }
    }

    void InitializeNewPlayerStateIfNeeded()
    {
        if (GameManager.instance == null || GameManager.instance.hasPlayerRuntimeState || playerData == null) return;
        GameManager.instance.playerName = playerData.displayName;
        GameManager.instance.level = 1;
        GameManager.instance.currentExp = 0;
        GameManager.instance.maxHP = playerData.maxHP;
        GameManager.instance.currentHP = playerData.maxHP;
        GameManager.instance.maxMP = playerData.maxMP;
        GameManager.instance.currentMP = playerData.maxMP;
        GameManager.instance.attackPower = playerData.attackPower;
        GameManager.instance.defense = playerData.defense;
        GameManager.instance.speed = playerData.speed;
        GameManager.instance.unlockedSkillIds = new List<string>();
        GameManager.instance.hasPlayerRuntimeState = true;
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
        skills = playerData.skills != null ? new List<SkillData>(playerData.skills) : new List<SkillData>();
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

    public void RefreshUnlockedSkills()
    {
        if (unlockedSkillIds == null) unlockedSkillIds = new List<string>();
        if (GameManager.instance != null && GameManager.instance.unlockedSkillIds != null)
            unlockedSkillIds = new List<string>(GameManager.instance.unlockedSkillIds);
        for (int i = 0; i < skills.Count; i++)
        {
            SkillData skill = skills[i];
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

    public List<SkillData> GetUnlockedSkills()
    {
        List<SkillData> result = new List<SkillData>();
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
