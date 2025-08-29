using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalPrice : MonoBehaviour
{
    Text priceText;
    public PurchaseType itemType;
    void Start()
    {
        // //print("My Item Type: " + itemType);
        // priceText = GetComponent<Text>();
        // for (int i = 0; i < IAPManager.Instance.purchaseIDController.Length; i++)
        // {
        //     if (itemType == IAPManager.Instance.purchaseIDController[i].itemType)
        //     {
        //         priceText.text = IAPManager.Instance.purchaseIDController[i].localPrice;
        //         break;
        //     }
        // }
    }
}