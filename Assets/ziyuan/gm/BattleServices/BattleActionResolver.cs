using System.Collections.Generic;
using UnityEngine;

public static class BattleActionResolver
{
    public static int CalculateSkillPower(CombatUnit attacker, SkillDefinition skill)
    {
        if (attacker == null || skill == null) return 0;
        return Mathf.Max(1, Mathf.RoundToInt(attacker.attackPower * skill.power));
    }

    public static void ApplySkill(CombatUnit attacker, SkillDefinition skill, CombatUnit target, IList<CombatUnit> allTargets)
    {
        if (attacker == null || skill == null) return;
        if (skill.type == SkillType.SelfHeal)
        {
            attacker.currentHP = Mathf.Min(attacker.maxHP, attacker.currentHP + Mathf.Max(1, Mathf.RoundToInt(skill.power)));
            return;
        }

        int damage = CalculateSkillPower(attacker, skill);
        if (skill.type == SkillType.AllEnemiesDamage && allTargets != null)
        {
            for (int i = 0; i < allTargets.Count; i++)
                if (allTargets[i] != null && allTargets[i].currentHP > 0) allTargets[i].TakeDamage(damage);
        }
        else if (target != null && target.currentHP > 0)
        {
            target.TakeDamage(damage);
        }
    }
}
