using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Battle Config", fileName = "BattleConfig")]
public sealed class BattleConfig : ScriptableObject
{
    [Header("Battle Timing")]
    [Min(0f)] public float enemyActionDelay = 0.6f;
    [Min(0f)] public float attackAnimationDuration = 0.5f;

    [Header("Animation")]
    public string playerAttackAnimationState = "Craven_Attack";
    public string enemyAttackAnimationState = "Wolf_Attack";

    [Header("Item Recovery")]
    [Min(0)] public int potionHealAmount = 30;
    [Min(0)] public int etherRestoreAmount = 20;

    [Header("Drops and Escape")]
    [Range(0f, 1f)] public float potionDropChance = 1f;
    [Range(0f, 1f)] public float etherDropChance = 0.25f;
    [Range(0f, 1f)] public float escapeChance = 0.5f;

    [Header("Battle Exit")]
    [Min(0f)] public float returnToExplorationDelay = 1f;
    public string explorationSceneName = "BattleForest";

    [Header("Defense and Reward Fallback")]
    [Min(0)] public int fallbackExpReward = 20;
    [Range(0f, 1f)] public float defendDamageMultiplier = 0.5f;
}