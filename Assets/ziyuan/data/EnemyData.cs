using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Enemy", fileName = "EnemyData")]
public sealed class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string dataId = "enemy_id";
    public string displayName = "Enemy";

    [Header("Base Stats")]
    [Min(1)] public int maxHP = 50;
    [Min(0)] public int maxMP;
    [Min(0)] public int attackPower = 10;
    [Min(0)] public int defense = 5;
    [Min(0)] public int speed = 10;

    [Header("Rewards")]
    [Min(0)] public int expReward = 10;
    [Min(0)] public int goldReward = 10;

    [Header("Skills")]
    public List<SkillData> skills = new List<SkillData>();
}