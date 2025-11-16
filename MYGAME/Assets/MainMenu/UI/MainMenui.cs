using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public SceneFader fade;
    public GameObject settingPanel;

    void Start()
    {
        // 如果fade引用为空，尝试查找SceneFader
        if (fade == null)
        {
            fade = FindObjectOfType<SceneFader>();
        }

        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        Debug.Log("开始游戏");

        string sceneName = "SampleScene";

        // 首先检查fade引用是否有效
        if (fade != null)
        {
            Debug.Log("使用fade引用切换场景");
            fade.FadeToScene(sceneName);
        }
        // 然后检查SceneFader单例
        else if (SceneFader.Instance != null)
        {
            Debug.Log("使用SceneFader单例切换场景");
            SceneFader.Instance.FadeToScene(sceneName);
        }
        // 最后直接加载场景
        else
        {
            Debug.Log("直接加载场景");
            SceneManager.LoadScene(sceneName);
        }
    }

    public void OpenSettings()
    {
        Debug.Log("打开设置");
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}