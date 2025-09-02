public enum PurchaseType
{
    RemoveAds,
    VIPGuns,
    AllGuns,
    AllScanners
}

[System.Serializable]
public class InAppProduct
{
    public UnityEngine.Purchasing.ProductType purchaseableType = UnityEngine.Purchasing.ProductType.Consumable;
    public string purchaseID = "";
    public PurchaseType itemType;
    public string price;
    public string localPrice;
}