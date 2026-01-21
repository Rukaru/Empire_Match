using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public bool blueLocked;//発動中ゲージ禁止
    public bool yellowLocked;

    [Header("Current")]
    public int red;
    public int green;
    public int blue;
    public int yellow;


    public void ResetAll()
    {
        red = green = blue = yellow =  0;
        blueLocked = false;
        yellowLocked = false;
    }
   
   // ゲージ加算
    public void Add(PuzzleResult result)
    {
        red += result.red;
        green += result.green;

        if (!yellowLocked) yellow += result.yellow;
        if (!blueLocked)   blue   += result.blue;

        red   = Mathf.Min(red, redMax);
        green = Mathf.Min(green, greenMax);
        yellow = Mathf.Min(yellow, yellowMax);
        blue   = Mathf.Min(blue, blueMax);

        Debug.Log($"[Norma Add] R:{red} G:{green} B:{blue} Y:{yellow}");
    }

    // 発動可能ノルマを順番で返す
    public NormaType CheckAndConsume()
    {
        if (red >= redMax)
        {
            red = 0;
            return NormaType.Red;
        }

        if (green >= greenMax)
        {
            green = 0;
            return NormaType.Green;
        }

        if (yellow >= yellowMax)
        {
            yellow = 0;
            return NormaType.Yellow;
        }

        if (blue >= blueMax)
        {
            blue = 0;
            return NormaType.Blue;
        }

        return NormaType.None;
    }
}

