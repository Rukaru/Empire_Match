using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    [Header("HP Bars")]
    [SerializeField] Image playerHpBar;
    [SerializeField] Image enemyHpBar;

    [Header("Norma Gauges")]
    public Image redGauge;
    public Image greenGauge;
    public Image yellowGauge;
    public Image blueGauge;

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI enemyTurnText;
    //[SerializeField] TextMeshProUGUI enemyHpText;

    [SerializeField] GameObject FlashImage;
    [SerializeField] Image screenFlash;

    void Start()
    {
        FlashImage.SetActive(false);
    }
    /*------------------------------
     * HP
     ------------------------------*/
    public void SetPlayerHP(float rate)
    {
        playerHpBar.fillAmount = Mathf.Clamp01(rate);
    }

    public void SetGauge(Image gauge, float rate)
    {
        gauge.fillAmount = Mathf.Clamp01(rate);
    }

    public void SetEnemyHP(float rate)
    {
        enemyHpBar.fillAmount = Mathf.Clamp01(rate);
    }

    public void SetWave(int wave)
    {
        waveText.text = $"WAVE {wave}";
    }

    public void SetEnemyTurn(int turn)
    {
        enemyTurnText.text = $"ENEMY ATTACK TURN {turn}";
    }

    public IEnumerator ScreenDamageFlash()
    {
        screenFlash.color = new Color(1, 0, 0, 0.4f);
        yield return new WaitForSeconds(0.15f);
        screenFlash.color = Color.clear;

        FlashImage.SetActive(true);
    }

    public void ResetEnemyHPColor()
    {
        enemyHpBar.color = Color.green; // 基本色
        enemyHpBar.fillAmount = 1f;
    }


}
