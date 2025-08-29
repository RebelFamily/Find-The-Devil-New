using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ShopItemData> allWeaponsItems; 
    [SerializeField] private List<ShopItemData> allScannerItems; 

    private Dictionary<string, IShopItem> _weaponItemLookup; 
    private Dictionary<string, IShopItem> _scannerItemLookup; 

    public void Init()
    {
        _weaponItemLookup = new Dictionary<string, IShopItem>();
        _scannerItemLookup = new Dictionary<string, IShopItem>();
        
        HashSet<string> allItemIds = new HashSet<string>();

        
        foreach (var itemData in allWeaponsItems)
        {
            if (!allItemIds.Add(itemData.GetItemID()))
            {
                Debug.LogError($"Duplicate ItemID found: {itemData.GetItemID()}! This will cause critical save data bugs. Please fix it.");
                continue;
            }
            _weaponItemLookup.Add(itemData.GetItemID(), itemData);
        }

        foreach (var itemData in allScannerItems)
        {
            if (!allItemIds.Add(itemData.GetItemID()))
            {
                Debug.LogError($"Duplicate ItemID found: {itemData.GetItemID()}! This will cause critical save data bugs. Please fix it.");
                continue;
            }
            _scannerItemLookup.Add(itemData.GetItemID(), itemData);
        }
        
        string firstWeaponID = allWeaponsItems.Count > 0 ? allWeaponsItems[0].GetItemID() : string.Empty;
        string firstScannerID = allScannerItems.Count > 0 ? allScannerItems[0].GetItemID() : string.Empty;
        
        GameManager.Instance.progressionManager.Init(firstWeaponID, firstScannerID);
    }

    public List<IShopItem> GetAvailableWeapons()
    {
        return _weaponItemLookup.Values.ToList();
    }

    public List<IShopItem> GetAvailableScanners()
    {
        return _scannerItemLookup.Values.ToList();
    }
    
    public bool ClaimItem(string itemId, CurrencyType currencyType) 
    {
        IShopItem itemToClaim = null;
        if (!_weaponItemLookup.TryGetValue(itemId, out itemToClaim))
        {
            _scannerItemLookup.TryGetValue(itemId, out itemToClaim);
        }

        if (itemToClaim == null)
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("Item not found!");
            return false;
        }
      
        int cost = itemToClaim.GetCost(currencyType);
        if (cost == -1) 
        {
            GameManager.Instance.uiManager.ShowGeneralMessage($"Cannot claim with {currencyType}! Requires {itemToClaim.GetCurrencyType()}.");
            return false;
        }

        bool claimSuccess = false;
        if (currencyType == CurrencyType.Coins)
        {
            claimSuccess = GameManager.Instance.economyManager.SpendCoins(cost);
        }
        else if (currencyType == CurrencyType.Gems)
        {
            claimSuccess = GameManager.Instance.economyManager.SpendGems(cost);
        }

        if (claimSuccess)
        {
            GameManager.Instance.progressionManager.UnlockItem(itemId); 
            GameManager.Instance.uiManager.ShowGeneralMessage($"Purchased Successful"); 
 
            if (itemToClaim is ShopItemData shopItemData)
            {
                if (shopItemData.GetItemType() == ShopItemType.Weapon)
                {
                    GameManager.Instance.progressionManager.EquipWeapon(itemId);
                }
                else if (shopItemData.GetItemType() == ShopItemType.Scanner)
                {
                    GameManager.Instance.progressionManager.EquipScanner(itemId);
                }
            }
            
            return true;
        }
        else
        {
            GameManager.Instance.uiManager.ShowGeneralMessage($"Not enough {currencyType} to claim {itemToClaim.GetItemID()}.");
            return false;
        }
    }

    public bool PurchaseItem(string itemId, CurrencyType currencyType) 
    {
        IShopItem itemToPurchase = null;
        if (!_weaponItemLookup.TryGetValue(itemId, out itemToPurchase))
        {
            _scannerItemLookup.TryGetValue(itemId, out itemToPurchase);
        }

        if (itemToPurchase == null)
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("Item not found!");
            return false;
        }

        if (GameManager.Instance.progressionManager.HasItem(itemId))
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("Item already owned!");
            return false;
        }

        int cost = itemToPurchase.GetCost(currencyType); 
        if (cost == -1) 
        {
            GameManager.Instance.uiManager.ShowGeneralMessage($"Cannot buy with {currencyType}! Requires {itemToPurchase.GetCurrencyType()}.");
            return false;
        }

        bool purchaseSuccess = false;
        if (currencyType == CurrencyType.Coins)
        {
            purchaseSuccess = GameManager.Instance.economyManager.SpendCoins(cost);
        }
        else if (currencyType == CurrencyType.Gems)
        {
            purchaseSuccess = GameManager.Instance.economyManager.SpendGems(cost);
        }

        if (purchaseSuccess)
        {
            GameManager.Instance.progressionManager.UnlockItem(itemId); 
            GameManager.Instance.uiManager.ShowGeneralMessage($"Purchased Successful"); 
            return true;
        }
        else
        {
            GameManager.Instance.uiManager.ShowGeneralMessage($"Not enough {currencyType} to buy {itemToPurchase.GetItemID()}.");
            return false;
        }
    }

    public GameObject GetEquippedWeaponPrefab()
    {
        if (_weaponItemLookup == null)
        {
            return null;
        }

        string equippedWeaponID = GameManager.Instance.progressionManager.GetEquippedWeaponId();

        if (!string.IsNullOrEmpty(equippedWeaponID) && _weaponItemLookup.TryGetValue(equippedWeaponID, out IShopItem equippedItem))
        {
            return equippedItem.GetPrefabToUnlock();
        }

        return null;
    }

    public GameObject GetTryWeaponPrefab(string weaponID)
    {
        if (_weaponItemLookup == null)
        {
            return null;
        }

        string equippedWeaponID = weaponID;

        if (!string.IsNullOrEmpty(equippedWeaponID) && _weaponItemLookup.TryGetValue(equippedWeaponID, out IShopItem equippedItem))
        {
            return equippedItem.GetPrefabToUnlock();
        }

        return null;
    }
    
    
    public GameObject GetEquippedScannerPrefab()
    {
        if (_scannerItemLookup == null)
        {
            return null;
        }
        
        string equippedScannerID = GameManager.Instance.progressionManager.GetEquippedScannerId();
        
        if (!string.IsNullOrEmpty(equippedScannerID) && _scannerItemLookup.TryGetValue(equippedScannerID, out IShopItem equippedItem))
        {
            return equippedItem.GetPrefabToUnlock();
        }

        return null;
    }
    
    public void UnlockAllVIPItems(PurchaseType type)
    {
        if (PurchaseType.VIPGuns == type)
        {
            GameManager.Instance.RemoveAds(PurchaseType.RemoveAds);
            
            foreach (var itemData in allWeaponsItems)
            {
                if (itemData.GetIsAVIPTool())
                {
                    GameManager.Instance.progressionManager.UnlockItem(itemData.GetItemID());
                    Debug.Log($"VIP Weapon Unlocked: {itemData.GetItemID()}");
                }
            }

            foreach (var itemData in allScannerItems)
            {
                if (itemData.GetIsAVIPTool())
                {
                    GameManager.Instance.progressionManager.UnlockItem(itemData.GetItemID());
                    Debug.Log($"VIP Scanner Unlocked: {itemData.GetItemID()}");
                }
            }
            
            //AdsCaller.Instance.SetVIPPurchased();
        }
        
        Debug.Log("All VIP weapons and scanners have been unlocked.");
        GameManager.Instance.uiManager.HideGetVIPPanel();
    }
}