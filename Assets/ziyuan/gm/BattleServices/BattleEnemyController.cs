using UnityEngine;

public static class BattleEnemyController
{
    public static SkillData SelectSkill(EnemyUnit enemy, CombatUnit target)
    {
        if (enemy == null || enemy.skills == null || target == null) return null;
        SkillData best = null;
        for (int i = 0; i < enemy.skills.Count; i++)
        {
            SkillData candidate = enemy.skills[i];
            if (candidate == null || candidate.IsSelfSkill || candidate.manaCost > enemy.currentMP) continue;
            if (candidate.NeedsEnemyTarget && target.currentHP <= 0) continue;
            if (best == null || candidate.aiPriority > best.aiPriority) best = candidate;
        }
        return best;
    }
}
