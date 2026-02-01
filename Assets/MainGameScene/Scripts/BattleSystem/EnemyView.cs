using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;

    [Header("Effects")]
    [SerializeField] float redGlowTime = 0.25f;
    [SerializeField] Color redGlowColor = new Color(1f, 0.25f, 0.25f, 1f);

    [SerializeField] float shakeTime = 0.15f;
    [SerializeField] float shakeStrength = 0.15f;
    [SerializeField] int shakeVibration = 20;//振動

    [Header("Defeat Flash")]
    [SerializeField] float whiteFlashTime = 0.18f;
    [SerializeField] Color whiteFlashColor = new Color(1.6f, 1.6f, 1.6f, 1f);

    Coroutine flashCo;
    Coroutine shakeCo;
    Vector3 baseLocalPos;


    void Awake()
    {
        if(sr == null)
        sr = GetComponent<SpriteRenderer>();
        baseLocalPos = transform.localPosition;
    }

    public void Show(Sprite sprite)
    {
        sr.sprite = sprite;
        sr.color = Color.white;
        gameObject.SetActive(true); 
        baseLocalPos = transform.localPosition;// 出現時の位置を基準に
    }

    public void SetEnemy(EnemyDefintion def)
    {
        sr.sprite = def.sp;
        sr.color = Color.white;
        baseLocalPos = transform.localPosition;
    }


   
    public void FlashDamage() // ダメージフラッシュ（赤くなる）
    {
        RedGlowAndSmallShake();
    }

    public void FlashWhite(float duration = -1f)
    {
        if (sr == null) return;
        if (duration < 0f) duration = whiteFlashTime;

        StopAllCoroutines();

        flashCo = StartCoroutine(WhiteFlash(duration));
    }

    IEnumerator WhiteFlash(float duration)
    {
        Color original = sr.color;
        original.a = 1f;

        float end = Time.time + duration;

        // 点滅の速さ（小さいほどパカパカ）
        const float blink = 0.06f;

        while (Time.time < end)
        {
            // 明るく白
            sr.color = Color.white;
            yield return new WaitForSeconds(blink);

            // 一瞬消す（これが超効く）
            sr.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(blink);
        }

        sr.color = original;
    }

    public void RedGlowAndSmallShake()
    {
        //発光
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(Flash(redGlowColor, redGlowTime));

        //揺れ
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(Shake(shakeTime, shakeStrength, shakeVibration));
    }

    IEnumerator Flash(Color color, float time)
    {
        Color original = sr.color;
        original.a = 1f;

        sr.color = color;
        yield return new WaitForSeconds(time);

        sr.color = Color.white;
    }

    IEnumerator Shake(float time, float strength, int vibration)
    {
        baseLocalPos = transform.localPosition;

        float elapsed = 0f;
        while(elapsed < time)
        {
            elapsed += Time.deltaTime;

            float x = (Random.value * 2f - 1f) * strength;
            float y = (Random.value * 2f - 1f) * strength;
            transform.localPosition = baseLocalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        transform.localPosition = baseLocalPos;
    }


    public void ResetColor()
    {
        if(sr != null) sr.color = Color.white;
        transform.localPosition = baseLocalPos;
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0, 1));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1, 0));
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        Color c = sr.color;

        while (t < 0.3f)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / 0.3f);
            sr.color = c;
            yield return null;
        }
    }
}
