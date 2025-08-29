// using UnityEngine;
// using System.Collections.Generic;
//
// public class AnalyticsService : MonoBehaviour
// {
//     private List<IAnalyticsProvider> providers; // Dependency Inversion
//
//     public void Init()
//     {
//         providers = new List<IAnalyticsProvider>();
//         // Add concrete analytics provider implementations
//         providers.Add(new FirebaseAnalyticsProvider());
//         providers.Add(new GameAnalyticsProvider());
//         providers.Add(new AppMetricaProvider());
//
//         foreach (var provider in providers)
//         {
//             provider.Initialize(); // Assume an Initialize method for providers
//         }
//         Debug.Log("AnalyticsService Initialized with configured providers.");
//     }
//
//     public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
//     {
//         foreach (var provider in providers)
//         {
//             provider.LogEvent(eventName, parameters);
//         }
//         Debug.Log($"Logged event: {eventName} with params: {Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}"); // For debugging
//     }
//
//     public void LogLevelComplete(int levelId, float timeTaken)
//     {
//         LogEvent("level_completed", new Dictionary<string, object>
//         {
//             {"level_id", levelId},
//             {"time_taken", timeTaken}
//         });
//     }
//
//     public void LogMonetizationEvent(string type, float value, string currency, string itemId = null)
//     {
//         var parameters = new Dictionary<string, object>
//         {
//             {"type", type},
//             {"value", value},
//             {"currency", currency} // Add currency type
//         };
//         if (itemId != null)
//         {
//             parameters.Add("item_id", itemId);
//         }
//         LogEvent("monetization_event", parameters);
//     }
//
//     // Other specific logging methods (e.g., game_start, item_purchase)
// }