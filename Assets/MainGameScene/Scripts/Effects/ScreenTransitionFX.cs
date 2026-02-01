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
        //ホワイトアウト開始 + 揺れ
        if (shake != null) shake.Play(shakeTime, shakeStrength);
        yield return Fade(whiteOut, 0f, 1f, whiteOutTime);
        
        //白になった直後、暗転（黒が上に乗る）
        yield return Fade(blackOut, 0f, 1f, blackInTime);
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
