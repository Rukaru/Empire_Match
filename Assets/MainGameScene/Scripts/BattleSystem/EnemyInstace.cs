using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstace 
{
    public EnemyDefintion def;

    public int hp;
    public int turn;

    public EnemyInstace(EnemyDefintion def, int wave)
    {
        this.def = def;

        hp = Mathf.FloorToInt(def.baseHP * Mathf.Pow(1.2f, wave - 1));
        turn = def.maxTurn;
    }

    public bool IsDead => hp <= 0;
}
