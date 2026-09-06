using UnityEngine;

public class EnemyUnit : CombatUnit
{
    [Header("growth")]
    public int speedGrowth = 0;

    [Header("award")]
    public int expReward = 20;
    public int goldReward = 10;

    protected override void Die()
    {
        base.Die();
    }
}
