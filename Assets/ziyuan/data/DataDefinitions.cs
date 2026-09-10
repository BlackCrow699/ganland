using System;
using UnityEngine;

/// <summary>Compatibility enum retained for old serialized skill definitions.</summary>
public enum SkillType
{
    SingleTargetDamage,
    AllEnemiesDamage,
    SelfHeal
}

/// <summary>Legacy serialized skill shape used only during migration.</summary>
[Serializable]
public sealed class SkillDefinition
{
    public string skillId;
    public string skillName;
    [TextArea] public string description;
    [Min(0)] public int mpCost;
    [Min(1)] public int unlockLevel = 1;
    public SkillType type;
    [Min(0f)] public float power;
    public bool needsEnemyTarget;
    [Min(0)] public int aiPriority;
}