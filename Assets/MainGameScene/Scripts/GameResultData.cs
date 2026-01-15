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

    public static void Reset()
    {
        Debug.Log("GameResultData.Reset() CALLED");

        isWin = false;
        survivedWave = 0;
        killedEnemyCount = 0;
        killedBossCount = 0;
        score = 0;
        maxCombo = 0;
    }
}
