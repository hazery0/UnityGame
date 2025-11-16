using UnityEngine;
using UnityEngine.UI; // 需要引用UI命名空间来处理按钮
using TMPro; // 如果您使用TextMeshPro，需要引用此命名空间

public class TutorialManager : MonoBehaviour
{
    [Header("教程页面")]
    public GameObject[] tutorialPages; // 存储所有页面的数组
    private int currentPageIndex = 0; // 当前显示页面的索引

    [Header("界面控制")]
    public KeyCode toggleKey = KeyCode.G; // 开关界面的按键，默认为G
    public GameObject tutorialCanvas; // 教程界面的根物体（您的course对象）

    void Start()
    {
        // 确保脚本开始时，教程界面是关闭状态
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        // 初始化所有页面，只显示第一页
        InitializePages();
    }

    void Update()
    {
        // 检测是否按下G键
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleTutorial();
        }
    }

    /// <summary>
    /// 打开或关闭教程界面
    /// </summary>
void ToggleTutorial()
{
    if (tutorialCanvas == null) return;

    // 切换界面的激活状态
    bool isActive = !tutorialCanvas.activeInHierarchy;
    tutorialCanvas.SetActive(isActive);

    // 新增的鼠标控制代码
    if (isActive)
    {
        // 打开UI时：显示并解锁鼠标
        Cursor.visible = true; // 让鼠标可见
        Cursor.lockState = CursorLockMode.None; // 不锁定鼠标
        ResetToFirstPage();
    }
    else
    {
        // 关闭UI时：隐藏并锁定鼠标（根据您的游戏需求选择）
        // 如果您希望关闭UI后鼠标继续消失，取消注释下面两行
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
        
        // 如果您希望关闭UI后鼠标仍然可见，保持如下设置：
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

    /// <summary>
    /// 初始化所有页面，只显示第一页
    /// </summary>
    void InitializePages()
    {
        // 首先隐藏所有页面
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(false);
            }
        }

        // 然后显示第一页
        if (tutorialPages.Length > 0 && tutorialPages[0] != null)
        {
            tutorialPages[0].SetActive(true);
            currentPageIndex = 0;
        }
    }

    /// <summary>
    /// 切换到下一页（由Next按钮调用）
    /// </summary>
    public void NextPage()
    {
        // 如果已经是最后一页，则不操作（或者可以循环到第一页，按需修改）
        if (currentPageIndex >= tutorialPages.Length - 1) return;

        // 隐藏当前页
        tutorialPages[currentPageIndex].SetActive(false);
        
        // 显示下一页
        currentPageIndex++;
        tutorialPages[currentPageIndex].SetActive(true);
    }

    /// <summary>
    /// 切换到上一页（由Pre按钮调用）
    /// </summary>
    public void PrePage()
    {
        // 如果已经是第一页，则不操作
        if (currentPageIndex <= 0) return;

        // 隐藏当前页
        tutorialPages[currentPageIndex].SetActive(false);
        
        // 显示上一页
        currentPageIndex--;
        tutorialPages[currentPageIndex].SetActive(true);
    }

    /// <summary>
    /// 重置到第一页
    /// </summary>
    void ResetToFirstPage()
    {
        // 隐藏当前页
        if (currentPageIndex >= 0 && currentPageIndex < tutorialPages.Length && tutorialPages[currentPageIndex] != null)
        {
            tutorialPages[currentPageIndex].SetActive(false);
        }

        // 显示第一页
        currentPageIndex = 0;
        if (tutorialPages.Length > 0 && tutorialPages[0] != null)
        {
            tutorialPages[0].SetActive(true);
        }
    }
}