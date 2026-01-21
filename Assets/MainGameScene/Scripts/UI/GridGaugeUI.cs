using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class GridGaugeUI : MonoBehaviour
{
    [Header("Cells(left->right) : size = 10")]
    [SerializeField] private Image[] cells;
   
   [Header("Visual")]
   [SerializeField, Range(0f, 1f)] private float emptyAlpha = 0.2f;
   [SerializeField, Range(0f, 1f)] private float filledAlpha = 1.0f;

   [Header("Pulse (optional)")]
   [SerializeField] private bool pulseOnFilled = true;
   [SerializeField] private float pulseScale = 1.08f;
   [SerializeField] private float pulseTime = 0.12f;

   private int lastFiledCount = -1;
   private Coroutine pulseCo;

/// <summary>
///  value/max を 0..cellCount に変換して表示更新
/// </summary>
/// <param name="value"></param>
/// <param name="max"></param>
   public void SetValue(int value, int max)
    {
        int cellCount = (cells != null) ? cells.Length : 0;
        if (cellCount <= 0) return;

        if(max <= 0) max = 1;

        // 0..1 に正規化して、マス数に変換
        float t = Mathf.Clamp01((float)value / max);
        int  filled = Mathf.RoundToInt(cellCount * t);

        // 念のため clamp
        filled = Mathf.Clamp(filled, 0, cellCount);

        ApplyFilledCount(filled);
    }

    public void ApplyFilledCount(int filledCount)
    {
        if (cells == null || cells.Length == 0) return;

        filledCount = Mathf.Clamp(filledCount, 0, cells.Length);

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null) continue;

            Color c = cells[i].color;
            c.a = (i < filledCount) ? filledAlpha : emptyAlpha;
            cells[i].color = c;
        } 

        bool  becameFullNow = (filledCount == cells.Length) && (lastFiledCount != cells.Length);
        if (becameFullNow && pulseOnFilled)
        {
            if (pulseCo != null) StopCoroutine(pulseCo);
            pulseCo = StartCoroutine(Pulse());
        }

        lastFiledCount = filledCount;
    }

    private IEnumerator Pulse()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 upScale = baseScale * pulseScale;

        float t = 0f;
        while (t < pulseTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(baseScale, upScale, t / pulseTime);
            yield return null;
        }

        t = 0f;
        while(t < pulseTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(upScale, baseScale, t / pulseTime);
            yield return null;
        } 

        transform.localScale = baseScale;
        pulseCo = null;
    }
}
