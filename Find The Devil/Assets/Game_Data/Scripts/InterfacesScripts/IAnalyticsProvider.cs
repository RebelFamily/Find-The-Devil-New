using System.Collections.Generic;

public interface IAnalyticsProvider
{
    void Initialize();
    void LogEvent(string name, Dictionary<string, object> parameters);
    void SetUserProperty(string key, string value); // Optional
}