using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Definition")]
public class EnemyDefintion : ScriptableObject
{
    [Header("basic")]
    public string enemyName;
    public Sprite sp;
    public bool isBoss;

    [Header("Stats")]
    public int baseHP;
    public int attackDamage;
    public int maxTurn;
}
