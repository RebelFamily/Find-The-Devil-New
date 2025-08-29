using UnityEngine;

public interface IShopItem
{
    string GetItemID();
    int GetCost(CurrencyType requestCurrency);
    CurrencyType GetCurrencyType();
    Sprite GetIcon();
    GameObject GetPrefabToUnlock();
    ShopItemType GetItemType();
    
    Sprite GetBaseSprite();
    Sprite GetFillerSprite();
    
    GameObject GetDemoPrefab(); 
    int GetProgressDivisions();
    bool GetIsAVIPTool();
}