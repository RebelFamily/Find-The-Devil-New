using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RemoteConfigService : MonoBehaviour
{
    private IRemoteConfigProvider provider; // Dependency Inversion

    public void Init()
    {
        provider = new FirebaseRemoteConfigProvider();
//        Debug.Log("RemoteConfigService Initialized.");
        FetchConfigs(); 
    }

    public async void FetchConfigs()
    {
        bool success = await provider.FetchConfigs();
        if (success)
        {
          //  Debug.Log("Remote Configs fetched successfully.");
            // GameManager.Instance.AdService.ApplyAdFrequencyFromConfig();
        }
        else
        {
           // Debug.LogError("Failed to fetch remote configs.");
        }
    }

    public string GetConfigValue(string key, string defaultValue = "")
    {
        return provider.GetString(key, defaultValue);
    }

    public int GetConfigValue(string key, int defaultValue = 0)
    {
        return provider.GetInt(key, defaultValue);
    }

    public bool GetConfigValue(string key, bool defaultValue = false)
    {
        return provider.GetBool(key, defaultValue);
    }
}