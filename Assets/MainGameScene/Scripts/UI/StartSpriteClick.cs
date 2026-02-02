using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSpriteClick : MonoBehaviour
{
    [SerializeField] string nextSceneName = "MatchPuzzle";
    [SerializeField] AudioClip tapSE;
    [SerializeField] AudioClip titleBGM;
    [SerializeField, Range(0f, 1f)] float BGMVol = 0.6f;
    [SerializeField, Range(0f, 1f)] float SEVol = 0.6f;

    void Start()
    {
        if (AudioManager.Instance != null && titleBGM != null)
        AudioManager.Instance.PlayBGM(titleBGM, BGMVol);
    }

    void OnMouseDown()
    {
        StartCoroutine(GoNext());
    }

    IEnumerator GoNext()
    {
        if (AudioManager.Instance != null && tapSE != null)
        AudioManager.Instance.PlaySE(tapSE, SEVol);

        if (AudioManager.Instance != null)
        AudioManager.Instance.StopBGM();

        yield return new WaitForSeconds(0.1f);

        GameResultData.Reset();
        SceneManager.LoadScene(nextSceneName);
    }

     public void GameGuide()
    {
        SceneManager.LoadScene("GuideScene");
        //AudioManager.Instance.PlaySE(tapSE);
    }
}
