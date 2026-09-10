using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Player", fileName = "PlayerData")]
public sealed class PlayerData : ScriptableObject
{
    [Header("Identity")]
    public string dataId = "craven";
    public string displayName = "Craven";

    [Header("Base Stats")]
    [Min(1)] public int maxHP = 100;
    [Min(0)] public int maxMP = 50;
    [Min(0)] public int attackPower = 20;
    [Min(0)] public int defense = 10;
    [Min(0)] public int speed = 10;

    [Header("Level Growth")]
    [Min(0)] public int hpGrowth = 20;
    [Min(0)] public int mpGrowth = 5;
    [Min(0)] public int attackGrowth = 4;
    [Min(0)] public int defenseGrowth = 2;
    [Min(0)] public int speedGrowth = 1;

    [Header("Experience Curve")]
    [Min(0)] public int baseExpRequirement = 100;
    [Min(1f)] public float expGrowthMultiplier = 1.2f;

    [Header("Skills")]
    public List<SkillData> skills = new List<SkillData>();
}