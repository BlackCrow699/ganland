using NUnit.Framework;
using UnityEngine;

public class GameDataTests
{
    [Test]
    public void SkillDataStoresCanonicalSkillValues()
    {
        SkillData data = ScriptableObject.CreateInstance<SkillData>();
        data.skillId = "test";
        data.manaCost = 7;
        data.effectPower = 2.5f;
        data.targetMode = SkillTargetMode.SingleEnemy;
        Assert.AreEqual("test", data.skillId);
        Assert.AreEqual(7, data.manaCost);
        Assert.AreEqual(2.5f, data.effectPower);
        Assert.IsTrue(data.NeedsEnemyTarget);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void EnemySkillSelectionUsesPriorityAndMana()
    {
        GameObject go = new GameObject("enemy");
        EnemyUnit enemy = go.AddComponent<EnemyUnit>();
        enemy.currentMP = 10;
        SkillData low = ScriptableObject.CreateInstance<SkillData>();
        low.skillId = "low";
        low.manaCost = 1;
        low.aiPriority = 1;
        low.targetMode = SkillTargetMode.SingleEnemy;
        SkillData high = ScriptableObject.CreateInstance<SkillData>();
        high.skillId = "high";
        high.manaCost = 10;
        high.aiPriority = 5;
        high.targetMode = SkillTargetMode.SingleEnemy;
        enemy.skills.Add(low);
        enemy.skills.Add(high);
        CombatUnit target = new GameObject("target").AddComponent<CombatUnit>();
        SkillData selected = BattleEnemyController.SelectSkill(enemy, target);
        Assert.AreEqual("high", selected.skillId);
        Object.DestroyImmediate(target.gameObject);
        Object.DestroyImmediate(low);
        Object.DestroyImmediate(high);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void RewardServiceUsesFallbackWhenNoEnemyReward()
    {
        Assert.AreEqual(20, BattleRewardService.CalculateExperience(new EnemyUnit[0], 20));
    }
}
