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
    //ads hide warning check
    //public UnityEngine.Purchasing.ProductType purchaseableType = UnityEngine.Purchasing.ProductType.Consumable;
    public string purchaseID = "";
    public PurchaseType itemType;
    public string price;
    public string localPrice;
}