/*
using GameAnalyticsSDK;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance;
    [HideInInspector] public long adTimer = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);
        
        GameAnalytics.Initialize();
    }
}
*/
