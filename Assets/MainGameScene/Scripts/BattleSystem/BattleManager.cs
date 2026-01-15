using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class BattleManager : MonoBehaviour
{
    [SerializeField] UIManager UI;
    int score;
    int killedEnemy;
    int killedBoss;

    public int KilledEnemy => killedEnemy;
    public int KilledBoss  => killedBoss;

    public event System.Action<bool> OnBattleFinished;

    
    [Header("Player")]
    [SerializeField] int playerMaxHP = 2000;
    int playerHP;
    
    [Header("Enemy")]
    EnemyData currentEnemy;
    [SerializeField] bool isBossWave;
    [SerializeField] int currentWave;
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] int enemyMaxHP;

    [Header("Norma")]
    [SerializeField]  NormaGauge norma;
    [SerializeField] EnemyView enemyView;


    // ダメージ定数（あとで ScriptableObject 化してもOK）
    const int normal_attack = 200;   // モンゴリアンチョップ
    const int red_bonus_attack = 300; // カミツキ
    const int finisher_damage = 99999; // ヒツジゴロシ

    void Awake()
    {
        Debug.Log($"BattleManager instance = {GetInstanceID()}");
    }

    void Start()
    {
        playerHP = playerMaxHP;
        UpdatePlayerHPUI();
        UpdateNormaUI();
        norma.ResetAll();
    }


    /*-------------------------------------------------
    *         EnemyManager から呼ばれる
    -------------------------------------------------*/
    public void SetEnemy(EnemyDefintion def, int wave)
    {
        currentEnemy = new EnemyData(def, wave);
        UI.SetWave(wave);
        UI.SetEnemyTurn(currentEnemy.turn);
        
        UI.SetEnemyHP(1f);

        enemyView.Show(def.sp);
        enemyView.FadeIn();
    }
    
        
    /*-------------------------------------------------
    //               UI処理
    ---------------------------------------------------*/
    void UpdateNormaUI()
    {
        UI.SetGauge(UI.redGauge, (float)norma.red / norma.redMax);
        UI.SetGauge(UI.greenGauge, (float)norma.green / norma.greenMax);
        UI.SetGauge(UI.blueGauge, (float)norma.blue / norma.blueMax);
        UI.SetGauge(UI.yellowGauge, (float)norma.yellow / norma.yellowMax);

        Debug.Log($"Norma red = {norma.red}/{norma.redMax}");
    }

    float SafeRate(int value, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)value / max);
    }
    
    void UpdatePlayerHPUI()
    {
        if(currentEnemy == null)
        {
            Debug.LogWarning("EnemyHP UI 更新が呼ばれたが、敵が未セット");
            return;
        }
        
        float rate = (float)playerHP / playerMaxHP;
        UI.SetPlayerHP(rate);
        // ui.SetPlayerHP((float)playerHP / playerMaxHP);
    }

    void HealPlayer(int value)
    {
        playerHP += value;
        if (playerHP > playerMaxHP) playerHP = playerMaxHP;

        UpdatePlayerHPUI();
    }

    void UpdateEnemyHPUI()
    {
        float rate = (float)currentEnemy.hp / currentEnemy.maxHP;
        Debug.Log($"Enemy HP UI rate = {rate}");
        UI.SetEnemyHP(rate);
        //ui.SetEnemyHP((float)currentEnemy.HP / currentEnemy.MaxHP);
    }
     
   /*-------------------------------------------------
    //               パズル情報処理
    ---------------------------------------------------*/
    public void OnPuzzleChainResult(PuzzleResult result)
    {
        Debug.Log($"Chain:{result.chain} " +
        $"R:{result.red} G:{result.green} B:{result.blue} Y:{result.yellow}");

        if (currentEnemy == null)
        {
            Debug.LogError("敵が未セットのまま戦闘処理が呼ばれています");
            return;
        }

       // ① ノルマゲージ加算
        norma.Add(result);

        UpdateNormaUI();

        // ② ノルマ発動（複数対応）
        while (true)
        {
          
            NormaType activated = norma.CheckAndConsume();
     
            if (activated == NormaType.None) break;

            ApplyNormaEffects(activated);
            UpdateNormaUI();
            UpdateEnemyHPUI();
            UpdatePlayerHPUI();

            if(currentEnemy.hp <= 0)
            {
                OnEnemyDefeated();
                return;
            }
        }

        ApplyNormalAttack();
    }

    void ApplyNormaEffects(NormaType type)
    {
        if (currentEnemy == null)
        {
            Debug.LogError("currentEnemy が null のままノルマ発動しています");
            return;
        }
        
        switch (type)
        {
            case NormaType.Red:
                Debug.Log("赤ノルマ発動：カミツキ");
                currentEnemy.TakeDamage(red_bonus_attack);
                break;

            case NormaType.Green:
                Debug.Log("緑ノルマ発動：回復解放");
                HealPlayer(400);
                break;

            case NormaType.Yellow:
                Debug.Log("黄ノルマ発動：防御アップ（仮）");
                norma.red += 5;
                norma.blue += 5;
                UpdateNormaUI();
                break;

            case NormaType.Blue:
                Debug.Log("青ノルマ発動：敵デバフ解放");
                currentEnemy.ApplyAttackDebuff(2.0f);
                break;
        }
    }
    /*-------------------------------------------------
    //               戦闘処理
    ---------------------------------------------------*/
    void ApplyNormalAttack()
    {
        int damage = CalculateNormalDamage();
        currentEnemy.TakeDamage(damage);

        UpdateEnemyHPUI();
        enemyView.FlashDamage();

        if (currentEnemy.hp <= 0)
        {
            OnEnemyDefeated();
            return;
        }

        EnemyTurnProgress();
    }

    int CalculateNormalDamage()
    {
        int damage = normal_attack;

        if(norma.red >= norma.redMax)
        {
            damage += red_bonus_attack;
        }

        Debug.Log($"通常攻撃ダメージ：{damage}");
        return damage;
    }
    
    void DoFinisher()
    {
        Debug.Log("必殺技発動：ヒツジゴロシ！！");
        currentEnemy.TakeDamage(finisher_damage);
        OnEnemyDefeated();
        UpdateEnemyHPUI();
    }

    /*-------------------------------------------------
    //               敵ターン
    ---------------------------------------------------*/
    void EnemyTurnProgress()
    {
        currentEnemy.DecreaseTurn();
        UI.SetEnemyTurn(currentEnemy.turn);

        if (currentEnemy.turn <= 0)
        {
            EnemyAttack();
            currentEnemy.ResetTurn();
            UI.SetEnemyTurn(currentEnemy.turn);
        }

    }
    
    void EnemyAttack()
    {
        int damage = currentEnemy.GetAttackDamage();
        //playerHP = Mathf.Max(playerHP - damage, 0);

        StartCoroutine(UI.ScreenDamageFlash());

        Debug.Log($"敵攻撃！ damage={currentEnemy.AttackDamage}");
        playerHP = Mathf.Max(playerHP - currentEnemy.AttackDamage, 0);

        UpdatePlayerHPUI();

        if(playerMaxHP <= 0)
        {
            GameOver();
        }
        
    }

    /*-------------------------------------------------
    *       撃破・ゲーム終了
    -------------------------------------------------*/
    void OnEnemyDefeated()
    {
        Debug.Log("敵撃破！");
        norma.ResetAll();// Wave終了時リセット
        UpdateNormaUI();

        StartCoroutine(EnemyDefeatedSequence());

        if (currentEnemy.def.isBoss)
        killedBoss++;
        else
        killedEnemy++;

        if (enemyManager.IsLastWave())
        {
            Debug.Log("[BattleManager] All waves cleared");
            BattleClear();
            return;
        }

        StartCoroutine(EnemyDefeatedSequence());

        //GameResultData.score += 100;
        enemyManager.NextWave();
    }


    IEnumerator EnemyDefeatedSequence()
    {
        enemyView.FadeOut();
        yield return new WaitForSeconds(0.3f);
        enemyManager.NextWave();
    }

    void BattleClear()
    {
        Debug.Log("[BattleManager] BattleClear Invoke");
        OnBattleFinished?.Invoke(true);
    }

    void GameOver()
    {
        Debug.Log("[BattleManager] GameOver Invoke");
        OnBattleFinished?.Invoke(false);
    }
}
