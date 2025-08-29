using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    
    private const string UnlockedLevelKey = "UnlockedLevel";
    private const string UnlockedItemsKeyPrefix = "UnlockedItem_";
    private const string EquippedWeaponKey = "EquippedWeaponId";
    private const string EquippedScannerKey = "EquippedScannerId";
    
    // Keys for Meta and City Data
    private const string MetaProgressKeyPrefix = "Meta_Progress_";
    private const string CityMetaStatesKey = "City_Meta_States";
    private const string CurrentMetaIDKey = "CurrentMetaID"; 
    
    // --- NEW: Key for the next city to unlock ---
    private const string NextCityToUnlockIDKey = "Next_City_To_Unlock_ID";

    // NEW: Key for tool unlock progress
    private const string ToolUnlockProgressKeyPrefix = "ToolUnlockProgress_";
    
    // --- NEW: Key for tool unlock availability
    private const string ToolUnlockAvailableKeyPrefix = "ToolUnlockAvailable_";

    // NEW: Key for total freed NPCs
    private const string TotalFreedNPCsKey = "TotalFreedNPCs";

    // --- NEW: Key for total levels repeated ---
    private const string TotalLevelsRepeatKey = "TotalLevelsRepeat";

    [SerializeField] private int _unlockedLevel;

    private string _equippedWeaponId;
    private string _equippedScannerId;

    public int UnlockedLevel
    {
        get { return _unlockedLevel; }
        set
        {
            if (_unlockedLevel != value)
            {
                _unlockedLevel = value;
                OnLevelUnlocked?.Invoke(_unlockedLevel);
            }
        }
    }

    public static event Action<string> OnWeaponEquipped;
    public static event Action<string> OnScannerEquipped;

    public delegate void OnLevelUnlockedEvent(int newLevel);
    public static event OnLevelUnlockedEvent OnLevelUnlocked;

    // Refactored Init to handle default equipment
    public void Init(string defaultWeaponId, string defaultScannerId)
    {
        _unlockedLevel = PlayerPrefs.GetInt(UnlockedLevelKey, 0); 
        _equippedWeaponId = PlayerPrefs.GetString(EquippedWeaponKey, string.Empty);
        _equippedScannerId = PlayerPrefs.GetString(EquippedScannerKey, string.Empty);

        // If no weapon is equipped, set the default one
        if (string.IsNullOrEmpty(_equippedWeaponId))
        {
            UnlockItem(defaultWeaponId);
            EquipWeapon(defaultWeaponId);
        }
        
        // If no scanner is equipped, set the default one
        if (string.IsNullOrEmpty(_equippedScannerId))
        {
            UnlockItem(defaultScannerId);
            EquipScanner(defaultScannerId);
        }
    }

    public int GetUnlockedLevel()
    {
        return UnlockedLevel;
    }

    public void UnlockNextLevel()
    {
        UnlockedLevel++;

        if (UnlockedLevel >= GameManager.Instance.levelManager.totalLevels)
        {
            //the loop of the game
            GameManager.Instance.levelManager._totalLevelsRepeat++;
            GameManager.Instance.progressionManager.SaveTotalLevelsRepeat(GameManager.Instance.levelManager._totalLevelsRepeat);
            UnlockedLevel = 0;
        }
        
        GameManager.Instance.saveLoadManager.SaveGameData();
    }

    public bool UnlockItem(string itemId)
    {
        if (!HasItem(itemId))
        {
            PlayerPrefs.SetInt(UnlockedItemsKeyPrefix + itemId, 1);
            GameManager.Instance.saveLoadManager.SaveGameData();
            return true;
        }
        return false;
    }

    public bool HasItem(string itemId)
    {
        return PlayerPrefs.GetInt(UnlockedItemsKeyPrefix + itemId, 0) == 1;
    }

    public void EquipWeapon(string weaponId)
    {
        if (!HasItem(weaponId))
        {
            return;
        }

        if (_equippedWeaponId != weaponId)
        {
            _equippedWeaponId = weaponId;
            PlayerPrefs.SetString(EquippedWeaponKey, _equippedWeaponId);
            GameManager.Instance.saveLoadManager.SaveGameData();
            OnWeaponEquipped?.Invoke(_equippedWeaponId);
        }
    }

    public void EquipScanner(string scannerId)
    {
        if (!HasItem(scannerId))
        {
            return;
        }

        if (_equippedScannerId != scannerId)
        {
            _equippedScannerId = scannerId;
            PlayerPrefs.SetString(EquippedScannerKey, _equippedScannerId);
            GameManager.Instance.saveLoadManager.SaveGameData();
            OnScannerEquipped?.Invoke(_equippedScannerId);
        }
    }

    public string GetEquippedWeaponId()
    {
        return _equippedWeaponId;
    }

    public string GetEquippedScannerId()
    {
        return _equippedScannerId;
    }

    public bool IsEquippedWeapon(string weaponId)
    {
        return _equippedWeaponId == weaponId;
    }

    public bool IsEquippedScanner(string scannerId)
    {
        return _equippedScannerId == scannerId;
    }
    
    // New methods for Meta and City progression
    public void SaveMetaProgress(string metaID, int coinsSpent)
    {
        PlayerPrefs.SetInt(MetaProgressKeyPrefix + metaID, coinsSpent);
    }

    public int LoadMetaProgress(string metaID)
    {
        return PlayerPrefs.GetInt(MetaProgressKeyPrefix + metaID, 0);
    }

    public void SaveCurrentMetaID(string metaID)
    {
        PlayerPrefs.SetString(CurrentMetaIDKey, metaID);
    }

    public string LoadCurrentMetaID()
    {
        return PlayerPrefs.GetString(CurrentMetaIDKey, string.Empty);
    }

    // --- UPDATED: Save city states and the next city to unlock ID
    public void SaveCityMetaStates(List<CityMeta> cityStates, int nextCityToUnlockID)
    {
        CityMetaListWrapper wrapper = new CityMetaListWrapper { cityMetaStates = cityStates };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(CityMetaStatesKey, json);
        PlayerPrefs.SetInt(NextCityToUnlockIDKey, nextCityToUnlockID);
    }

    public List<CityMeta> LoadCityMetaStates(int count)
    {
        string json = PlayerPrefs.GetString(CityMetaStatesKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            CityMetaListWrapper wrapper = JsonUtility.FromJson<CityMetaListWrapper>(json);
            if (wrapper != null && wrapper.cityMetaStates.Count == count)
            {
                return wrapper.cityMetaStates;
            }
        }
        
        List<CityMeta> defaultStates = new List<CityMeta>();
        for (int i = 0; i < count; i++)
        {
            defaultStates.Add(new CityMeta { isLocked = true });
        }
        return defaultStates;
    }

    // --- NEW: Load the next city to unlock ID separately ---
    public int LoadNextCityToUnlockID()
    {
        return PlayerPrefs.GetInt(NextCityToUnlockIDKey, 0);
    }

    public void ResetAllMetaProgress(List<string> allMetaIDs, int cityCount)
    {
        // 1. Reset the progress for all individual meta items.
        if (allMetaIDs != null)
        {
            foreach (string metaID in allMetaIDs)
            {
                PlayerPrefs.DeleteKey(MetaProgressKeyPrefix + metaID);
            }
        }
        
        // 2. Reset the currently loaded meta ID.
        PlayerPrefs.DeleteKey(CurrentMetaIDKey);
        
        // 3. Reset all city meta states to be locked and the next unlock ID.
        List<CityMeta> defaultStates = new List<CityMeta>();
        for (int i = 0; i < cityCount; i++)
        {
            defaultStates.Add(new CityMeta { isLocked = true });
        }
        SaveCityMetaStates(defaultStates, 0);
        
        // --- NEW: Delete the next city to unlock ID key ---
        PlayerPrefs.DeleteKey(NextCityToUnlockIDKey);

        // 4. Save changes to PlayerPrefs.
        PlayerPrefs.Save();
       // Debug.Log("All meta and city progression has been reset.");
    }
    
    public void SaveProgress()
    {
        PlayerPrefs.SetInt(UnlockedLevelKey, _unlockedLevel);
        PlayerPrefs.SetString(EquippedWeaponKey, _equippedWeaponId);
        PlayerPrefs.SetString(EquippedScannerKey, _equippedScannerId);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        UnlockedLevel = PlayerPrefs.GetInt(UnlockedLevelKey, 0);
        _equippedWeaponId = PlayerPrefs.GetString(EquippedWeaponKey, string.Empty);
        _equippedScannerId = PlayerPrefs.GetString(EquippedScannerKey, string.Empty);
    }

    public void SaveToolUnlockProgress(string toolID, int progress)
    {
        PlayerPrefs.SetInt(ToolUnlockProgressKeyPrefix + toolID, progress);
        PlayerPrefs.Save();
    }

    public int LoadToolUnlockProgress(string toolID)
    {
        return PlayerPrefs.GetInt(ToolUnlockProgressKeyPrefix + toolID, 0);
    }
    
    // --- NEW: Method to set a tool's unlock as available ---
    public void SetToolUnlockAvailable(string toolID)
    {
        PlayerPrefs.SetInt(ToolUnlockAvailableKeyPrefix + toolID, 1);
        PlayerPrefs.Save();
    }
    
    // --- NEW: Method to check if a tool's unlock is available
    public bool IsToolUnlockAvailable(string toolID)
    {
        return PlayerPrefs.GetInt(ToolUnlockAvailableKeyPrefix + toolID, 0) == 1;
    }
    
    // --- NEW: Method to claim a tool unlock and perform the actual unlock
    public void ClaimToolUnlock(string toolID)
    {
        // Perform the actual unlock
        UnlockItem(toolID);
        // Clear the unlock available flag
        PlayerPrefs.DeleteKey(ToolUnlockAvailableKeyPrefix + toolID);
        PlayerPrefs.Save();
    }


    public void SaveTotalFreedNPC(int totalFreed)
    {
        PlayerPrefs.SetInt(TotalFreedNPCsKey, totalFreed);
        PlayerPrefs.Save();
       // Debug.Log($"Saved total freed NPCs: {totalFreed}");
    }
    
    public int LoadTotalFreedNPC()
    {
        int totalFreed = PlayerPrefs.GetInt(TotalFreedNPCsKey, 0);
       // Debug.Log($"Loaded total freed NPCs: {totalFreed}");
        return totalFreed;
    }
    
    public void SaveTotalLevelsRepeat(int repeatCount)
    {
        PlayerPrefs.SetInt(TotalLevelsRepeatKey, repeatCount);
        PlayerPrefs.Save();
        //Debug.Log($"Saved total level repetitions: {repeatCount}");
    }
    

    public int LoadTotalLevelsRepeat()
    {
        int repeatCount = PlayerPrefs.GetInt(TotalLevelsRepeatKey, 0);
       // Debug.Log($"Loaded total level repetitions: {repeatCount}");
        return repeatCount;
    }
}

[System.Serializable]
public class CityMetaListWrapper
{
    public List<CityMeta> cityMetaStates;
}