// using UnityEngine;
// using System.Collections.Generic;
// using System.Threading.Tasks;
//
// public enum AdPosition { Top, Bottom, Center }
//
// public class AdService : MonoBehaviour
// {
//     private List<IAdNetwork> adNetworks; // Dependency Inversion
//
//     public void Init()
//     {
//         adNetworks = new List<IAdNetwork>();
//         // Add concrete ad network implementations
//         adNetworks.Add(new MaxMediationAdNetwork());
//         adNetworks.Add(new AdMobAdNetwork());
//
//         foreach (var network in adNetworks)
//         {
//             network.Initialize();
//         }
//         Debug.Log("AdService Initialized with configured networks.");
//     }
//
//     public void ShowInterstitialAd()
//     {
//         // Implement logic for ad frequency from RemoteConfigService
//         bool canShowAd = GameManager.Instance.RemoteConfigService.GetConfigValue("interstitial_ad_frequency_enabled") == "true";
//         if (!canShowAd)
//         {
//             Debug.Log("Interstitial ads disabled by remote config.");
//             return;
//         }
//
//         // Try to show ad from any available network
//         foreach (var network in adNetworks)
//         {
//             if (network.TryShowInterstitial()) // Assume TryShowInterstitial returns bool and handles loading
//             {
//                 Debug.Log($"Interstitial ad shown by {network.GetType().Name}");
//                 return;
//             }
//         }
//         Debug.LogWarning("No ad network could show an interstitial ad.");
//     }
//
//     public async Task<bool> ShowRewardedAd()
//     {
//         // Try to show rewarded ad from any available network
//         foreach (var network in adNetworks)
//         {
//             bool success = await network.ShowRewarded(); // Assume async method
//             if (success)
//             {
//                 Debug.Log($"Rewarded ad shown by {network.GetType().Name}");
//                 return true;
//             }
//         }
//         Debug.LogWarning("No ad network could show a rewarded ad.");
//         return false;
//     }
//
//     public void ShowBannerAd(AdPosition position)
//     {
//         foreach (var network in adNetworks)
//         {
//             network.ShowBanner(position);
//         }
//     }
// }