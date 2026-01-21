using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject menuRoot;

    [Header("SceneNames")]
    [SerializeField] string gameSceneName = "MatchPuzzle";
    [SerializeField] string titleSceneName = "TitleScene";

    bool isOpen = false;
    
    void Start()
    {
        SetOpen(false);    
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(!isOpen);
        }
    }

    void SetOpen(bool open)
    {
        isOpen = open;

        if(menuRoot != null)
        menuRoot.SetActive(open);

        Time.timeScale = open ? 0f : 1f;
    }

    public void OnclickReStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnclickContinue()
    {
        SetOpen(false);
    }

    public void OnClickExitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }
}
