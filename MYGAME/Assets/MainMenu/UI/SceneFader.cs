using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    public CanvasGroup fadeCanvas;  // 拖入 FadePanel 的 CanvasGroup
    public float fadeDuration = 1f; // 渐变时间（秒）

    private void Awake()
    {
        // 实现单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 确保FadePanel在Awake中激活
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // 确保FadePanel激活
        EnsureFadePanelActive();

        // 进入主菜单时淡入（从黑到透明）
        StartCoroutine(FadeIn());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 确保FadePanel激活
        EnsureFadePanelActive();

        // 每次场景加载完成后淡入
        StartCoroutine(FadeIn());
    }

    // 确保FadePanel处于激活状态
    private void EnsureFadePanelActive()
    {
        if (fadeCanvas != null && !fadeCanvas.gameObject.activeInHierarchy)
        {
            fadeCanvas.gameObject.SetActive(true);
        }
    }

    public void FadeToScene(string sceneName)
    {
        // 确保FadePanel激活后再开始协程
        EnsureFadePanelActive();

        // 检查对象是否激活
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SceneFader对象未激活，直接加载场景");
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("SceneFader组件未激活，直接加载场景");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        // 再次确保FadePanel激活
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
        }

        if (fadeCanvas == null)
        {
            Debug.LogError("fadeCanvas为空！");
            yield break;
        }

        fadeCanvas.alpha = 1;
        fadeCanvas.blocksRaycasts = true; // 先阻止射线

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = 1 - (timer / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false; // 淡入完成后取消阻止射线
    }

    IEnumerator FadeOut(string sceneName)
    {
        // 确保FadePanel激活
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
        }

        if (fadeCanvas == null)
        {
            Debug.LogError("fadeCanvas为空！直接加载场景");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeCanvas.blocksRaycasts = true; // 阻止射线穿透

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = timer / fadeDuration;
            yield return null;
        }

        fadeCanvas.alpha = 1;
        SceneManager.LoadScene(sceneName);
    }

    // 安全的方法来检查是否可以启动协程
    private bool CanStartCoroutine()
    {
        return gameObject.activeInHierarchy && isActiveAndEnabled && fadeCanvas != null && fadeCanvas.gameObject.activeInHierarchy;
    }
}