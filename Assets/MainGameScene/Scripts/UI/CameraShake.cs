using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;
    Coroutine routine;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Play(float duration, float strength)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShakeCo(duration, strength));
    }

    IEnumerator ShakeCo(float duration, float strength)
    {
        float t = 0f;
        originalPos = transform.localPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            Vector2 r = Random.insideUnitCircle * strength;
            transform.localPosition = originalPos + new Vector3(r.x, r.y, 0);
            yield return null;
        }

        transform.localPosition = originalPos; // ★必ず戻す
        routine = null;
    }
}