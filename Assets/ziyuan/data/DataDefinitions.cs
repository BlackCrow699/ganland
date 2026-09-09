using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType { SingleTargetDamage, AllEnemiesDamage, SelfHeal }

[CreateAssetMenu(menuName = "Game Data/Skill", fileName = "SkillData")]
public sealed class SkillData : ScriptableObject
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

    public SkillDefinition ToRuntimeDefinition()
    {
        return new SkillDefinition
        {
            skillId = skillId,
            skillName = skillName,
            description = description,
            mpCost = mpCost,
            unlockLevel = unlockLevel,
            type = type,
            power = power,
            needsEnemyTarget = needsEnemyTarget,
            aiPriority = aiPriority
        };
    }
}

[CreateAssetMenu(menuName = "Game Data/Player", fileName = "PlayerData")]
public sealed class PlayerData : ScriptableObject
{
    public string dataId = "craven";
    public string displayName = "Craven";
    [Min(1)] public int maxHP = 100;
    [Min(0)] public int maxMP = 50;
    [Min(0)] public int attackPower = 20;
    [Min(0)] public int defense = 10;
    [Min(0)] public int speed = 10;
    [Min(0)] public int hpGrowth = 20;
    [Min(0)] public int mpGrowth = 5;
    [Min(0)] public int attackGrowth = 4;
    [Min(0)] public int defenseGrowth = 2;
    [Min(0)] public int speedGrowth = 1;
    [Min(0)] public int baseExpRequirement = 100;
    [Min(1f)] public float expGrowthMultiplier = 1.2f;
    public List<SkillData> skills = new List<SkillData>();
}

[CreateAssetMenu(menuName = "Game Data/Enemy", fileName = "EnemyData")]
public sealed class EnemyData : ScriptableObject
{
    public string dataId = "slime";
    public string displayName = "slime";
    [Min(1)] public int maxHP = 50;
    [Min(0)] public int attackPower = 10;
    [Min(0)] public int maxMP = 0;
    [Min(0)] public int defense = 5;
    [Min(0)] public int speed = 10;
    [Min(0)] public int expReward = 10;
    [Min(0)] public int goldReward = 10;
    public List<SkillData> skills = new List<SkillData>();
}

[CreateAssetMenu(menuName = "Game Data/Battle Config", fileName = "BattleConfig")]
public sealed class BattleConfig : ScriptableObject
{
    [Min(0f)] public float enemyActionDelay = 0.6f;
    [Min(0f)] public float attackAnimationDuration = 0.5f;
    public string playerAttackAnimationState = "Craven_Attack";
    public string enemyAttackAnimationState = "Wolf_Attack";
    [Min(0)] public int potionHealAmount = 30;
    [Min(0)] public int etherRestoreAmount = 20;
    [Range(0f, 1f)] public float potionDropChance = 1f;
    [Range(0f, 1f)] public float etherDropChance = 0.25f;
    [Range(0f, 1f)] public float escapeChance = 0.5f;
    [Min(0f)] public float returnToExplorationDelay = 1f;
    [Min(0)] public int fallbackExpReward = 20;
    [Range(0f, 1f)] public float defendDamageMultiplier = 0.5f;
    public string explorationSceneName = "BattleForest";
}

