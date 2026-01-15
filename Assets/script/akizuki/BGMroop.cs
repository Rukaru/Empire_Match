using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMroop : MonoBehaviour
{
    AudioSource source, source2;

    public IEnumerator PlayBGM(AudioClip BGM_Game_01, AudioClip BGM_Game_02)
    {
        yield return
        StartCoroutine(Play(BGM_Game_01, BGM_Game_02));
    }
    IEnumerator Play(AudioClip BGM_Game_01, AudioClip BGM_Game_02)
    {
        // 事前にBGMをロードしておく
        // introからloopに遷移する際の無音時間を減らすため
        BGM_Game_01.LoadAudioData();
        BGM_Game_02.LoadAudioData();

        // PlayでBGM_Game_01の再生
        source.clip = BGM_Game_01;
        source.loop = false;
        source.Play();

        // BGM_Game_01の終了まで待機
        // timeScaleを無視できる「WaitForSecondsRealtime()」を使用する
        float wait = BGM_Game_01.length / source.pitch;
        yield return new WaitForSecondsRealtime(wait);

        // PlayでBGM_Game_02の再生
        source2.clip = BGM_Game_02;
        source2.loop = true;
        source2.Play();
    }
}
