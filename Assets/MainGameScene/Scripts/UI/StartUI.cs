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
        BGMManager.Instance.Play(titleBGM);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySE(tapSE);

        GameResultData.Reset();
        SceneManager.LoadScene("MatchPuzzle");
    }
}