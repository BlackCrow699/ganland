using UnityEngine;

public static class BattleEnemyController
{
    public static SkillDefinition SelectSkill(EnemyUnit enemy, CombatUnit target)
    {
        if (enemy == null || enemy.skills == null || target == null) return null;
        SkillDefinition best = null;
        for (int i = 0; i < enemy.skills.Count; i++)
        {
            SkillDefinition candidate = enemy.skills[i];
            if (candidate == null || candidate.type == SkillType.SelfHeal || candidate.mpCost > enemy.currentMP) continue;
            if (candidate.needsEnemyTarget && target.currentHP <= 0) continue;
            if (best == null || candidate.aiPriority > best.aiPriority) best = candidate;
        }
        return best;
    }
}
