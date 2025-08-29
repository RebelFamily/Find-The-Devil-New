using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public  string CoinsKey = "PlayerCoins";
    public  string GemsKey = "PlayerGems"; // New key for Gems

    private int _coins;
    private int _gems; // New field for Gems

    public int Coins
    {
        get { return _coins; }
        set
        {
            _coins = value;
            OnCoinsChanged?.Invoke(_coins);
        }
    }

    public int Gems 
    {
        get { return _gems; }
        set
        {
            _gems = value;
            OnGemsChanged?.Invoke(_gems);
        }
    }

    public delegate void OnCoinsChangedEvent(int newCoins);
    public static event OnCoinsChangedEvent OnCoinsChanged;
    
    public delegate void OnGemsChangedEvent(int newGems); 
    public static event OnGemsChangedEvent OnGemsChanged;

    public void Init()
    {
        GameManager.Instance.saveLoadManager.LoadCurrencies();
    }

    public void AddCoins(int amount)
    {
        if (amount < 0) return;
        Coins += amount;
        GameManager.Instance.saveLoadManager.SaveGameData();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0) { Debug.LogWarning("Cannot spend negative coins."); return false; }

        if (Coins >= amount)
        {
            Coins -= amount;
            GameManager.Instance.saveLoadManager.SaveGameData();
            return true;
        }

        return false;
    }

    public void AddGems(int amount) 
    {
        if (amount < 0) return;
        Gems += amount;
        GameManager.Instance.saveLoadManager.SaveGameData();
    }

    public bool SpendGems(int amount)
    {
        if (amount < 0) { Debug.LogWarning("Cannot spend negative gems."); return false; }
        if (Gems >= amount)
        {
            Gems -= amount;
            GameManager.Instance.saveLoadManager.SaveGameData();
            return true;
        }
        return false;
    }


    public int GetCoins()
    {
        return _coins;
    }

    public int GetGems()
    {
        return _gems;
    }
}