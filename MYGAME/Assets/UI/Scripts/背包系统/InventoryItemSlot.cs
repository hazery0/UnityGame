using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI组件")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI stackCountText;
    
    [Header("右键菜单")]
    public GameObject contextMenu;
    public Button useButton;
    public Button equipButton;
    public Button discardButton;
    
    private Item currentItem;
    private InventoryUIManager inventoryManager;
    
    public void Initialize(Item item, InventoryUIManager manager)
    {
        currentItem = item;
        inventoryManager = manager;
        
        // 更新显示
        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        
        // 显示堆叠数量
        if (stackCountText != null)
        {
            stackCountText.text = item.isStackable ? item.stackCount.ToString() : "";
            stackCountText.gameObject.SetActive(item.isStackable);
        }
        
        // 隐藏右键菜单
        if (contextMenu != null) contextMenu.SetActive(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowContextMenu();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            inventoryManager.ShowItemOptions(currentItem);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停效果
        GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 恢复颜色
        GetComponent<Image>().color = Color.white;
    }
    
    void ShowContextMenu()
    {
        if (contextMenu == null) return;
        
        contextMenu.SetActive(true);
        
        // 根据物品类型显示不同按钮
        if (useButton != null)
            useButton.gameObject.SetActive(currentItem.itemType == ItemType.Consumable);
            
        if (equipButton != null)
            equipButton.gameObject.SetActive(currentItem.isEquippable);
        
        // 绑定按钮事件
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(UseItem);
        }
        
        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(EquipItem);
        }
        
        if (discardButton != null)
        {
            discardButton.onClick.RemoveAllListeners();
            discardButton.onClick.AddListener(DiscardItem);
        }
    }
    
    public void UseItem()
    {
        inventoryManager.ShowItemOptions(currentItem);
        HideContextMenu();
    }
    
    public void EquipItem()
    {
        inventoryManager.ShowItemOptions(currentItem);
        HideContextMenu();
    }
    
    public void DiscardItem()
    {
        inventoryManager.DiscardItem(currentItem);
        HideContextMenu();
    }
    
    void HideContextMenu()
    {
        if (contextMenu != null) contextMenu.SetActive(false);
    }
}