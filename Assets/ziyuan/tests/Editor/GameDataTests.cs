using NUnit.Framework;
using UnityEngine;

public class GameDataTests
{
    [Test]
    public void SkillDataConvertsToRuntimeDefinition()
    {
        SkillData data = ScriptableObject.CreateInstance<SkillData>();
        data.skillId = "test";
        data.mpCost = 7;
        data.power = 2.5f;
        SkillDefinition runtime = data.ToRuntimeDefinition();
        Assert.AreEqual("test", runtime.skillId);
        Assert.AreEqual(7, runtime.mpCost);
        Assert.AreEqual(2.5f, runtime.power);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void EnemySkillSelectionUsesPriorityAndMana()
    {
        GameObject go = new GameObject("enemy");
        EnemyUnit enemy = go.AddComponent<EnemyUnit>();
        enemy.currentMP = 10;
        enemy.skills.Add(new SkillDefinition { skillId = "low", mpCost = 1, aiPriority = 1, needsEnemyTarget = true });
        enemy.skills.Add(new SkillDefinition { skillId = "high", mpCost = 10, aiPriority = 5, needsEnemyTarget = true });
        CombatUnit target = new GameObject("target").AddComponent<CombatUnit>();
        SkillDefinition selected = BattleEnemyController.SelectSkill(enemy, target);
        Assert.AreEqual("high", selected.skillId);
        Object.DestroyImmediate(target.gameObject);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void RewardServiceUsesFallbackWhenNoEnemyReward()
    {
        Assert.AreEqual(20, BattleRewardService.CalculateExperience(new EnemyUnit[0], 20));
    }
}
