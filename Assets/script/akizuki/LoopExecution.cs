using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopExecution : MonoBehaviour
{
    readonly AudioSource source;
    int LoopStart, LoopLength, Hz;
    float time;
    public LoopExecution(AudioSource source)
    {
        this.source = source;
    }

    public void Update()
    {
        if(source.isPlaying)
        {
            if(time*Hz>=LoopStart+LoopLength)
            {
                source.timeSamples = (int)((time * Hz) - LoopLength);
                time = (float)source.timeSamples / Hz;
            }
            time += Time.unscaledDeltaTime * source.pitch;
        }
    }
    public void PlayBGM(AudioClip clip,int LoopStart,int LoopLength,int hz=44100)
    {
        this.LoopStart = LoopStart;
        this.LoopLength = LoopLength;
        Hz = hz;

        source.clip = clip;
        source.loop = false;
        source.Play();
        time = 0;
    }
}
