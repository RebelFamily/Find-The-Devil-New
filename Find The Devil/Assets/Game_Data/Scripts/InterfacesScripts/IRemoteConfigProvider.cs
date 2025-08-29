using System.Threading.Tasks;

public interface IRemoteConfigProvider
{
    Task<bool> FetchConfigs();
    string GetString(string key, string defaultValue = "");
    int GetInt(string key, int defaultValue = 0);
    bool GetBool(string key, bool defaultValue = false);
    float GetFloat(string key, float defaultValue = 0f);
}