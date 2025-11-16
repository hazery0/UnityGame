using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    
    public List<Item> allItems = new List<Item>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeDatabase()
    {
        CreateMedicalItems();
        CreateWeapons();
        CreateArmor();
        CreateFoodItems();
        CreateMaterials();
        CreateSpecialItems();
        
        Debug.Log($"物品数据库初始化完成，共{allItems.Count}个物品");
    }
    
    void CreateMedicalItems()
    {
        // 脏绷带
        Item dirtyBandage = new Item();
        dirtyBandage.itemName = "脏绷带";
        dirtyBandage.itemDescription = "简单的止血工具，但不卫生";
        dirtyBandage.itemType = ItemType.Consumable;
        dirtyBandage.weight = 0.1f;
        dirtyBandage.healthRestore = 10;
        dirtyBandage.curesEffect = StatusEffectType.Bleeding;
        dirtyBandage.negativeEffect = StatusEffectType.Infection;
        dirtyBandage.negativeEffectChance = 0.25f;
        dirtyBandage.isStackable = true;
        dirtyBandage.maxStackSize = 10;
        allItems.Add(dirtyBandage);
        
        // 消毒绷带
        Item cleanBandage = new Item();
        cleanBandage.itemName = "消毒绷带";
        cleanBandage.itemDescription = "经过消毒处理的医疗绷带";
        cleanBandage.itemType = ItemType.Consumable;
        cleanBandage.weight = 0.2f;
        cleanBandage.healthRestore = 20;
        cleanBandage.curesEffect = StatusEffectType.Bleeding;
        cleanBandage.isStackable = true;
        cleanBandage.maxStackSize = 10;
        allItems.Add(cleanBandage);
        
        // 医疗包
        Item medkit = new Item();
        medkit.itemName = "医疗包";
        medkit.itemDescription = "完整的战地医疗工具包";
        medkit.itemType = ItemType.Consumable;
        medkit.weight = 0.8f;
        medkit.healthRestore = 40;
        medkit.curesEffect = StatusEffectType.DeepWound;
        allItems.Add(medkit);
        
        // 军用医疗针
        Item militarySyringe = new Item();
        militarySyringe.itemName = "军用医疗针";
        militarySyringe.itemDescription = "强效急救药物，快速恢复生命";
        militarySyringe.itemType = ItemType.Consumable;
        militarySyringe.weight = 0.3f;
        militarySyringe.healthRestore = 60;
        militarySyringe.rarity = ItemRarity.Rare;
        allItems.Add(militarySyringe);
        
        // 抗生素
        Item antibiotics = new Item();
        antibiotics.itemName = "抗生素";
        antibiotics.itemDescription = "治疗细菌感染";
        antibiotics.itemType = ItemType.Consumable;
        antibiotics.weight = 0.1f;
        antibiotics.curesEffect = StatusEffectType.Infection;
        antibiotics.isStackable = true;
        antibiotics.maxStackSize = 5;
        allItems.Add(antibiotics);
        
        // 夹板
        Item splint = new Item();
        splint.itemName = "夹板";
        splint.itemDescription = "用于固定骨折部位";
        splint.itemType = ItemType.Consumable;
        splint.weight = 0.6f;
        splint.curesEffect = StatusEffectType.Fracture;
        allItems.Add(splint);
        
        // 抗辐射药
        Item antiRadPills = new Item();
        antiRadPills.itemName = "抗辐射药";
        antiRadPills.itemDescription = "降低体内辐射水平";
        antiRadPills.itemType = ItemType.Consumable;
        antiRadPills.weight = 0.2f;
        antiRadPills.radiationReduction = 30;
        antiRadPills.isStackable = true;
        antiRadPills.maxStackSize = 5;
        allItems.Add(antiRadPills);
        
        // 肾上腺素
        Item adrenaline = new Item();
        adrenaline.itemName = "肾上腺素";
        adrenaline.itemDescription = "激发身体潜能，但会消耗精力";
        adrenaline.itemType = ItemType.Consumable;
        adrenaline.weight = 0.1f;
        adrenaline.strengthBonus = 3;
        adrenaline.accuracyBonus = 3;
        adrenaline.agilityBonus = 3;
        adrenaline.energyRestore = -20; // 负值表示消耗
        adrenaline.isStackable = true;
        adrenaline.maxStackSize = 3;
        allItems.Add(adrenaline);
        
        // 类固醇合剂
        Item steroids = new Item();
        steroids.itemName = "类固醇合剂";
        steroids.itemDescription = "临时增强肌肉力量";
        steroids.itemType = ItemType.Consumable;
        steroids.weight = 0.2f;
        steroids.strengthBonus = 5;
        allItems.Add(steroids);
        
        // 聚焦药剂
        Item focusPotion = new Item();
        focusPotion.itemName = "聚焦药剂";
        focusPotion.itemDescription = "增强注意力和精准度";
        focusPotion.itemType = ItemType.Consumable;
        focusPotion.weight = 0.2f;
        focusPotion.accuracyBonus = 5;
        allItems.Add(focusPotion);
        
        // 实验性辐射药
        Item experimentalRadCure = new Item();
        experimentalRadCure.itemName = "实验性辐射药";
        experimentalRadCure.itemDescription = "强效但危险的辐射治疗药物";
        experimentalRadCure.itemType = ItemType.Consumable;
        experimentalRadCure.weight = 0.3f;
        experimentalRadCure.radiationReduction = 1000; // 清除所有辐射
        experimentalRadCure.negativeEffect = StatusEffectType.RadiationDiscomfort;
        experimentalRadCure.negativeEffectChance = 0.5f;
        experimentalRadCure.rarity = ItemRarity.Epic;
        allItems.Add(experimentalRadCure);
    }
    
    void CreateWeapons()
    {
        // 近战武器
        CreateWeapon("撬棍", "基本的近战工具", EquipmentType.Weapon, 2.5f, 1, 0, 0, 0);
        CreateWeapon("破冰斧", "重型劈砍工具", EquipmentType.Weapon, 3.0f, 2, 0, 0, 0);
        CreateWeapon("掠夺者链锯", "危险的动力武器", EquipmentType.Weapon, 6.5f, 4, 0, 0, 0);
        CreateWeapon("动力拳套", "高科技格斗装备", EquipmentType.Weapon, 7.5f, 6, 0, 0, 0);
        
        // 远程武器
        CreateWeapon("粗制弓", "简单的远程武器", EquipmentType.Weapon, 1.5f, 0, 1, 0, 0);
        CreateWeapon("复合弓", "改进的弓弩", EquipmentType.Weapon, 2.5f, 0, 2, 0, 0);
        CreateWeapon("手枪", "基础 firearms", EquipmentType.Weapon, 2.0f, 0, 3, 0, 0);
        CreateWeapon("猎枪", "威力强大的霰弹枪", EquipmentType.Weapon, 4.0f, 0, 5, 0, 0);
        CreateWeapon("狙击步枪", "精准的远程武器", EquipmentType.Weapon, 5.5f, 0, 7, 0, 0);
    }
    
    void CreateWeapon(string name, string desc, EquipmentType type, float weight, int str, int acc, int agi, int def)
    {
        Item weapon = new Item();
        weapon.itemName = name;
        weapon.itemDescription = desc;
        weapon.itemType = ItemType.Weapon;
        weapon.isEquippable = true;
        weapon.equipmentType = type;
        weapon.weight = weight;
        weapon.strengthBonus = str;
        weapon.accuracyBonus = acc;
        weapon.agilityBonus = agi;
        weapon.defenseBonus = def;
        allItems.Add(weapon);
    }
    
    void CreateArmor()
    {
        CreateArmorItem("单薄衣物", "基本的防护", EquipmentType.Armor, 0.8f, 1, 2, 0, 0);
        CreateArmorItem("加固皮衣", "皮革防护服", EquipmentType.Armor, 1.5f, 2, 1, 0, 0);
        CreateArmorItem("防弹背心", "军用防护装备", EquipmentType.Armor, 3.0f, 4, -2, 0, 0);
        CreateArmorItem("战术护甲", "高级战斗装备", EquipmentType.Armor, 5.5f, 6, -4, 0, 0);
        CreateArmorItem("军用外骨骼", "高科技动力装甲", EquipmentType.Armor, 8.0f, 8, -2, 0, 0);
    }
    
    void CreateArmorItem(string name, string desc, EquipmentType type, float weight, int def, int agiPenalty, int str, int acc)
    {
        Item armor = new Item();
        armor.itemName = name;
        armor.itemDescription = desc;
        armor.itemType = ItemType.Armor;
        armor.isEquippable = true;
        armor.equipmentType = type;
        armor.weight = weight;
        armor.defenseBonus = def;
        armor.agilityBonus = agiPenalty; // 负值表示惩罚
        armor.strengthBonus = str;
        armor.accuracyBonus = acc;
        allItems.Add(armor);
    }
    
    void CreateFoodItems()
    {
        // 生食
        CreateFood("变异果", "受辐射影响的水果", 0.2f, 5, 0, 0.3f, StatusEffectType.RadiationSickness);
        CreateFood("罐头食品", "密封保存的食物", 0.4f, 15, 0, 0, StatusEffectType.None);
        CreateFood("军用压缩饼干", "高能量食品", 0.2f, 30, 0, 0, StatusEffectType.None);
        CreateFood("生肉", "未烹饪的肉类", 0.4f, 10, 0, 0.4f, StatusEffectType.Diarrhea);
        CreateFood("变异肉", "受辐射的肉类", 0.3f, 10, 5, 0.4f, StatusEffectType.Diarrhea);
        CreateFood("水", "饮用水", 0.1f, 10, 0, 0.4f, StatusEffectType.Infection);
        CreateFood("蔬菜", "新鲜蔬菜", 0.2f, 5, 0, 0, StatusEffectType.None);
        CreateFood("面粉", "烹饪原料", 0.3f, 5, 0, 0, StatusEffectType.None);
        
        // 熟食
        CreateCookedFood("烤肉串", "烤制的肉串", 0.5f, 25, 0, 0, StatusEffectType.None);
        CreateCookedFood("炖肉汤", "营养丰富的肉汤", 0.8f, 30, 10, 0, StatusEffectType.None);
        CreateCookedFood("营养杂烩", "高营养炖菜", 1.0f, 50, 0, 0, StatusEffectType.None);
        CreateCookedFood("果酱馅饼", "甜味点心", 0.4f, 30, 0, 0, StatusEffectType.None);
        CreateCookedFood("辐射杂烩", "危险的强化食物", 1.0f, 50, 20, 0, StatusEffectType.None);
        
        // 饮品
        CreateDrink("提神茶", "恢复精力的茶饮", 0.3f, 0, 20, 0, StatusEffectType.None);
        CreateDrink("自制烈酒", "手工酿造的酒类", 0.4f, 0, 0, 0, StatusEffectType.None);
        CreateDrink("抗辐射茶", "降低辐射的茶饮", 0.3f, 0, 0, 15, StatusEffectType.None);
    }
    
    void CreateFood(string name, string desc, float weight, float hunger, float rad, float negChance, StatusEffectType negEffect)
    {
        Item food = new Item();
        food.itemName = name;
        food.itemDescription = desc;
        food.itemType = ItemType.Consumable;
        food.weight = weight;
        food.hungerRestore = hunger;
        food.radiationReduction = -rad; // 负值表示增加辐射
        food.negativeEffect = negEffect;
        food.negativeEffectChance = negChance;
        food.isStackable = true;
        food.maxStackSize = 10;
        allItems.Add(food);
    }
    
    void CreateCookedFood(string name, string desc, float weight, float hunger, int health, float rad, StatusEffectType negEffect)
    {
        Item food = new Item();
        food.itemName = name;
        food.itemDescription = desc;
        food.itemType = ItemType.Consumable;
        food.weight = weight;
        food.hungerRestore = hunger;
        food.healthRestore = health;
        food.radiationReduction = rad;
        food.negativeEffect = negEffect;
        food.requiresCooking = true;
        allItems.Add(food);
    }
    
    void CreateDrink(string name, string desc, float weight, float hunger, float energy, float rad, StatusEffectType negEffect)
    {
        Item drink = new Item();
        drink.itemName = name;
        drink.itemDescription = desc;
        drink.itemType = ItemType.Consumable;
        drink.weight = weight;
        drink.hungerRestore = hunger;
        drink.energyRestore = energy;
        drink.radiationReduction = rad;
        drink.negativeEffect = negEffect;
        drink.isStackable = true;
        drink.maxStackSize = 5;
        allItems.Add(drink);
    }
    
    void CreateMaterials()
    {
        CreateMaterial("木板", "建筑和制作材料", 1.0f);
        CreateMaterial("胶带", "多功能粘合材料", 0.1f);
        CreateMaterial("塑料", "合成材料", 0.3f);
        CreateMaterial("废金属", "金属废料", 0.8f);
        CreateMaterial("布料", "纺织材料", 0.2f);
        CreateMaterial("绳子", "捆绑材料", 0.3f);
        CreateMaterial("电子零件", "精密元件", 0.2f);
        CreateMaterial("化学试剂", "化学物质", 0.3f);
        CreateMaterial("动力电池", "能源装置", 1.5f);
        CreateMaterial("精密镜片", "光学组件", 0.4f);
        CreateMaterial("稀有草药", "特殊植物", 0.2f);
        CreateMaterial("过滤水", "纯净水", 0.1f);
    }
    
    void CreateMaterial(string name, string desc, float weight)
    {
        Item material = new Item();
        material.itemName = name;
        material.itemDescription = desc;
        material.itemType = ItemType.Material;
        material.weight = weight;
        material.isStackable = true;
        material.maxStackSize = 50;
        allItems.Add(material);
    }
    
    void CreateSpecialItems()
    {
        // 这里可以添加任务物品等特殊物品
    }
    
    // 根据名称查找物品
    public Item FindItemByName(string name)
    {
        return allItems.Find(item => item.itemName == name);
    }
    
    // 根据类型筛选物品
    public List<Item> GetItemsByType(ItemType type)
    {
        return allItems.FindAll(item => item.itemType == type);
    }
    
    // 获取所有可装备物品
    public List<Item> GetEquippableItems()
    {
        return allItems.FindAll(item => item.isEquippable);
    }
}