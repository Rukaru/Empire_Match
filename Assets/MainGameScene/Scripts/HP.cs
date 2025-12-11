using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    public int maxHP = 100;//HPを最大値に設定
    private int currentHP;//スライダーの最大値を設定
    public Slider hpSlider;//現在のHPを反映

    public void Start()
    {
        currentHP = maxHP;
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
    }

    public void TakeDamage(int damage)
    {
        //HPを減らす処理
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        //スライダーに現在のHPを反映
        hpSlider.value = currentHP;

        //HPが0になった時の処理
        if (currentHP == 0)
        {
            Debug.Log("クリア！");
            //ここにゲームオーバーの処理を追加
        }
        
    }

    private void OnDestroy()
    {
        TakeDamage(20); // HPを減らす
    }
}


