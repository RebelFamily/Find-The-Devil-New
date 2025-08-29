// using UnityEngine;
// using System.Collections.Generic;
// // Assuming AppMetrica SDK imported
// // using Yandex.Metrica;
//
// public class AppMetricaProvider : IAnalyticsProvider
// {
//     public void Initialize()
//     {
//         Debug.Log("AppMetrica Provider Initializing...");
//         // YandexMetrica.ActivateWithApiKey("YOUR_APP_METRICA_API_KEY");
//         // YandexMetrica.ResumeSession();
//         Debug.Log("AppMetrica Provider Initialized (mock).");
//     }
//
//     public void LogEvent(string name, Dictionary<string, object> parameters)
//     {
//         // Example for AppMetrica
//         // if (parameters != null)
//         // {
//         //     YandexMetrica.ReportEvent(name, parameters);
//         // }
//         // else
//         // {
//         //     YandexMetrica.ReportEvent(name);
//         // }
//         Debug.Log($"AppMetrica LogEvent: {name} with parameters {Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}"); // Mock
//     }
// }