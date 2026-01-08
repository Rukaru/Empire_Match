using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageFadeText : MonoBehaviour
{
    // 数字を下から上にあげる(５フレーム)
    //３０フレーム待機
    //透明度を調節して徐々に消す(２０フレームかけて消す)
    //必要なもの・ダメージの数値のテキスト・上にあげる時間の設定・待機時間の設定・透明度調整の時間の設定
    public TextMeshProUGUI _DamageText;
    [SerializeField]
    private float _UpTime;
    [SerializeField]
    private float _WaitTime;
    [SerializeField]
    private float _FadeTime;
    [SerializeField]
    private float _moveYOffset;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(DamageText(11));
        }
    }

    public IEnumerator DamageText(int DamageValue)
    {
        // ダメージの値を表示
        _DamageText.text = DamageValue.ToString();

        // テキストを上に上げる
        yield return StartCoroutine(TextMove());

        // 指定時間待機
        yield return new WaitForSeconds(_WaitTime);

        // フェードアウトさせる
        yield return StartCoroutine(Fade());


    }

    /// <summary>
    /// テキストを上昇させる
    /// </summary>
    /// <returns></returns>
    private IEnumerator TextMove()
    {
        // 経過時間
        float elapsed = 0f;

        // 最初の位置
        Vector2 startPos = gameObject.transform.position;
        // 終わりの位置
        Vector2 endPos = new Vector2(startPos.x, startPos.y + _moveYOffset);

        // 経過時間が制限時間より大きくなるまで移動
        while(elapsed < _UpTime)
        {
            // 経過時間を記録
            elapsed += Time.deltaTime;

            // 位置を変更
            gameObject.transform.position = Vector2.Lerp(startPos, endPos, elapsed / _UpTime);
            yield return null;
        }

        gameObject.transform.position = endPos;


    }

    private IEnumerator Fade()
    {
        // 経過時間
        float elapsed = 0f;

        // テキストの色
        var textColor = gameObject.GetComponent<TextMeshProUGUI>().color;

        // 最初の色
        Color startAlpha = new Vector4(textColor.r, textColor.g, textColor.b, textColor.a);
        // 終わりの色
        Color endAlpha = new Vector4(startAlpha.r,startAlpha.g,startAlpha.b,0);

        // 経過時間が制限時間よりも大きくなるまで
        while(elapsed < _FadeTime)
        {
            // 経過時間を記録
            elapsed += Time.deltaTime;

            //　色をカエル
            gameObject.GetComponent<TextMeshProUGUI>().color = Color.Lerp(startAlpha,endAlpha,elapsed/_FadeTime);
            yield return null;
        }

        gameObject.GetComponent<TextMeshProUGUI>().color = endAlpha;
        yield return null;
    }
}
