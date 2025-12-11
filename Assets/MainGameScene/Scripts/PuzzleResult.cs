using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleResult : MonoBehaviour
{
    // 連鎖回数
    public int chain;

    // 色別ピース消去数
    public int red;
    public int green;
    public int blue;
    public int yellow;

    public PuzzleResult(int chain, int red, int green, int blue, int yellow)
    {
        this.chain = chain;
        this.red = red;
        this.green = green;
        this.blue = blue;
        this.yellow = yellow;
    }

    public override string ToString()
    {
        return $"chain={chain}, 赤={red}, 緑={green}, 青={blue}, 黄={yellow}";
    }
}
