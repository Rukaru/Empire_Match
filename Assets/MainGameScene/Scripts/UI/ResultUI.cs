using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    //[SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI enemyText;
    [SerializeField] TextMeshProUGUI maxComboText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI bossText;
    [SerializeField] AudioClip tapSE;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"[RESULT] score={GameResultData.score}, maxCombo={GameResultData.maxCombo}");

        resultText.text = GameResultData.isWin ? "YOU WIN" : "GAME OVER";
        scoreText.text = $"Score : {GameResultData.score}";
        enemyText.text = $"Enemies : {GameResultData.killedEnemyCount}";
        maxComboText.text = $"MaxCombo : {GameResultData.maxCombo}";

        //bossText.text = $"Bosses : {GameResultData.killedBossCount}";
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
        AudioManager.Instance.PlaySE(tapSE);
    }
}
