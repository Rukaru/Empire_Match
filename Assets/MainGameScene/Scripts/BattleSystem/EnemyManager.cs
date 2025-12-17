using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] BattleManager battleManager;

    [Header("Enemy Master")]
    [SerializeField] EnemyDefintion[] normalEnemies;
    [SerializeField] EnemyDefintion bossEnemy;
    int normalIndex = 0;
    bool isBoss = false;
    int turnCount = 1;
    int currentWave;

    [Header("Wave")]
    [SerializeField] int maxWave = 20;
    

    void Start()
    {
        SpawnNormal();
    }

    public void NextWave()
    {

        if (turnCount >= maxWave)
        {
            BattleClear();
            return;
        }

        // ノーマル敵を全部倒した → ボス
        if (!isBoss && normalIndex >= normalEnemies.Length - 1)
        {
            isBoss = true;
            SpawnBoss();
        }
        // ボス撃破 → ノーマルに戻る
        else if (isBoss)
        {
            isBoss = false;
            normalIndex = 0;
            turnCount++;
            SpawnNormal();
        }
        // ノーマル敵続行
        else
        {
            normalIndex++;
            SpawnNormal();
        }
    }

    void SpawnNormal()
    {
        EnemyDefintion def = normalEnemies[normalIndex];
        battleManager.SetEnemy(def, turnCount);

        Debug.Log($"Normal Spawn : {def.enemyName}");
    }

    void SpawnBoss()
    {
        battleManager.SetEnemy(bossEnemy, turnCount);
       // Debug.Log("=== BOSS SPAWN ===");
    }

    void BattleClear()
    {
        GameResultData.isWin = true;
        GameResultData.survivedWave = currentWave - 1;

        Debug.Log("BATTLE CLEAR");
        SceneManager.LoadScene("ResultScene");
    }
}
