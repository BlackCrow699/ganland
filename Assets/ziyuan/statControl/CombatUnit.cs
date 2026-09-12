using System.Collections.Generic;
using UnityEngine;

public sealed class StatModifier
{
    public StatType stat;
    public float percent;
    public int remainingTurns;

    public StatModifier(StatType stat, float percent, int remainingTurns)
    {
        this.stat = stat;
        this.percent = percent;
        this.remainingTurns = remainingTurns;
    }
}

public class CombatUnit : MonoBehaviour
{
    [Header("Basic Information")]
    public string unitName;
    public int maxHP = 100;
    public int currentHP;

    [Header("Battle Stats")]
    public int attackPower = 15;
    public int defense = 5;
    public int speed = 10;

    private readonly List<StatModifier> activeModifiers = new List<StatModifier>();

    public virtual int EffectiveAttack { get { return Mathf.RoundToInt(attackPower * (1f + GetStatModifier(StatType.Attack))); } }
    public virtual int EffectiveDefense { get { return Mathf.RoundToInt(defense * (1f + GetStatModifier(StatType.Defense))); } }
    public virtual int EffectiveSpeed { get { return Mathf.RoundToInt(speed * (1f + GetStatModifier(StatType.Speed))); } }

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual bool TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - EffectiveDefense, 1);
        currentHP -= finalDamage;
        Debug.Log($"{unitName} takes {finalDamage} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
            return true;
        }
        return false;
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public void ApplyStatModifier(StatType stat, float percent, int durationTurns)
    {
        if (durationTurns <= 0) return;

        for (int i = 0; i < activeModifiers.Count; i++)
        {
            if (activeModifiers[i].stat == stat)
            {
                activeModifiers[i].percent = percent;
                activeModifiers[i].remainingTurns = durationTurns;
                return;
            }
        }

        activeModifiers.Add(new StatModifier(stat, percent, durationTurns));
    }

    public void TickStatusEffects()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].remainingTurns--;
            if (activeModifiers[i].remainingTurns <= 0)
            {
                activeModifiers.RemoveAt(i);
            }
        }
    }

    float GetStatModifier(StatType stat)
    {
        float sum = 0f;
        for (int i = 0; i < activeModifiers.Count; i++)
        {
            if (activeModifiers[i].stat == stat)
            {
                sum += activeModifiers[i].percent;
            }
        }
        return sum;
    }

    protected virtual void Die()
    {
        Debug.Log($"{unitName} is defeated!");
    }
}
