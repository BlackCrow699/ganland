using UnityEngine;
using UnityEngine.Serialization;

public enum SkillTargetMode
{
    Self,
    SingleEnemy,
    AllEnemies
}

public enum SkillEffectType
{
    SingleTargetDamage,
    AllEnemiesDamage,
    SelfHeal
}

[CreateAssetMenu(menuName = "Game Data/Skill", fileName = "SkillData")]
public sealed class SkillData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable ID used by saves and UI selection.")]
    public string skillId = "skill_id";
    [FormerlySerializedAs("skillName")]
    public string displayName = "Skill";
    [TextArea(2, 4)] public string description;

    [Header("Cost and Unlock")]
    [FormerlySerializedAs("mpCost")]
    [Min(0)] public int manaCost;
    [Min(1)] public int unlockLevel = 1;

    [Header("Effect")]
    [FormerlySerializedAs("type")]
    public SkillEffectType effectType;
    [FormerlySerializedAs("power")]
    [Min(0f)] public float effectPower;

    [Header("Targeting")]
    public SkillTargetMode targetMode = SkillTargetMode.SingleEnemy;
    [FormerlySerializedAs("needsEnemyTarget")]
    [SerializeField, HideInInspector] private bool legacyNeedsEnemyTarget = true;

    [Header("Enemy AI")]
    [Min(0)] public int aiPriority;

    public bool NeedsEnemyTarget { get { return targetMode != SkillTargetMode.Self; } }
    public bool IsAreaSkill { get { return targetMode == SkillTargetMode.AllEnemies || effectType == SkillEffectType.AllEnemiesDamage; } }
    public bool IsSelfSkill { get { return targetMode == SkillTargetMode.Self || effectType == SkillEffectType.SelfHeal; } }

    public void MigrateLegacyTargeting()
    {
        if (targetMode == SkillTargetMode.SingleEnemy && !legacyNeedsEnemyTarget)
            targetMode = effectType == SkillEffectType.SelfHeal ? SkillTargetMode.Self : SkillTargetMode.AllEnemies;
    }

    public SkillDefinition ToRuntimeDefinition()
    {
        MigrateLegacyTargeting();
        return new SkillDefinition
        {
            skillId = skillId,
            skillName = displayName,
            description = description,
            mpCost = manaCost,
            unlockLevel = unlockLevel,
            type = (SkillType)effectType,
            power = effectPower,
            needsEnemyTarget = NeedsEnemyTarget,
            aiPriority = aiPriority
        };
    }
}
