using System.Collections.Generic;
using UnityEngine;

public static class BattleActionResolver
{
    /// <summary>
    /// Resolves and applies every effect entry on a skill.
    /// </summary>
    /// <param name="attacker">The unit using the skill.</param>
    /// <param name="skill">The skill template being used.</param>
    /// <param name="selectedTarget">The single selected enemy/ally, or null for area/self skills.</param>
    /// <param name="opponentUnits">Units on the opposing side (enemies of the attacker).</param>
    /// <param name="friendlyUnits">Units on the attacker's side (allies of the attacker).</param>
    public static void ApplySkill(CombatUnit attacker, SkillData skill, CombatUnit selectedTarget,
        IList<CombatUnit> opponentUnits, IList<CombatUnit> friendlyUnits)
    {
        if (attacker == null || skill == null) return;

        List<SkillEffect> effects = skill.GetEffects();
        if (effects == null) return;

        for (int i = 0; i < effects.Count; i++)
        {
            SkillEffect effect = effects[i];
            if (effect == null) continue;
            ResolveAndApply(attacker, effect, selectedTarget, opponentUnits, friendlyUnits);
        }
    }

    static void ResolveAndApply(CombatUnit attacker, SkillEffect effect, CombatUnit selectedTarget,
        IList<CombatUnit> opponentUnits, IList<CombatUnit> friendlyUnits)
    {
        List<CombatUnit> targets = ResolveTargets(attacker, effect.targetMode, selectedTarget, opponentUnits, friendlyUnits);
        if (targets == null) return;

        for (int i = 0; i < targets.Count; i++)
        {
            CombatUnit target = targets[i];
            if (target == null || target.currentHP <= 0) continue;
            ApplyEffect(attacker, effect, target);
        }
    }

    static List<CombatUnit> ResolveTargets(CombatUnit attacker, SkillTargetMode mode, CombatUnit selectedTarget,
        IList<CombatUnit> opponentUnits, IList<CombatUnit> friendlyUnits)
    {
        List<CombatUnit> result = new List<CombatUnit>();

        switch (mode)
        {
            case SkillTargetMode.Self:
                if (attacker != null && attacker.currentHP > 0) result.Add(attacker);
                break;
            case SkillTargetMode.SingleEnemy:
            case SkillTargetMode.SingleAlly:
                if (selectedTarget != null && selectedTarget.currentHP > 0) result.Add(selectedTarget);
                break;
            case SkillTargetMode.AllEnemies:
                AddAlive(result, opponentUnits);
                break;
            case SkillTargetMode.AllAllies:
                AddAlive(result, friendlyUnits);
                break;
        }

        return result;
    }

    static void AddAlive(List<CombatUnit> result, IList<CombatUnit> units)
    {
        if (units == null) return;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null && units[i].currentHP > 0) result.Add(units[i]);
        }
    }

    static void ApplyEffect(CombatUnit attacker, SkillEffect effect, CombatUnit target)
    {
        switch (effect.effectType)
        {
            case SkillEffectType.Damage:
                int damage = Mathf.Max(1, Mathf.RoundToInt(attacker.EffectiveAttack * effect.value));
                target.TakeDamage(damage);
                break;

            case SkillEffectType.Heal:
                int heal = effect.valueMode == ValueMode.Percent
                    ? Mathf.RoundToInt(target.maxHP * effect.value)
                    : Mathf.RoundToInt(effect.value);
                target.Heal(heal);
                break;

            case SkillEffectType.BuffStat:
                target.ApplyStatModifier(effect.stat, Mathf.Abs(effect.value), effect.duration);
                break;

            case SkillEffectType.DebuffStat:
                target.ApplyStatModifier(effect.stat, -Mathf.Abs(effect.value), effect.duration);
                break;
        }
    }
}
