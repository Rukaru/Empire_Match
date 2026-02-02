using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI maxComboText;
    [SerializeField] TextMeshProUGUI maxComboValue;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI scoreValue;
    //[SerializeField] TextMeshProUGUI bossText;
    [SerializeField] AudioClip tapSE;
    [SerializeField] AudioClip winBGM;
    [SerializeField] AudioClip loseBGM;
    [SerializeField, Range(0f, 1f)] float BGMVol = 0.6f;

    [SerializeField] GameObject winCharacterObject;

    [SerializeField] TextMeshProUGUI turnText;
    [SerializeField] TextMeshProUGUI turnValue;
    [SerializeField, Range(0f, 1f)] float SEVol = 0.6f;
    //[SerializeField] TextMeshProUGUI rankingText;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"[RESULT] score={GameResultData.score}, maxCombo={GameResultData.maxCombo}");

        resultText.text = GameResultData.isWin ? "YOU WIN" : "GAME OVER";
        scoreText.text = $"スコア :";
        scoreValue.text = $" {GameResultData.score}";
        maxComboText.text = $"最大コンボ :";
        maxComboValue.text = $" {GameResultData.maxCombo}";

        if (turnText != null && turnValue != null)
        {
            if (GameResultData.isWin)
            {
                turnText.text = "クリアターン :";
                turnValue.text = $"{GameResultData.clearTurns}";
            }
            else
            {
                turnText.text = "生存ターン :";
                turnValue.text = $"{GameResultData.surviveTurns}";
            }

            if (winCharacterObject != null)
            {
                winCharacterObject.SetActive(GameResultData.isWin);
            }
        }
       var bgm = GameResultData.isWin ? winBGM : loseBGM;

       if(AudioManager.Instance != null && bgm != null)
        {
            AudioManager.Instance.PlayBGM(bgm, BGMVol);
        }
    }

    public void ReturnToTitle()
    {
        StartCoroutine(ReturnCo());
    }

    IEnumerator ReturnCo()
    {
        if(AudioManager.Instance != null)
        AudioManager.Instance.PlaySE(tapSE, SEVol);

        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene("TitleScene");
    }
}
