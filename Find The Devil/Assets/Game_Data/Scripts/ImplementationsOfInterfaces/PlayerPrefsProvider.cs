using UnityEngine;

public class PlayerPrefsProvider : IDataProvider
{
    public void Save(string key, string data)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
       // Debug.Log($"Saved {key} to PlayerPrefs.");
    } 
    public void Save(string key, int data)
    {
        PlayerPrefs.SetInt(key, data);
        PlayerPrefs.Save();
      //  Debug.Log($"Saved {key} to PlayerPrefs.");
    }

    public int Load(string key, int data)
    {
        if (PlayerPrefs.HasKey(key))
        {
            data = PlayerPrefs.GetInt(key);
//            Debug.Log($"Loaded {key} from PlayerPrefs.");
            return data;
        }
        Debug.LogWarning($"No data found for {key} in PlayerPrefs.");
        return 0;
    }

    public string Load(string key, string data)
    {
        if (PlayerPrefs.HasKey(key))
        { 
            data = PlayerPrefs.GetString(key);
         //   Debug.Log($"Loaded {key} from PlayerPrefs.");
            return data;
        }
        Debug.LogWarning($"No data found for {key} in PlayerPrefs.");
        return "";
    }

    public void Delete(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
       // Debug.Log($"Deleted {key} from PlayerPrefs.");
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
}