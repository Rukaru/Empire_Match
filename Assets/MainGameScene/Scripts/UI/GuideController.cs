using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GuideController : MonoBehaviour
{
   [SerializeField] Image guideImage;
   [SerializeField] Sprite[] pages;

   [SerializeField] Button nextButton;
   [SerializeField] Text nextButtonText;
   [SerializeField] string nextLabel = "次へ";
   [SerializeField] string lastLabel = "タイトルへ";

   [SerializeField] Color normalColor = Color.white;
   [SerializeField] Color lastColor = Color.yellow;

   int index = 0;

   void Start()
    {
        Refresh();
    }

    public void Next()
    {
        if (index >= pages.Length - 1)
        {
            BackToTitle();
            return;
        }

        index++;
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
        if (pages == null || pages.Length == 0) return;
       
        if(index < 0) index = 0;
        if (index >= pages.Length) index = pages.Length - 1;

        guideImage.sprite = pages[index];

        bool isLast = (index == pages.Length - 1);
        
        if (nextButtonText != null)
        nextButtonText.text = isLast ? nextLabel : nextLabel;

        if (nextButton != null)
        {
            var colors = nextButton.colors;
            colors.normalColor = isLast ? lastColor : normalColor;
            nextButton.colors = colors;
        }
    }
    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    
}
