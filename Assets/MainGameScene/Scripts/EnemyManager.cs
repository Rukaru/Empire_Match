using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        public string name;
        public int maxHP;
        public int attack;
        public Sprite sp;
    }

    public EnemyData[] enemyList;

    int enemyIndex = 0;
    [SerializeField] EnemyData currentEnemy;
    [SerializeField] int currentHP;
    [SerializeField] static EnemyManager Instace;


    void Awake()
    {
        if (Instace == null)
        Instace = this;
        else
        Destroy(gameObject);
    }

    void Start()
    {
        LoadEnemy(enemyIndex);
    }


    /*-------------------------------------------------
    //               敵の読み取り
    ---------------------------------------------------*/
    public void LoadEnemy(int index)
    {
        if (index >= enemyList.Length)
        {
            Debug.Log("クリア！");
            return;
        }

        currentEnemy = enemyList[index];
        currentHP = currentEnemy.maxHP;

        //TODO: UIにHPを更新させる
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        //TODO: UI

        if (currentHP <= 0)
        {
            EnemyDefeated();
        }
    }

    void EnemyDefeated()
    {
        enemyIndex++;

        LoadEnemy(enemyIndex);
    }

}


