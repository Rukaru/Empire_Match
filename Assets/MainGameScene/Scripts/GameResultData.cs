using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameResultData
{
    public static bool isWin;
    
    public static int survivedWave;
    public static int killedEnemyCount;
    
    public static int killedBossCount;
    
    public static int score;
    public static int maxCombo;

    public static int turns;//今回プレイの確定したターン数
    public static int clearTurns;//勝利時：クリアターン
    public static int surviveTurns;//敗北時：生存ターン

    public static void Reset()
    {
        isWin = false;
        survivedWave = 0;
        killedEnemyCount = 0;
        killedBossCount = 0;
        score = 0;
        maxCombo = 0;

        turns = 0;
        clearTurns = 0;
        surviveTurns = 0;
    }
}
