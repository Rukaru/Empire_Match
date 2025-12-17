using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI enemyText;
    [SerializeField] TextMeshProUGUI bossText;
    [SerializeField] AudioClip tapSE;

    // Start is called before the first frame update
    void Start()
    {
        resultText.text = GameResultData.isWin ? "YOU WIN" : "GAME OVER";
        scoreText.text = $"Score : {GameResultData.score}";
        enemyText.text = $"Enemies : {GameResultData.killedEnemyCount}";
        bossText.text = $"Bosses : {GameResultData.killedBossCount}";
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
        AudioManager.Instance.PlaySE(tapSE);
    }
}
