using UnityEngine;

public class SpriteRendererAnimationCurveExample : MonoBehaviour
{
    // 点滅させる対象
    [SerializeField] private SpriteRenderer _target;

    // 時系列のアルファ値の変化カーブ
    [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private float _cycle;
    private double _time;

    private void Start()
    {
        var length = _alphaCurve.length;
        if (length < 1)
            return;

        _cycle = _alphaCurve.keys[length - 1].time;
    }

    private void Update()
    {
        // 内部時刻を経過させる
        _time += Time.deltaTime;

        // 内部時刻をcycleで折り返す
        if (_time > _cycle)
            _time = Mathf.Repeat((float)_time, _cycle);

        // アニメーションカーブに従ったアルファ値計算
        var alpha = _alphaCurve.Evaluate((float)_time);

        // 内部時刻timeにおけるアルファ値を反映
        var color = _target.color;
        color.a = alpha;
        _target.color = color;
    }
}
