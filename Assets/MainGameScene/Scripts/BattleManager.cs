using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] int PlayerMaxHP = 2000;
    [SerializeField] int PlayerHP;

    [SerializeField] int redAttackGauge;
    [SerializeField] int greenHealGauge;
    [SerializeField] int blueSkillGauge;
    [SerializeField] int yellowAssistGauge;
    [SerializeField] int gaugeMax = 100;
    [SerializeField] EnemyManager enemyManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        PlayerHP = PlayerMaxHP;
        ResetGauges();
    }

    void ResetGauges()
    {
        redAttackGauge = 0;
        greenHealGauge = 0;
        blueSkillGauge = 0;
        yellowAssistGauge = 0;
    }
/*-------------------------------------------------
//               戦闘処理
---------------------------------------------------*/
public void ApplyPuzzleResult(PuzzleResult result)
    {
        //TODO: ゲージ加算(コア)
        AddGauges(result);

        //TODO:攻撃・回復発動
        TryPlayerAttack(result.chain);
        TryPlayerHeal(result.chain);
        TryPlayerSkill(result.chain);

        //TODO:敵のターン
    }

void AddGauges(PuzzleResult result)
    {
        redAttackGauge += result.red * (1 + result.chain * 1);
        greenHealGauge += result.green * (1 + result.chain * 1);
        blueSkillGauge += result.blue * (1 + result.chain * 1);
        
        yellowAssistGauge += result.yellow;
        redAttackGauge += result.yellow / 2;
        blueSkillGauge += result.yellow / 2;

        redAttackGauge = Mathf.Min(redAttackGauge, gaugeMax);
        greenHealGauge = Mathf.Min(greenHealGauge, gaugeMax);
        blueSkillGauge = Mathf.Min(blueSkillGauge, gaugeMax);
    }

    void TryPlayerAttack(int chain)
    {
        if (redAttackGauge <= gaugeMax) return;
        redAttackGauge = 0;

        int damage = 200 + chain * 50;//調整可能

        enemyManager.TakeDamage(damage);
    }

    void TryPlayerHeal(int chain)
    {
        if (greenHealGauge < gaugeMax) return;
        greenHealGauge = 0;
        
        int heal = 150 + chain * 30;

        PlayerHP += heal;
        if(PlayerHP > PlayerMaxHP) PlayerHP = PlayerMaxHP;
    }

    void TryPlayerSkill(int chain)
    {
        if (blueSkillGauge < gaugeMax) return;
        blueSkillGauge = 0;

        int damage = 400 + chain * 80;

        enemyManager.TakeDamage(damage);

    }

}
