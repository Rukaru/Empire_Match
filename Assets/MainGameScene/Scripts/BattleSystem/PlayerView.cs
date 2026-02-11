using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;

    [Header("Glow")]
    [SerializeField] float glowTime = 1.0f;
    [SerializeField] Color healColor = Color.green;
    [SerializeField] Color buffColor = Color.yellow;

    Coroutine runner;
    Queue<(Color color, float time)> queue = new Queue<(Color, float)>();

    void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    public void GlowHeal(float time = -1)
    {
        EnqueueGlow(healColor, time > 0 ? time : glowTime);
    }

    public void GlowBuff(float time = -1f)
    {
        EnqueueGlow(buffColor, time > 0 ? time : glowTime);
    }

    public void GlowHealThenBuff(float timeEach = -1f)
    {
        float t = timeEach > 0 ? timeEach : glowTime;
        EnqueueGlow(healColor, t);
        EnqueueGlow(buffColor, t);
    }

    void EnqueueGlow(Color target, float time)
    {
        queue.Enqueue((target, time));
        if (runner == null) runner = StartCoroutine(RunQueue());
    }

    IEnumerator RunQueue()
    {
        while (queue.Count > 0)
        {
            var (target, time) = queue.Dequeue();
            Color original = sr.color;

            Color c = target;
            c.a = original.a;
            sr.color = c;

            yield return new WaitForSeconds(time);

            sr.color = original; 
        }

        runner = null;
    }

    public IEnumerator FadeOut(float time = 1.0f)
    {
        if (sr == null) yield break;

        float t = 0f;
        Color c = sr.color;

        float startA = c.a;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            c.a = Mathf.Lerp(startA, 0f, k);
            sr.color = c;
            yield return null;
        }

        c.a = 0f;
        sr.color = c;
    }

}
