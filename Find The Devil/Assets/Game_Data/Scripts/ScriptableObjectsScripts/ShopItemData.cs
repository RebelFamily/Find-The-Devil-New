using UnityEngine;
using System.Collections.Generic;

// Define the two types of shop items
public enum ShopItemType
{
    Weapon,
    Scanner
}


[CreateAssetMenu(fileName = "NewShopItem", menuName = "Game Data/Shop Item")]
public class ShopItemData : ScriptableObject, IShopItem
{
    [Header("Item Info")]
    [SerializeField] private string itemID;
    [SerializeField] private ShopItemType itemType;

    [Header("Shop & Progression")]
    [SerializeField] private int cost;
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private int progressDivisions = 1;
    
    [SerializeField] private bool isA_VIPTool;

    
    [Header("Visuals & Prefabs")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite fillerSprite;
    [SerializeField] private GameObject demoPrefab;
    [SerializeField] private GameObject prefabToUnlock;
    
    
    
    public string GetItemID()
    {
        return itemID;
    }

    public int GetCost(CurrencyType requestCurrency)
    {
        if (requestCurrency == currencyType)
        {
            return cost;
        }
        Debug.LogWarning($"Item {itemID} cannot be bought with {requestCurrency}. It requires {currencyType}.");
        return -1;
    }

    public CurrencyType GetCurrencyType()
    {
        return currencyType;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public GameObject GetPrefabToUnlock()
    {
        return prefabToUnlock;
    }
    
    public ShopItemType GetItemType()
    {
        return itemType;
    }

    public Sprite GetBaseSprite()
    {
        return baseSprite;
    }

    public Sprite GetFillerSprite()
    {
        return fillerSprite;
    }

    public GameObject GetDemoPrefab()
    {
        return demoPrefab;
    }
    
    public int GetProgressDivisions()
    {
        return progressDivisions;
    }

    public bool GetIsAVIPTool()
    {
        return isA_VIPTool;
    }
}