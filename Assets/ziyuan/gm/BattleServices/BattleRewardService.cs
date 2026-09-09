using UnityEngine;

public static class BattleRewardService
{
    public static int CalculateExperience(EnemyUnit[] enemies, int fallback)
    {
        int total = 0;
        if (enemies != null)
            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i] != null) total += Mathf.Max(0, enemies[i].expReward);
        return total > 0 ? total : Mathf.Max(0, fallback);
    }

    public static int RollDropCount(int enemyCount, float chance)
    {
        int count = 0;
        for (int i = 0; i < Mathf.Max(0, enemyCount); i++)
            if (Random.value <= Mathf.Clamp01(chance)) count++;
        return count;
    }
}
