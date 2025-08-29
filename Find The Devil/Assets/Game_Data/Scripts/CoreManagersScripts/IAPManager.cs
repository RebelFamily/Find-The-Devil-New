// using UnityEngine;
// using UnityEngine.Purchasing; // For Unity IAP
// using System.Collections.Generic;
//
// // Implement IStoreListener for Unity IAP callbacks
// public class IAPManager : MonoBehaviour, IStoreListener
// {
//     private IStoreController storeController;
//     private IExtensionProvider storeExtensionProvider;
//
//     // Define your product IDs here
//     // These should match your IAP product definitions in Unity Dashboard/Google Play/App Store
//     public const string ProductId_Gems_SmallPack = "com.yourcompany.findthedevils.gems_small";
//     public const string ProductId_Gems_MediumPack = "com.yourcompany.findthedevils.gems_medium";
//     // ... other gem packs or direct unlocks
//
//     public void Init()
//     {
//         Debug.Log("IAPManager Initialized.");
//         if (storeController == null)
//         {
//             InitializeUnityIAP();
//         }
//     }
//
//     private void InitializeUnityIAP()
//     {
//         var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
//         
//         builder.AddProduct(ProductId_Gems_SmallPack, ProductType.Consumable);
//         builder.AddProduct(ProductId_Gems_MediumPack, ProductType.Consumable);
//         // ... add other products
//
//         UnityPurchasing.Initialize(this, builder);
//     }
//
//     public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//     {
//         Debug.Log("Unity IAP Initialized successfully!");
//         storeController = controller;
//         storeExtensionProvider = extensions;
//     }
//
//     public void OnInitializeFailed(InitializationFailureReason error)
//     {
//         Debug.LogError($"Unity IAP Initialization Failed: {error}");
//         // Handle error: show message to user, retry logic
//     }
//
//     public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//     {
//         Debug.Log($"Processing purchase for product: {args.purchasedProduct.definition.id}");
//
//         // Example: Award gems based on product ID
//         int gemsAwarded = 0;
//         string productId = args.purchasedProduct.definition.id;
//
//         switch (productId)
//         {
//             case ProductId_Gems_SmallPack:
//                 gemsAwarded = 100; // Define your amounts
//                 break;
//             case ProductId_Gems_MediumPack:
//                 gemsAwarded = 500;
//                 break;
//             // ... more cases
//             default:
//                 Debug.LogWarning($"Unhandled product ID: {productId}");
//                 break;
//         }
//
//         if (gemsAwarded > 0)
//         {
//             GameManager.Instance.EconomyManager.AddGems(gemsAwarded);
//             Debug.Log($"Awarded {gemsAwarded} gems from purchase of {productId}");
//             GameManager.Instance.AnalyticsService.LogMonetizationEvent(
//                 "iap_purchase_success", (float)args.purchasedProduct.metadata.localizedPrice,
//                 "real_money", productId); // Log real money value
//         }
//         else
//         {
//             Debug.LogWarning($"No gems awarded for product {productId}. Is it configured correctly?");
//         }
//
//         return PurchaseProcessingResult.Complete;
//     }
//
//     public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//     {
//         Debug.LogError($"Purchase failed for product {product.definition.id}: {failureReason}");
//         GameManager.Instance.AnalyticsService.LogMonetizationEvent(
//             "iap_purchase_failed", (float)product.metadata.localizedPrice,
//             "real_money", product.definition.id + "_" + failureReason.ToString());
//         // Inform user about failure
//     }
//
//     // Public method to initiate a purchase
//     public void PurchaseProduct(string productId)
//     {
//         if (storeController != null && storeController.products.WithID(productId) != null)
//         {
//             Debug.Log($"Attempting to purchase: {productId}");
//             storeController.InitiatePurchase(productId);
//         }
//         else
//         {
//             Debug.LogWarning($"Can't purchase {productId}. Unity IAP not initialized or product not found.");
//             // Inform user that IAP is not ready or product is unavailable
//         }
//     }
// }