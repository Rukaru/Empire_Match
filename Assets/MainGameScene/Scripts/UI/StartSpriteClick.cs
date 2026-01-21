using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSpriteClick : MonoBehaviour
{
    [SerializeField] string nextSceneName = "MatchPuzzle";
    [SerializeField] AudioClip tapSE;
    [SerializeField] AudioClip titleBGM;

    void Start()
    {
        if (AudioManager.Instance != null && titleBGM != null)
        AudioManager.Instance.PlayBGM(titleBGM);
    }

    void OnMouseDown()
    {
        // ここが「Spriteクリック」
        Debug.Log("Start Sprite Clicked!");

        if (AudioManager.Instance != null && tapSE != null)
        AudioManager.Instance.PlaySE(tapSE);

        GameResultData.Reset();
        SceneManager.LoadScene(nextSceneName);
    }
}
