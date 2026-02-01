using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources (Auto if empty)")]
    [SerializeField] AudioSource bgm;
    [SerializeField] AudioSource seSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ★ここが重要：シーン跨いでも壊れない “自分の子” を参照する
        EnsureSources();
    }

    void EnsureSources()
    {
        // 参照が切れてる（destroy済み）も含めて復旧
        if (bgm == null || bgm.Equals(null))
            bgm = gameObject.AddComponent<AudioSource>();

        if (seSource == null || seSource.Equals(null))
            seSource = gameObject.AddComponent<AudioSource>();

        bgm.loop = true;
        bgm.playOnAwake = false;

        seSource.loop = false;
        seSource.playOnAwake = false;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        EnsureSources(); // ★シーン切替後に参照が壊れてても復旧

        if (bgm.clip == clip && bgm.isPlaying) return;

        bgm.clip = clip;
        bgm.loop = true;
        bgm.Play();
    }

    public void StopBGM()
    {
        EnsureSources();
        bgm.Stop();
        bgm.clip = null;
    }

    public void PlaySE(AudioClip clip, float volume)
    {
        if (clip == null) return;

        EnsureSources();
        seSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}