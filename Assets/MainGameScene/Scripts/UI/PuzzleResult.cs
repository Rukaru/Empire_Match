using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleResult 
{
    // 連鎖回数
    public int chain;

    // 色別ピース消去数
    public int red;
    public int green;
    public int yellow;
    public int blue;

    public int maxComboThisTurn;

    public PuzzleResult(){ }

    public PuzzleResult(int chain, int red, int green, int yellow, int blue, int maxComboThisTurn)
    {
        this.chain = chain;
        this.red = red;
        this.green = green;
        this.yellow = yellow;
        this.blue = blue;
        this.maxComboThisTurn = maxComboThisTurn;
    }

    public override string ToString()
    {
        return $"chain={chain}, 赤={red}, 緑={green} 黄={yellow}, 青={blue},";
    }
}
