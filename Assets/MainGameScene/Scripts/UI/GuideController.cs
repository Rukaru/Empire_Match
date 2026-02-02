using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GuideController : MonoBehaviour
{
   [SerializeField] Image guideImage;
   [SerializeField] Sprite[] pages;

   int index = 0;

   void Start()
    {
        Refresh();
    }

    public void Next()
    {
        index++;
        if (index < 0) index = 0;
        Refresh();
    }

    public void Prev()
    {
        if(index <= 0) return;
        index--;
        Refresh();
    }

    void Refresh()
    {
        guideImage.sprite = pages[index];
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    
}
