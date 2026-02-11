using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ScreenTransitionFX : MonoBehaviour
{
   [Header("Overlay Images")]
   [SerializeField] Image whiteOut;
   [SerializeField] Image blackOut;

   [Header("Timings")]
   [SerializeField] float whiteOutTime = 3.0f;//3秒かけて白く
   [SerializeField] float blackInTime = 0.5f;//0.5秒で暗転

   [Header("Camera Shake")]
   [SerializeField] CameraShake shake;
   [SerializeField] float shakeTime = 3.0f;
   [SerializeField] float shakeStrength = 0.35f;

    void Awake()
    {
        ResetOverlay();
    }

     public void ResetOverlay()
    {
        SetAlpha(whiteOut, 0f);
        SetAlpha(blackOut, 0f);
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    public IEnumerator PlayBossDefeatToResult()
    {
        ResetOverlay();
        //ホワイトアウト開始 + 揺れ
        if (shake != null) shake.Play(shakeTime, shakeStrength);
        yield return Fade(whiteOut, 0f, 1f, whiteOutTime);
        
        //白になった直後、暗転（黒が上に乗る）
        yield return Fade(blackOut, 0f, 1f, blackInTime);
    }

    // ========== ★追加：白フラッシュ（死亡の最初に使う） ==========
    public IEnumerator FlashWhite(float flashTime = 0.2f)
    {
        if (whiteOut == null) yield break;

        // 一瞬だけ真っ白にして戻す（目で分かる）
        SetAlpha(whiteOut, 0f);
        yield return Fade(whiteOut, 0f, 1f, flashTime * 0.2f); // 速く点灯
        yield return Fade(whiteOut, 1f, 0f, flashTime * 0.8f); // 少しだけ余韻
    }

    // ========== ★追加：黒暗転だけ（死亡で使う） ==========
    public IEnumerator BlackOut(float time)
    {
        if (blackOut == null) yield break;
        yield return Fade(blackOut, 0f, 1f, time);
    }

    IEnumerator Fade(Image img, float from, float to, float time)
    {
        if (img == null) yield break;

        float t = 0f;
        SetAlpha(img, from);

        while (t < time)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / time);
            SetAlpha(img, a);
            yield return null;
        }

        SetAlpha(img, to);
    }
}
