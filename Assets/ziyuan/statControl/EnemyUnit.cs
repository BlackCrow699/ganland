using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : CombatUnit
{
    [Header("External Data")]
    public EnemyData enemyData;

    [Header("Growth")]
    public int speedGrowth = 0;

    [Header("Reward")]
    public int expReward = 20;
    public int goldReward = 10;
    public int maxMP = 0;
    public int currentMP = 0;

    [Header("Skills")]
    public List<SkillDefinition> skills = new List<SkillDefinition>();

    protected override void Awake()
    {
        if (enemyData != null)
        {
            unitName = enemyData.displayName;
            maxHP = enemyData.maxHP;
            attackPower = enemyData.attackPower;
            defense = enemyData.defense;
            speed = enemyData.speed;
            expReward = enemyData.expReward;
            goldReward = enemyData.goldReward;
            maxMP = enemyData.maxMP;
            currentMP = maxMP;
            skills = RuntimeDataHelpers.ToRuntimeSkills(enemyData.skills);
        }
        base.Awake();
    }

    protected override void Die()
    {
        base.Die();
    }
}