using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarttUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AudioClip tapSE;
    [SerializeField, Range(0f, 1f)] float SEVol = 0.6f;
    [SerializeField] AudioClip titleBGM;
    [SerializeField, Range(0f, 1f)] float BGMVol = 0.6f;
    [SerializeField] TextMeshProUGUI Title;

    void Start()
    {
        AudioManager.Instance.PlayBGM(titleBGM, BGMVol);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySE(tapSE, SEVol);

        GameResultData.Reset();
        SceneManager.LoadScene("MatchPuzzle");
    }

    public void GameGuide()
    {
        SceneManager.LoadScene("GuideScene");
        //AudioManager.Instance.PlaySE(tapSE);
    }
}