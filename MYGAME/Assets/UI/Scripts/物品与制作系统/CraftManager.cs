using UnityEngine;

public class CraftManager : MonoBehaviour
{
    public GameObject craftUI;
    private bool isCraftOpen = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // 使用T键打开制作
        {
            ToggleCraftUI();
        }
    }
    
    public void ToggleCraftUI()
    {
        isCraftOpen = !isCraftOpen;
        craftUI.SetActive(isCraftOpen);
    }
}