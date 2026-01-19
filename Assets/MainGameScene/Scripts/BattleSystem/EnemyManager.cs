using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] BattleManager battleManager;

    [Header("Enemy Master")]
    [SerializeField] EnemyDefintion[] normalEnemies;
    [SerializeField] EnemyDefintion bossEnemy;
   int wave = 1;
   int normalIndex = 0;

    [Header("Wave")]
    [SerializeField] int maxWave = 5;
    

    void Start()
    {
        SpawnWave();
    }

    public void NextWave()
    {

        wave++;
        Debug.Log($"[EnemyManager] NextWave called -> wave={wave} / maxWave={maxWave}");

        if (wave > maxWave)
        {
            Debug.Log("[EnemyManager] WIN (after boss)");
            battleManager.EndBattle(true);
            return;
        }

        SpawnWave();
    }

    void SpawnWave()
    {
        Debug.Log($"[EnemyManager] SpawnWave wave={wave} / maxWave={maxWave}");

        if (wave == maxWave)
        {
            Debug.Log($"[EnemyManager] Span BOSS wave ={wave}");
            battleManager.SetEnemy(bossEnemy, wave);
            return;
        }

        if(normalEnemies == null || normalEnemies.Length == 0)
        {
            Debug.LogError("normalEnemies が空！");
            return;
        }

        var def = normalEnemies[normalIndex];
        Debug.Log($"[EnemyManager] Spawn NORMAL wave={wave} index={normalIndex} name={def.enemyName}");
        
        battleManager.SetEnemy(def, wave);

        normalIndex = (normalIndex + 1) % normalEnemies.Length;
    }
}
