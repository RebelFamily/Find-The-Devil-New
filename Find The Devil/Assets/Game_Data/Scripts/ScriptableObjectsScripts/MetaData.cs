using UnityEngine;

[CreateAssetMenu(fileName = "NewMetaData", menuName = "Game Data/Meta Data")] 
public class MetaData : ScriptableObject
{
    [Header("Core Meta Information")]
    public string metaID;
    public string metaName;
    public MetaType metaType; 
    public GameObject metaPrefab; 
    public int metaCompletionCoins;
    public AudioManager.GameSound metaBgSouns;

    [Tooltip("The total amount of coins required to fully repair this building.")]
    public int coinsRequiredToRepair;    
    

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(metaID))
        {
            Debug.LogWarning($"MetaData asset '{name}' has an empty Meta ID. Please provide a unique ID.");
        }
    }
}

public enum MetaType
{
    None,
    City,
    Level,      
    Achievement,
    Unlockable,
    Reward,
    Setting,
    Building 
}