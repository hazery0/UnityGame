using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsManager : MonoBehaviour
{

    [Header("UI Elements")]
    public Dropdown resolutionDropdown;
    public Dropdown screenModeDropdown;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Text masterVolumeValue;
    public TMP_Text musicVolumeValue;
    public TMP_Text sfxVolumeValue;
    public Button applyButton;
    public Button cancelButton;
    public Button backToTitleButton;
    public Button exitGameButton;
    
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Scene Names")]
    public string titleSceneName = "TitleScene";
    
    private Resolution[] resolutions;
    private bool isInitialized = false;
    
    void Start()
    {
        // 延迟一帧确保UI组件完全初始化
        StartCoroutine(InitializeAfterFrame());
    }
    
    IEnumerator InitializeAfterFrame()
    {
        yield return null; // 等待一帧确保所有UI组件就绪
        
        InitializeSettings();
    }
    
    void InitializeSettings()
    {
        // 验证所有UI引用
        if (!ValidateUIReferences())
        {
            Debug.LogError("UI引用验证失败！");
            return;
        }
        
        // 获取可用分辨率
        resolutions = Screen.resolutions;
        
        // 初始化分辨率下拉菜单
        InitializeResolutionDropdown();
        
        // 初始化屏幕模式下拉菜单
        InitializeScreenModeDropdown();
        
        // 加载保存的设置
        LoadPlayerPrefs();
        
        // 设置事件监听
        SetupEventListeners();
        
        isInitialized = true;
        Debug.Log("设置界面初始化完成");
    }
    
    bool ValidateUIReferences()
    {
        bool allValid = true;
        
        if (resolutionDropdown == null)
        {
            Debug.LogError("resolutionDropdown 引用为空！");
            allValid = false;
        }
        
        if (screenModeDropdown == null)
        {
            Debug.LogError("screenModeDropdown 引用为空！");
            allValid = false;
        }
        
        
        return allValid;
    }
    
    void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        // 清除现有选项
        resolutionDropdown.ClearOptions();
        
        // 创建分辨率选项列表
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            // 过滤掉低刷新率的分辨率
            if (resolutions[i].refreshRate < 50) continue;
            
            string option = $"{resolutions[i].width} × {resolutions[i].height} ({resolutions[i].refreshRate}Hz)";
            options.Add(option);
            
            // 查找当前分辨率
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }
        
        // 添加选项到下拉菜单
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        
        // 确保下拉菜单显示正确的值
        resolutionDropdown.RefreshShownValue();
        
        Debug.Log($"分辨率下拉菜单初始化完成，找到 {options.Count} 个选项");
    }
    
    void InitializeScreenModeDropdown()
    {
        if (screenModeDropdown == null) return;
        
        // 设置屏幕模式选项
        screenModeDropdown.ClearOptions();
        List<string> screenOptions = new List<string>
        {
            "全屏模式",
            "窗口模式", 
            "无边框窗口"
        };
        screenModeDropdown.AddOptions(screenOptions);
        
        // 设置当前屏幕模式
        int currentMode = Screen.fullScreen ? 0 : 1;
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            currentMode = 0;
        else if (Screen.fullScreenMode == FullScreenMode.Windowed)
            currentMode = 1;
        else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
            currentMode = 2;
            
        screenModeDropdown.value = currentMode;
        screenModeDropdown.RefreshShownValue();
    }
    
    void LoadPlayerPrefs()
    {
        // 加载音量设置
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        
        // 应用音量设置
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        
        UpdateVolumeDisplays();
        ApplyAudioSettings();
    }
    
    void SetupEventListeners()
    {
        // 移除可能存在的旧监听器，避免重复绑定
        RemoveEventListeners();
        
        // 音量滑块事件
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)    
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        // 下拉菜单事件
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        
        // 按钮事件
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplySettings);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelSettings);
        if (backToTitleButton != null)
            backToTitleButton.onClick.AddListener(OnBackToTitle);
        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(OnExitGame);
    }
    
    void RemoveEventListeners()
    {
        // 安全地移除所有事件监听
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveAllListeners();
        if (screenModeDropdown != null) screenModeDropdown.onValueChanged.RemoveAllListeners();
        if (applyButton != null) applyButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        if (backToTitleButton != null) backToTitleButton.onClick.RemoveAllListeners();
        if (exitGameButton != null) exitGameButton.onClick.RemoveAllListeners();
    }
    
    // 音量控制方法
    void OnMasterVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        UpdateVolumeDisplays();
    }
    
    void OnMusicVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        UpdateVolumeDisplays();
    }
    
    void OnSFXVolumeChanged(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        UpdateVolumeDisplays();
    }
    
    void UpdateVolumeDisplays()
    {
        if (masterVolumeValue != null && masterVolumeSlider != null)
            masterVolumeValue.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
        if (musicVolumeValue != null && musicVolumeSlider != null)
            musicVolumeValue.text = $"{Mathf.RoundToInt(musicVolumeSlider.value * 100)}%";
        if (sfxVolumeValue != null && sfxVolumeSlider != null)
            sfxVolumeValue.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
    }
    
    void ApplyAudioSettings()
    {
        if (audioMixer == null) return;
        
        if (masterVolumeSlider != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolumeSlider.value) * 20);
        if (musicVolumeSlider != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolumeSlider.value) * 20);
        if (sfxVolumeSlider != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolumeSlider.value) * 20);
    }
    
    // 分辨率和大屏幕模式控制
    void OnResolutionChanged(int index)
    {
        if (!isInitialized || index < 0 || index >= resolutions.Length) return;
        
        Debug.Log($"分辨率更改为: {resolutions[index].width} × {resolutions[index].height}");
    }
    
    void OnScreenModeChanged(int index)
    {
        if (!isInitialized) return;
        
        string[] modes = { "全屏", "窗口", "无边框" };
        Debug.Log($"屏幕模式更改为: {modes[index]}");
    }
    
    // 按钮功能
    void OnApplySettings()
    {
        if (!isInitialized) return;
        
        // 应用分辨率设置
        if (resolutionDropdown != null && resolutionDropdown.value < resolutions.Length)
        {
            Resolution selectedResolution = resolutions[resolutionDropdown.value];
            FullScreenMode mode = GetSelectedScreenMode();
            
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, mode, selectedResolution.refreshRate);
            Debug.Log($"已应用设置: {selectedResolution.width}×{selectedResolution.height} {mode}");
        }
        
        // 保存设置
        SaveSettings();
    }
    
    void OnCancelSettings()
    {
        // 重新加载设置
        LoadPlayerPrefs();
        Debug.Log("设置已重置");
    }
    
    void OnBackToTitle()
    {
       
    }
    
    public void OnExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    FullScreenMode GetSelectedScreenMode()
    {
        if (screenModeDropdown == null) return FullScreenMode.FullScreenWindow;
        
        return screenModeDropdown.value switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.MaximizedWindow,
            _ => FullScreenMode.FullScreenWindow
        };
    }
    
    void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            
        PlayerPrefs.Save();
    }
    
    void OnDestroy()
    {
        // 清理事件监听
        RemoveEventListeners();
    }
}