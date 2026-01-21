using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarttUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AudioClip tapSE;
    [SerializeField] AudioClip titleBGM;
    [SerializeField] TextMeshProUGUI Title;

    void Start()
    {
        AudioManager.Instance.PlayBGM(titleBGM);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySE(tapSE);

        GameResultData.Reset();
        SceneManager.LoadScene("MatchPuzzle");
    }

    public void GameGuide()
    {
        SceneManager.LoadScene("GuideScene");
        //AudioManager.Instance.PlaySE(tapSE);
    }
}