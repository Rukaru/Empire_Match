using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;

    public void Show(Sprite sprite)
    {
        sr.sprite = sprite;
        sr.color = Color.white;
        gameObject.SetActive(true); 
    }

    public void SetEnemy(EnemyDefintion def)
    {
        sr.sprite = def.sp;
        sr.color = Color.white;
    }

    void Awake()
    {
        if(sr == null)
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.gameObject.SetActive(true);
    }


    public void FlashDamage() // ダメージフラッシュ（赤くなる）
    {
        StopAllCoroutines();
        StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
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

    public void ResetColor()
    {
        if(sr != null)
        sr.color = Color.white;
    }
}
