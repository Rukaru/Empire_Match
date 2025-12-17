using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public EnemyDefintion def;
    public int maxHP;

    public int hp { get; private set; }
    public int turn { get; private set; }

    bool isAttackDebuffed;
    float attackDebuffRate;

    // ★ これを追加
    public EnemyData(EnemyDefintion def, int wave)
    {
        this.def = def;

        maxHP = Mathf.FloorToInt(def.baseHP * Mathf.Pow(1.2f, wave - 1));
        turn = def.maxTurn;
        hp = maxHP;
    }

    public void TakeDamage(int damage)
    {
        hp = Mathf.Max(0, hp - damage);
        Debug.Log($"Enemy TakeDamage {damage} => HP {hp}");
    }

    public void DecreaseTurn() => turn--;
    public void ResetTurn() => turn = def.maxTurn;

    public bool IsDead => hp <= 0;
    public int AttackDamage => GetAttackDamage();

    public void ApplyAttackDebuff(float rate)
    {
        isAttackDebuffed = true;
        attackDebuffRate = rate;
    }

    public int GetAttackDamage()
    {
        int damage = def.attackDamage;

        if (isAttackDebuffed)
        {
            damage = Mathf.FloorToInt(damage * attackDebuffRate);
            isAttackDebuffed = false;
        }
        return damage;
    }
}


