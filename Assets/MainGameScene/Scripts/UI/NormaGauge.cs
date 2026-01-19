using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NormaGauge
{
    [Header("Max")]
    public int redMax = 10;
    public int greenMax = 10;
    public int blueMax = 10;
    public int yellowMax = 10;

    [Header("Current")]
    public int red;
    public int green;
    public int blue;
    public int yellow;
   
   // ゲージ加算
    public void Add(PuzzleResult result)
    {
        red += result.red;
        green += result.green;
        yellow += result.yellow;
        blue += result.blue;

        red = Mathf.Min(red, redMax);
        green = Mathf.Min(green, greenMax);
        blue = Mathf.Min(blue, blueMax);
        yellow = Mathf.Min(yellow, yellowMax);

        Debug.Log($"[Norma Add] R:{red} G:{green} B:{blue} Y:{yellow}");
    }

    // 発動可能ノルマを順番で返す
    public NormaType CheckAndConsume()
    {
        if (red >= redMax)
        {
            red -= redMax;
            return NormaType.Red;
        }

        if (green >= greenMax)
        {
            green -= greenMax;
            return NormaType.Green;
        }

        if (yellow >= yellowMax)
        {
            yellow -= yellowMax;
            return NormaType.Yellow;
        }

        if (blue >= blueMax)
        {
            blue -= blueMax;
            return NormaType.Blue;
        }

        return NormaType.None;
    }

    public void ResetAll()
    {
        red = green = yellow = blue = 0;
    }
    

}

