using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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
        
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgm.clip == clip) return;
        bgm.clip = clip;
        bgm.loop = true;
        bgm.Play();
    }

    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip);
    }
}
