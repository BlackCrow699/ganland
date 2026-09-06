using UnityEngine;

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

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual bool TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - defense, 1);
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

    protected virtual void Die()
    {
        Debug.Log($"{unitName} is defeated!");
    }
}
