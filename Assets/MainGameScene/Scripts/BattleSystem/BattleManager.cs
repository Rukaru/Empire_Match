using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    bool resolvingTurn = false;
    int maxCombo = 0;
    public int MaxCombo => maxCombo;

    
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


    [Header("Battle Numbers")]
    [SerializeField] int normalAttack = 100;
    [SerializeField] int redBonusAttack = 150;
    [SerializeField] int greenHeal = 200;

    [Header("Buff/Debuff Numbers")]
    [SerializeField, Range(0.1f, 1f)] float blueDebuffRate = 0.5f;
    [SerializeField] int yellowAttackMul = 2;
    [SerializeField] int lockedTileScore = 50;


    // ===== 青黄の状態 =====
    int blueDebuffTurns = 0;
    int yellowBuffTurns = 0;

    [Header("Finisher")]
    [SerializeField] int finisherComboThreshold = 10;
    [SerializeField] int finisherDamage = 1200;
    [SerializeField] int finisherBonusScore = 2000;

    int turnCount = 0;
    public int TurnCount => turnCount;

    void Start()
    {
        playerHP = playerMaxHP;
        UpdatePlayerHPUI();
        UpdateNormaUI();
        norma.ResetAll();

        Debug.Log("[BattleManager] Start called");
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

        //敵が変わったタイミングで矢印UI更新（表示ズレ防止）
        UI.SetBlueDebuff(blueDebuffTurns);
        UI.SetYellowBuff(yellowBuffTurns);
    }
    
        
    /*-------------------------------------------------
    //               UI処理
    ---------------------------------------------------*/
    void UpdateNormaUI()
    {
        // 格子ゲージ版（value/max をそのまま渡す）
        UI.SetGridGauge(UI.redGrid, norma.red, norma.redMax);
        UI.SetGridGauge(UI.greenGrid, norma.green, norma.greenMax);
        UI.SetGridGauge(UI.blueGrid, norma.blue, norma.blueMax);
        UI.SetGridGauge(UI.yellowGrid, norma.yellow, norma.yellowMax);
    }

    float SafeRate(int value, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)value / max);
    }
    
    void UpdatePlayerHPUI()
    {
        float rate = (float)playerHP / playerMaxHP;
        UI.SetPlayerHP(rate);
        // ui.SetPlayerHP((float)playerHP / playerMaxHP);
    }

    void UpdateEnemyHPUI()
    {
        float rate = (float)currentEnemy.hp / currentEnemy.maxHP;
        UI.SetEnemyHP(rate);
        //ui.SetEnemyHP((float)currentEnemy.HP / currentEnemy.MaxHP);
    }
     
   /*-------------------------------------------------
    //               パズル情報処理
    ---------------------------------------------------*/
    public void OnPuzzleChainResult(PuzzleResult result)
    {
        if (currentEnemy == null) return;

        //2重呼び出しガード
        if (resolvingTurn) return;
        StartCoroutine(ResolveTurn(result));
    }

    IEnumerator ResolveTurn(PuzzleResult result)
    {
        resolvingTurn = true;

        if(result.maxComboThisTurn > GameResultData.maxCombo)
        {
            GameResultData.maxCombo = result.maxComboThisTurn;
            Debug.Log($"[SAVE] GameResultData.maxCombo updated => {GameResultData.maxCombo}");    
        }

        maxCombo = Mathf.Max(maxCombo, result.maxComboThisTurn);
        Debug.Log($"[TURN] {result}");

        // -------------------------
        // 1) ターン開始時にバフ残りターン減算
        // -------------------------
        TickBuffs();

        // -------------------------
        // 2) 効果中の青/黄はゲージ加算しない → スコア化
        // -------------------------
        if (blueDebuffTurns > 0 && result.blue > 0) UI.AddScore(result.blue * lockedTileScore);
        if (yellowBuffTurns > 0 && result.yellow > 0) UI.AddScore(result.yellow * lockedTileScore);

        norma.blueLocked   = (blueDebuffTurns > 0);
        norma.yellowLocked = (yellowBuffTurns > 0);

        // 満タンSE用の「加算前」状態
        bool blueWasFull   = (norma.blue >= norma.blueMax);
        bool yellowWasFull = (norma.yellow >= norma.yellowMax);

        // -------------------------
        // 3) ゲージ加算
        // -------------------------
        norma.Add(result);
        UpdateNormaUI();

        // 満タンSE（ロック中は鳴らさない）
        if (!blueWasFull && norma.blue >= norma.blueMax && blueDebuffTurns == 0) UI.PlayBlueFull();
        if (!yellowWasFull && norma.yellow >= norma.yellowMax && yellowBuffTurns == 0) UI.PlayYellowFull();

        // -------------------------
        // ★4) 必殺を「ここで即判定」→ 必殺だけで終了（混ざらない）
        // -------------------------
        if ( result.maxComboThisTurn >= finisherComboThreshold)
        {
            yield return StartCoroutine(DoFinisher());

            resolvingTurn = false;
            yield break; // ← 他の攻撃・ノルマを完全に止める
        }

        // -------------------------
        // 5) ノルマ発動（赤は“貯めるだけ”＝bonusDamageに入れる）
        // -------------------------
        int bonusDamageThisTurn = 0;

        while (true)
        {
            NormaType activated = norma.CheckAndConsume();
            if (activated == NormaType.None) break;

            switch (activated)
            {
                case NormaType.Red:
                    bonusDamageThisTurn += redBonusAttack;
                    UI.PlayHitBonus(); // SEは鳴らしてOK（ダメージは後でまとめて適用）
                    break;

                case NormaType.Green:
                    HealPlayer(greenHeal); // ★直書き禁止
                    break;

                case NormaType.Blue:
                    blueDebuffTurns = 2;
                    UI.SetBlueDebuff(blueDebuffTurns);
                    currentEnemy.ApplyAttackDebuff(blueDebuffRate);
                    break;

                case NormaType.Yellow:
                    yellowBuffTurns = 2;
                    UI.SetYellowBuff(yellowBuffTurns);
                    break;
            }
        }

        // -------------------------
        // 6) 通常攻撃（赤を消したら）
        // -------------------------
        int mainDamageThisTurn = 0;
        if (result.red > 0)
        {
            mainDamageThisTurn = CalculateNormalDamage();
        }

        // 攻撃なしなら敵ターン進行
        int totalDamage = bonusDamageThisTurn + mainDamageThisTurn;
        if (totalDamage <= 0)
        {
            EnemyTurnProgress();
            yield return null;
            resolvingTurn = false;
            yield break;
        }

        // -------------------------
        // 7) ダメージ適用は1回だけ + 表示も1回だけ
        // -------------------------
        currentEnemy.TakeDamage(totalDamage);
        UpdateEnemyHPUI();
        enemyView.FlashDamage();

        // 表示：BONUSが絡むなら1本化
        if (bonusDamageThisTurn > 0)
        {
            UI.PopupEnemyDamageText($"-{totalDamage}", UI.GetBonusColor());
        }
        else
        {
            UI.PopupEnemyDamage(totalDamage, false);
            UI.PlayHitNormal();
        }

        if (currentEnemy.hp <= 0)
        {
            OnEnemyDefeated();
            yield return null;
            resolvingTurn = false;
            yield break;
        }

        // 攻撃したら敵ターン進行（あなたの仕様）
        if (result.red > 0)
        {
            EnemyTurnProgress();
        }

        yield return null;

        turnCount++;
        Debug.Log($"[TURN COUNT] turnCount={turnCount}");

        resolvingTurn = false;
    }
    /*-------------------------------------------------
    //               戦闘処理
    ---------------------------------------------------*/
    void ApplyNormalAttack()
    {
        int damage = CalculateNormalDamage();   // 200など
        currentEnemy.TakeDamage(damage);

        UI.PopupEnemyDamage(damage, false);
        UI.PlayHitNormal();

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
        int damage = normalAttack;

        // if(norma.red >= norma.redMax)
        // {
        //     damage += red_bonus_attack;
        // }
        if(yellowBuffTurns > 0)
        damage *= yellowAttackMul;

        Debug.Log($"通常攻撃ダメージ：{damage}");
        return damage;
    }
    
    IEnumerator DoFinisher()
    {
        if (currentEnemy == null) yield break;

        //必殺演出
        UI.PlayFinisher();
        UI.ShowFinisherCutin(1.2f);

        var cam = Camera.main;
        if (cam && cam.TryGetComponent(out CameraShake shake))
        {
            shake.Play(0.15f, 1.2f); // ←短いほど気持ちいい（おすすめ）
        }

        //カットイン見せる時間
        yield return new WaitForSeconds(0.4f);

        UI.PlayFinisherHit();
        enemyView.FlashDamage();

        currentEnemy.TakeDamage(finisherDamage);
        UpdateEnemyHPUI();

        UI.PopupEnemyDamageText($"-{finisherDamage} !!", UI.GetBonusColor());
        //スコアは「Resultに反映されるところ」に足す必要あり
        UI.AddScore(finisherBonusScore);
        
        //ヒット時の追加揺れ（さらに弱く）
        if (cam && cam.TryGetComponent(out shake))
        {
            shake.Play(0.15f, 1.2f);
        }


        if(currentEnemy.hp <= 0)
        {
            OnEnemyDefeated();
        }
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
        StartCoroutine(UI.ScreenDamageFlash());

        int damage = currentEnemy.GetAttackDamage();
        playerHP = Mathf.Max(playerHP - damage, 0);

        //playerHP = Mathf.Max(playerHP - currentEnemy.AttackDamage, 0);
        UI.PopupPlayerDamage(damage);
        UI.PlayPlayerDamage();

        UpdatePlayerHPUI();

        if(playerHP <= 0)
        EndBattle(false);
        
    }
    /*-------------------------------------------------
    *       バフ/デバフのターン減算
    -------------------------------------------------*/
    void TickBuffs()
    {
        if (blueDebuffTurns > 0)
        {
            blueDebuffTurns--;
            UI.SetBlueDebuff(blueDebuffTurns);

            if (blueDebuffTurns == 0)
            {
                currentEnemy.ApplyAttackDebuff(1.0f);
            }
        }

        if(yellowBuffTurns > 0)
        {
            yellowBuffTurns--;
            UI.SetYellowBuff(yellowBuffTurns);
        }
    }

    void HealPlayer(int value)
    {
        int before = playerHP;
        playerHP = Mathf.Min(playerHP + value, playerMaxHP);
        int healed = playerHP - before;

        if (healed > 0)
        {
            UI.PopupPlayerHeal(healed);
            UI.PlayHeal();
        }

        UpdatePlayerHPUI();
        Debug.Log($"HealPlayer value={value} before={before} after={playerHP} healed={healed}");
    }

    /*-------------------------------------------------
    *       撃破・ゲーム終了
    -------------------------------------------------*/
    void OnEnemyDefeated()
    {
        norma.ResetAll();
        UpdateNormaUI();

        StartCoroutine(EnemyDefeatedSequence());
    }

    IEnumerator EnemyDefeatedSequence()
    {
        enemyView.FadeOut();
        yield return new WaitForSeconds(0.3f);
        enemyManager.NextWave();
    }

    public void EndBattle(bool isWin)
    {
        Debug.Log($"[EndBattle] isWin={isWin} turnCount={turnCount}");
        
        Debug.Log($"[EndBattle] maxCombo(final)={GameResultData.maxCombo}");
        GameResultData.isWin = isWin;

        if(isWin) GameResultData.clearTurns = turnCount;
        else GameResultData.surviveTurns = turnCount;

        OnBattleFinished?.Invoke(isWin);
    }

    void GameOver()
    {
       EndBattle(false);
    }
}
