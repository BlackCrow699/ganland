using System.Collections.Generic;
using UnityEngine;

public enum SkillTargetMode
{
    Self,
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies
}

// Legacy enum retained so old serialized effectType values (0/1/2) still deserialize.
public enum LegacySkillEffectType
{
    SingleTargetDamage,
    AllEnemiesDamage,
    SelfHeal
}

public enum SkillEffectType
{
    Damage,
    Heal,
    BuffStat,
    DebuffStat
}

public enum StatType
{
    Attack,
    Defense,
    Speed
}

public enum ValueMode
{
    Flat,
    Percent,
    Multiplier
}

[System.Serializable]
public sealed class SkillEffect
{
    [Tooltip("Who this effect applies to.")]
    public SkillTargetMode targetMode = SkillTargetMode.SingleEnemy;

    [Tooltip("What this effect does.")]
    public SkillEffectType effectType = SkillEffectType.Damage;

    [Tooltip("Stat modified by BuffStat / DebuffStat.")]
    public StatType stat = StatType.Attack;

    [Tooltip("How 'value' is interpreted. Damage uses multiplier, Heal uses flat/percent, buffs use percent.")]
    public ValueMode valueMode = ValueMode.Multiplier;

    [Min(0f)] public float value = 1f;

    [Tooltip("Turns a BuffStat / DebuffStat lasts.")]
    [Min(0)] public int duration = 3;

    public bool IsSingleTarget
    {
        get { return targetMode == SkillTargetMode.SingleEnemy || targetMode == SkillTargetMode.SingleAlly; }
    }
}

[CreateAssetMenu(menuName = "Game Data/Skill", fileName = "SkillData")]
public sealed class SkillData : ScriptableObject
{
    [Header("Identity")]
    public string skillId = "skill_id";
    public string displayName = "Skill";
    [TextArea(2, 4)] public string description;

    [Header("Cost and Unlock")]
    [Min(0)] public int manaCost;
    [Min(1)] public int unlockLevel = 1;

    [Header("Effects")]
    public List<SkillEffect> effects = new List<SkillEffect>();

    [Header("Enemy AI")]
    [Min(0)] public int aiPriority;

    // Legacy serialized fields retained to migrate pre-template assets.
    [SerializeField, HideInInspector] private LegacySkillEffectType effectType;
    [SerializeField, HideInInspector] private float effectPower;
    [SerializeField, HideInInspector] private SkillTargetMode targetMode = SkillTargetMode.SingleEnemy;
    [SerializeField, HideInInspector] private bool legacyMigrated;

    public bool RequiresEnemyTarget { get { return HasTarget(SkillTargetMode.SingleEnemy); } }
    public bool RequiresAllyTarget { get { return HasTarget(SkillTargetMode.SingleAlly); } }
    public bool TargetsAllies { get { return HasTarget(SkillTargetMode.SingleAlly) || HasTarget(SkillTargetMode.AllAllies); } }
    public bool TargetsEnemies { get { return HasTarget(SkillTargetMode.SingleEnemy) || HasTarget(SkillTargetMode.AllEnemies); } }
    public bool TargetsSelf { get { return HasTarget(SkillTargetMode.Self); } }

    // Backward-compatible helpers used by battle AI and UI.
    public bool NeedsEnemyTarget { get { return TargetsEnemies; } }
    public bool IsAreaSkill { get { return HasTarget(SkillTargetMode.AllEnemies) || HasTarget(SkillTargetMode.AllAllies); } }
    public bool IsSelfSkill { get { return TargetsSelf; } }

    void OnEnable()
    {
        if (EnsureMigrated())
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool EnsureMigrated()
    {
        if (legacyMigrated) return false;

        if ((effects == null || effects.Count == 0) && effectPower > 0f)
        {
            effects = new List<SkillEffect>();
            SkillEffect migrated = new SkillEffect();

            switch (effectType)
            {
                case LegacySkillEffectType.SingleTargetDamage:
                    migrated.effectType = SkillEffectType.Damage;
                    migrated.valueMode = ValueMode.Multiplier;
                    migrated.targetMode = targetMode == SkillTargetMode.AllEnemies ? SkillTargetMode.AllEnemies : SkillTargetMode.SingleEnemy;
                    migrated.value = effectPower;
                    break;
                case LegacySkillEffectType.AllEnemiesDamage:
                    migrated.effectType = SkillEffectType.Damage;
                    migrated.valueMode = ValueMode.Multiplier;
                    migrated.targetMode = SkillTargetMode.AllEnemies;
                    migrated.value = effectPower;
                    break;
                case LegacySkillEffectType.SelfHeal:
                    migrated.effectType = SkillEffectType.Heal;
                    migrated.valueMode = ValueMode.Flat;
                    migrated.targetMode = SkillTargetMode.Self;
                    migrated.value = effectPower;
                    break;
            }

            effects.Add(migrated);
        }

        legacyMigrated = true;
        return true;
    }

    public List<SkillEffect> GetEffects()
    {
        EnsureMigrated();
        return effects;
    }

    bool HasTarget(SkillTargetMode mode)
    {
        EnsureMigrated();
        if (effects == null) return false;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null && effects[i].targetMode == mode) return true;
        }
        return false;
    }
}
