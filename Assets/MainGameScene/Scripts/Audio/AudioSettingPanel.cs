using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject panelRoot; 
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    [Header("Mixer")] 
    [SerializeField] AudioMixer mixer;
    [SerializeField] string bgmParam = "BGM_Vol";
    [SerializeField] string seParam = "SE_Vol";
    const string KEY_BGM = "Vol_BGM";
    const string KEY_SE = "Vol_SE";

    void Start()
    {
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        float se = PlayerPrefs.GetFloat(KEY_SE, 1f);

        bgmSlider.value = bgm;
        seSlider.value = se;   

        ApplyBGM(bgm);
        ApplySE(se);

        bgmSlider.onValueChanged.AddListener(ApplyBGM);
        seSlider.onValueChanged.AddListener(ApplySE);
        if(panelRoot != null) panelRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if(panelRoot == null) return;

        bool show = !panelRoot.activeSelf;
        panelRoot.SetActive(show);

        Time.timeScale = show ? 0f : 1f;
    }

    void ApplyBGM(float linear)
    {
        SetMixerVolume(bgmParam, linear);
        PlayerPrefs.SetFloat(KEY_BGM, linear);
    }

    void ApplySE(float linear)
    {
        SetMixerVolume(seParam, linear);
        PlayerPrefs.SetFloat(KEY_SE, linear);
    }

    void SetMixerVolume(string param, float linear)
    {
        if (mixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        mixer.SetFloat(param, dB);
    }

}
