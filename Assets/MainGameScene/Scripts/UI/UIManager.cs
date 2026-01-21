using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using Unity.Burst.CompilerServices;


public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textGameScore;
    int gameScore = 0;

    [Header("HP Bars")]
    [SerializeField] Image playerHpBar;
    [SerializeField] Image enemyHpBar;

    [Header("Norma Gauges")]
    [SerializeField]public GridGaugeUI redGrid;
    [SerializeField]public GridGaugeUI greenGrid;
    [SerializeField]public GridGaugeUI blueGrid;
    [SerializeField]public GridGaugeUI yellowGrid;

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI waveValue;
    [SerializeField] TextMeshProUGUI enemyTurnText;
    [SerializeField] TextMeshProUGUI enemyTurnValue;

    [SerializeField] GameObject FlashImage;
    [SerializeField] Image screenFlash;

    [Header("Damage Popup")]
    [SerializeField] RectTransform playerPopupRoot;
    [SerializeField] RectTransform enemyPopupRoot;
    [SerializeField] TextMeshProUGUI damageTextPrefab;

    [SerializeField] float popupLife = 0.6f;
    [SerializeField] float popupMoveY = 60f;

    int enemyPopupStack = 0;
    int playerPopupStack = 0;

    [Header("SE")]
    [SerializeField] AudioSource seSource;
    [SerializeField] AudioClip seHitNormal;
    [SerializeField] AudioClip seHitBonus;
    [SerializeField] AudioClip sePlayerDamage;
    [SerializeField] AudioClip seHeal;

    [SerializeField] AudioClip seFinisherHit;

    [SerializeField, Range(0f,1f)] float volFinisherHit = 1.0f;

    public void PlayFinisherHit() => PlaySE(seFinisherHit, volFinisherHit);
    [SerializeField, Range(0f, 1f)] float volHitNormal = 0.4f;
    [SerializeField, Range(0f, 1f)] float volHitBonus  = 0.9f;
    [SerializeField, Range(0f, 1f)] float volDamage    = 0.8f;
    [SerializeField, Range(0f, 1f)] float volHeal      = 0.6f;

    [Header("SE Extra")]
    [SerializeField] AudioClip seBlueFull;
    [SerializeField] AudioClip seYellowFull;
    [SerializeField, Range(0f,1f)] float volGaugeFull = 0.7f;
    
    public void PlayBlueFull()   => PlaySE(seBlueFull, volGaugeFull);
    public void PlayYellowFull() => PlaySE(seYellowFull, volGaugeFull);

    [Header("Popup Colors")]
    [SerializeField] Color damageColor = Color.red;
    [SerializeField] Color healColor = Color.green;
    [SerializeField] Color bonusColor = new Color(1f, 0.55f, 0.1f, 1f);

    [Header("Popup Layout")]
    [SerializeField] float popupStackY = 40f; //縦の重なり
    [SerializeField] float bonusOffsetX = 90f;
    [SerializeField] float bonusFontMul = 1.25f;

    [SerializeField] RectTransform canvasRect;


    float nextSeTime = 0f;
    [SerializeField] float seCooldown = 0.08f;

    [Header("Buff/Debuff Icons")]
    [SerializeField] Image blueDebuffIcon;                 // 下向き矢印
    [SerializeField] TextMeshProUGUI blueDebuffTurnText;   // 残りターン

    [SerializeField] Image yellowBuffIcon;                 // 上向き矢印
    [SerializeField] TextMeshProUGUI yellowBuffTurnText;   // 残りターン

    [Header("Finisher UI")]
    [SerializeField] AudioClip seFinisher;
    [SerializeField] GameObject finisherCutin; //TextやImageを置いたPanel
    [SerializeField, Range(0f, 1f)] float volFinisher = 1.0f;

    [Header("Camera Shake")]
    [SerializeField] CameraShake cameraShake;  

    void Start()
    {
        FlashImage.SetActive(false);

        SetBlueDebuff(0);
        SetYellowBuff(0);
    }

    void Awake()
    {
        if (seSource == null) seSource = GetComponent<AudioSource>();
    }

    void PlaySE(AudioClip clip, float volume)
    {
        if (clip == null || seSource == null) return;
        if(Time.time < nextSeTime) return;

        seSource.PlayOneShot(clip, volume);
        nextSeTime = Time.time + seCooldown;
    }

    public void PlayHitNormal()   => PlaySE(seHitNormal, volHitNormal);
    public void PlayHitBonus()    => PlaySE(seHitBonus,  volHitBonus);
    public void PlayPlayerDamage()=> PlaySE(sePlayerDamage, volDamage);
    public void PlayHeal()        => PlaySE(seHeal, volHeal);
    /*------------------------------
     * 必殺技
    ------------------------------*/
    public void PlayFinisher() => PlaySE(seFinisher, volFinisher);

    public void ShowFinisherCutin(float sec)
    {
        if(finisherCutin == null) return;
        StartCoroutine(FinisherCutinCo(sec));
    }

    IEnumerator FinisherCutinCo(float sec)
    {
        finisherCutin.SetActive(true);
        yield return new WaitForSeconds(sec);
        finisherCutin.SetActive(false);
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
        waveText.text = $"WAVE ";
        waveValue.text = $"{wave}";
    }

    public void SetEnemyTurn(int turn)
    {
        enemyTurnText.text = $"Until Enemy Attacks: ";
        enemyTurnValue.text = $"{turn}";
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

    public void SetGridGauge(GridGaugeUI grid, int value, int max)
    {
        if (grid == null) return;
        grid.SetValue(value, max);
    }

    /*------------------------------
     * Damage Popup
    ------------------------------*/
    public void PopupEnemyDamage(int value, bool isBonus)
    {
        string text = isBonus ? $"-{value} !" : $"-{value}";
        Color color = isBonus ? bonusColor : damageColor;
        
        SpawnPopup(enemyPopupRoot, text, color, true, isBonus);
    }

    public void PopupPlayerDamage(int value)
    {
        SpawnPopup(playerPopupRoot, $"-{value}", damageColor, false, false);
    }

    public void PopupPlayerHeal(int value)
    {
        SpawnPopup(playerPopupRoot, $"+{value}", healColor, false, false);
    }

     void SpawnPopup(RectTransform root, string text, Color color, bool isEnemy, bool isBonus)
    {
        if (root == null || damageTextPrefab == null) return;

        var t = Instantiate(damageTextPrefab, root);
        t.text = text;
        t.color = color;

        if(isBonus)
        t.fontSize = t.fontSize * bonusFontMul;

        // どっちのスタックか選ぶ
        int stack = isEnemy ? enemyPopupStack : playerPopupStack;

        // ★重なり対策：Yずらし
        float offsetY = stack * popupStackY;
        float offsetX = isBonus ? bonusOffsetX : 0f;

        t.rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);

        // スタック増やす
        if (isEnemy) enemyPopupStack++;
        else playerPopupStack++;

        // 終わったらスタック減らす
        StartCoroutine(PopupAnim(t, () =>
        {
            if (isEnemy) enemyPopupStack = Mathf.Max(0, enemyPopupStack - 1);
            else playerPopupStack = Mathf.Max(0, playerPopupStack - 1);
        }));

        ClampToCanvas(t.rectTransform);
    }

     IEnumerator PopupAnim(TextMeshProUGUI t, System.Action onDone)
    {
        var rt = t.rectTransform;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + new Vector2(0, popupMoveY);

        float time = 0f;
        Color c = t.color;

        while (time < popupLife)
        {
            time += Time.deltaTime;
            float k = time / popupLife;

            rt.anchoredPosition = Vector2.Lerp(start, end, k);

            c.a = Mathf.Lerp(1f, 0f, k);
            t.color = c;

            yield return null;
        }

        Destroy(t.gameObject);
        onDone?.Invoke();
    }

    void ClampToCanvas(RectTransform rt)
    {
        if (canvasRect == null) return;

        // rtの現在位置
        Vector2 pos = rt.anchoredPosition;

        // だいたいの幅/高さ（テキストのRect）
        Vector2 size = rt.sizeDelta;

        // Canvasの範囲
        Vector2 min = canvasRect.rect.min + size * 0.5f;
        Vector2 max = canvasRect.rect.max - size * 0.5f;

        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);

        rt.anchoredPosition = pos;
    }

    public void AddScore(int add)
    {
        gameScore += add;
        if (textGameScore) textGameScore.text = gameScore.ToString();
    }

    public void SetBlueDebuff(int turns)
    {
        bool active = turns > 0;

         if (blueDebuffIcon) blueDebuffIcon.gameObject.SetActive(active);
        if (blueDebuffTurnText)
        {
            blueDebuffTurnText.gameObject.SetActive(active);
            if (active) blueDebuffTurnText.text = turns.ToString();
        }
    }

    public void SetYellowBuff(int turns)
    {
        bool active = turns > 0;

        if (yellowBuffIcon) yellowBuffIcon.gameObject.SetActive(active);
        if (yellowBuffTurnText)
        {
            yellowBuffTurnText.gameObject.SetActive(active);
            if (active) yellowBuffTurnText.text = turns.ToString();
        }
    }

    public Color GetBonusColor() => bonusColor;

    public void PopupEnemyDamageText(string text, Color color)
    {
        SpawnPopup(enemyPopupRoot, text, color, true, true);
    }


}
