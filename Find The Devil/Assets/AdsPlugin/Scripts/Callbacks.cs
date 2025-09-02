using UnityEngine;
public class Callbacks : MonoBehaviour {
    public delegate void RewardItem();
    public static event RewardItem OnRewardItem;
    public static RewardType rewardType;
    private void Start () 
    {
		DontDestroyOnLoad (gameObject);
	}
    public static void RewardedAdWatched ()
	{
        switch (rewardType)
        {
            case RewardType.RewardItem:
                OnRewardItem?.Invoke();
                AnalyticsManager.Instance.SendRewardReceivedEvent(rewardType.ToString());
                break;
            
        }
    }
    public enum RewardType
    {
        RewardItem
    }
}