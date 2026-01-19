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
    [SerializeField]public GridGaugeUI redGrid;
    [SerializeField]public GridGaugeUI greenGrid;
    [SerializeField]public GridGaugeUI blueGrid;
    [SerializeField]public GridGaugeUI yellowGrid;

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI waveValue;
    [SerializeField] TextMeshProUGUI enemyTurnText;
    [SerializeField] TextMeshProUGUI enemyTurnValue;
    //[SerializeField] TextMeshProUGUI enemyHpText;

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

    [SerializeField] Color damageColor = Color.red;
    [SerializeField] Color healColor = Color.green;

    [Header("SE")]
    [SerializeField] AudioSource seSource;
    [SerializeField] AudioClip seHitNormal;
    [SerializeField] AudioClip seHitBonus;
    [SerializeField] AudioClip sePlayerDamage;
    [SerializeField] AudioClip seHeal;

    [SerializeField, Range(0f, 1f)] float volHitNormal = 0.4f;
    [SerializeField, Range(0f, 1f)] float volHitBonus  = 0.9f;
    [SerializeField, Range(0f, 1f)] float volDamage    = 0.8f;
    [SerializeField, Range(0f, 1f)] float volHeal      = 0.6f;

    float nextSeTime = 0f;
    [SerializeField] float seCooldown = 0.08f;

    void Start()
    {
        FlashImage.SetActive(false);
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
        SpawnPopup(enemyPopupRoot, $"-{value}", damageColor, true);
    }

    public void PopupPlayerDamage(int value)
    {
        SpawnPopup(playerPopupRoot, $"-{value}", damageColor, false);
    }

    public void PopupPlayerHeal(int value)
    {
        SpawnPopup(playerPopupRoot, $"+{value}", healColor, false);
    }

     void SpawnPopup(RectTransform root, string text, Color color, bool isEnemy)
    {
        if (root == null || damageTextPrefab == null) return;

        var t = Instantiate(damageTextPrefab, root);
        t.text = text;
        t.color = color;

        // どっちのスタックか選ぶ
        int stack = isEnemy ? enemyPopupStack : playerPopupStack;

        // ★重なり対策：Yずらし
        float offsetY = stack * 60f;
        t.rectTransform.anchoredPosition += new Vector2(0, offsetY);

        // スタック増やす
        if (isEnemy) enemyPopupStack++;
        else playerPopupStack++;

        // 終わったらスタック減らす
        StartCoroutine(PopupAnim(t, () =>
        {
            if (isEnemy) enemyPopupStack = Mathf.Max(0, enemyPopupStack - 1);
            else playerPopupStack = Mathf.Max(0, playerPopupStack - 1);
        }));
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

}
