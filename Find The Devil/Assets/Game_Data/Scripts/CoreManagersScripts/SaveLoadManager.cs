using UnityEngine;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    private IDataProvider _dataProvider;

    public void Init()
    {
        _dataProvider = new PlayerPrefsProvider();
       // Debug.Log($"SaveLoadManager Initialized with {_dataProvider.GetType().Name}.");
        LoadGameData();
    }

    public void SetDataProvider(IDataProvider provider)
    {
        _dataProvider = provider;
    }
 
    public void SaveGameData()
    {
        SaveCurrencies(); // Now saves both coins and gems
        GameManager.Instance.progressionManager.SaveProgress();
        
      //  Debug.Log("Game data saved.");
    }

    public void LoadGameData()
    {
        LoadCurrencies(); // Now loads both coins and gems
        GameManager.Instance.progressionManager.LoadProgress();

       // Debug.Log("Game data loaded.");
    }
    
    public void SaveCurrencies() 
    {
        _dataProvider.Save(GameManager.Instance.economyManager.CoinsKey,GameManager.Instance.economyManager.Coins);
        _dataProvider.Save(GameManager.Instance.economyManager.GemsKey,GameManager.Instance.economyManager.Coins);
    }

    public void LoadCurrencies()
    {
        GameManager.Instance.economyManager.Coins = _dataProvider.Load(GameManager.Instance.economyManager.CoinsKey,0);
        GameManager.Instance.economyManager.Gems = _dataProvider.Load(GameManager.Instance.economyManager.GemsKey,0);
    }
}