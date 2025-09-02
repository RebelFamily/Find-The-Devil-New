using com.adjust.sdk;
using Firebase.Analytics;
using GameAnalyticsSDK;
using GoogleMobileAds.Api;
using UnityEngine;
public class AnalyticsManager : MonoBehaviour
{
    #region Constants

    private const string UserBannerString = "BANNER", UserInterString = "INTER", UserRewardedString = "REWARDED", UserAppOpenString = "APPOPEN", UserUsdString = "USD",
        UserAppLovingString = "AppLovin", UserAdPlatformString = "ad_platform", UserAdSourceString = "ad_source", UserAdUnitNameString = "ad_unit_name", 
        UserAdFormatString = "ad_format", UserValueString = "value", UserCurrencyString = "currency", UserPaidAdImpressionString = "paid_ad_impression", 
        UserAdMobString = "AdMob", UserGoogleAdManagerString = "Google Ad Manager Native", UserGoogleAdMobString = "Google AdMob", UserAdImpressionString = "ad_impression";
    private const string UserLevelString = "Level", MetaString = "Meta_",CityMetaString = "CityMeta_", UserLevelUnderScoreString = "Level_", UserModeString = "Mode", UserModeUnderScoreString = "Mode_";
    private const string UserAdmobString = "Admob", UserUnderScoreString = "_", UserAdInterString = "ad_inter", UserAdBannerString = "ad_banner", BothString = "Both";
    
    #endregion
    
    [SerializeField] private bool showLogs = false;
    [SerializeField] private FirebaseManager firebaseManager;
    public static AnalyticsManager Instance;
    public enum AdPlacements
    {
        LevelComplete = 1,
        LevelFail = 2,
        NextLevel = 3,
        LevelRestart = 4,
        PauseGame = 5,
        LevelSelection = 6
    }
    private void Awake()
    {
        Instance = this;
        GameAnalytics.Initialize();
        firebaseManager.OnFireBase();
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Debug.unityLogger.logEnabled = showLogs;
    }
    public void ShowLogs(string str)
    {
        if(showLogs)
            Debug.Log(str);
    }
    public bool AreLogsEnabled()
    {
        return showLogs;
    }
    
    #region ProgressionEvents
    
    public void ProgressionEventSingleMode(GAProgressionStatus status, string levelNo)
    {
        var msg = UserLevelString + levelNo;
        ShowLogs("Sending Progression Event SingleMode: " + status +" _ "+ msg);
        GameAnalytics.NewProgressionEvent(status, msg);
        FirebaseAnalytics.LogEvent(status + msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, UserLevelUnderScoreString + levelNo, status.ToString());
    }
    public void ProgressionEventMetaMode(GAProgressionStatus status, string levelNo)
    {
        var msg = MetaString + levelNo;
        ShowLogs("Sending Progression Event MetaMode: " + status +" _ " + msg);
        GameAnalytics.NewProgressionEvent(status, msg);
        FirebaseAnalytics.LogEvent(status + msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, UserLevelUnderScoreString + levelNo, status.ToString());
    }
    public void ProgressionEventMetaCityMode(GAProgressionStatus status, string levelNo)
    {
        var msg = CityMetaString + levelNo;
        ShowLogs("Sending Progression Event: " + status +" _ "+ msg);
        GameAnalytics.NewProgressionEvent(status, msg);
        FirebaseAnalytics.LogEvent(status + msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, UserLevelUnderScoreString + levelNo, status.ToString());
    }
    private void ProgressionEventMultipleModes(GAProgressionStatus status, int modeNo, int levelNo)
    {
        var msg = UserModeString + modeNo + UserLevelString + levelNo;
        ShowLogs("Sending Progression Event: " + msg);
        GameAnalytics.NewProgressionEvent(status, msg);
        FirebaseAnalytics.LogEvent(status + msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, UserModeUnderScoreString + modeNo,
            UserLevelUnderScoreString + levelNo, status.ToString());
    }
    
    #endregion
    
    #region AdEvents
    
    public void SendShowingInterAdmobEvent()
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAdmobString + GAAdType.Interstitial;
        ShowLogs("SendShowingInterAdmobEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, UserAdmobString, UserAdmobString + GAAdType.Interstitial);
        FirebaseAnalytics.LogEvent(msg);
        FirebaseAnalytics.LogEvent(UserAdInterString);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingInterAdmobEvent(AdPlacements adPlacement)
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAdmobString + GAAdType.Interstitial + UserUnderScoreString + adPlacement;
        ShowLogs("SendShowingInterAdmobEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, UserAdmobString, adPlacement.ToString());
        FirebaseAnalytics.LogEvent(msg);
        FirebaseAnalytics.LogEvent(UserAdInterString);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingRewardedAdmobEvent()
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAdmobString + GAAdType.RewardedVideo;
        ShowLogs("SendShowingRewardedAdmobEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, UserAdmobString, UserAdmobString + GAAdType.RewardedVideo);
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingRewardedAdmobEvent(AdPlacements adPlacement)
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAdmobString + GAAdType.RewardedVideo + UserUnderScoreString + adPlacement;
        ShowLogs("SendShowingRewardedAdmobEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, UserAdmobString, adPlacement.ToString());
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingInterApplovinEvent()
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAppLovingString + GAAdType.Interstitial;
        ShowLogs("SendShowingInterApplovinEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, UserAppLovingString, UserAppLovingString + GAAdType.Interstitial);
        FirebaseAnalytics.LogEvent(msg);
        FirebaseAnalytics.LogEvent(UserAdInterString);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingInterApplovinEvent(AdPlacements adPlacement)
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAppLovingString + GAAdType.Interstitial + UserUnderScoreString + adPlacement;
        ShowLogs("SendShowingInterApplovinEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, UserAppLovingString, adPlacement.ToString());
        FirebaseAnalytics.LogEvent(msg);
        FirebaseAnalytics.LogEvent(UserAdInterString);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingRewardedApplovinEvent()
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAppLovingString + GAAdType.RewardedVideo;
        ShowLogs("SendShowingRewardedApplovinEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, UserAppLovingString, UserAppLovingString + GAAdType.RewardedVideo);
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendShowingRewardedApplovinEvent(AdPlacements adPlacement)
    {
        var msg = GAAdAction.Show + UserUnderScoreString + UserAppLovingString + GAAdType.RewardedVideo + UserUnderScoreString + adPlacement;
        ShowLogs("SendShowingRewardedApplovinEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, UserAppLovingString, adPlacement.ToString());
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendFailedInterEvent()
    {
        var msg = GAAdAction.FailedShow + UserUnderScoreString + GAAdType.Interstitial;
        ShowLogs("SendFailedInterEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.FailedShow, GAAdType.Interstitial, BothString, GAAdType.Interstitial.ToString());
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendFailedRewardedEvent()
    {
        var msg = GAAdAction.FailedShow + UserUnderScoreString + GAAdType.RewardedVideo;
        ShowLogs("SendFailedRewardedEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, BothString, GAAdType.RewardedVideo.ToString());
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    public void SendRewardReceivedEvent(string rewardName)
    {
        var msg = GAAdAction.RewardReceived + UserUnderScoreString + rewardName;
        ShowLogs("SendRewardReceivedEvent: " + msg);
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, BothString, rewardName);
        FirebaseAnalytics.LogEvent(msg);
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.AdsData, msg);
    }
    
    #endregion

    #region Paid Events for Adjust, AppMetrica & Firebase
    
    public void RevReport_Max(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        ShowLogs("Rev reporting for max");
        var revenue = impressionData.Revenue;
        var adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
        adjustAdRevenue.setRevenue(revenue, UserUsdString);
        adjustAdRevenue.setAdRevenueNetwork(impressionData.NetworkName);
        adjustAdRevenue.setAdRevenueUnit($"{impressionData.AdFormat}_{impressionData.AdUnitIdentifier}");
        Adjust.trackAdRevenue(adjustAdRevenue);
        var adFormat = AppmetricaAnalytics.AdFormat.Interstitial;
        var maxAdFormat = impressionData.AdFormat.ToString();
        adFormat = maxAdFormat switch
        {
            UserBannerString => AppmetricaAnalytics.AdFormat.Banner,
            UserInterString => AppmetricaAnalytics.AdFormat.Interstitial,
            UserRewardedString => AppmetricaAnalytics.AdFormat.Rewarded,
            UserAppOpenString => AppmetricaAnalytics.AdFormat.AppOpen,
            _ => adFormat
        };
        AppmetricaAnalytics.ReportRevenue_Applovin(impressionData, adFormat);
        if (!FirebaseManager.IsFirebaseInitialized()) return;
        if(adFormat == AppmetricaAnalytics.AdFormat.Banner)
            FirebaseAnalytics.LogEvent(UserAdBannerString);
        var impressionParameters = new[] {
            new Parameter(UserAdPlatformString, UserAppLovingString),
            new Parameter(UserAdSourceString, impressionData.NetworkName),
            new Parameter(UserAdUnitNameString, impressionData.AdUnitIdentifier),
            new Parameter(UserAdFormatString, impressionData.AdFormat),
            new Parameter(UserValueString, revenue),
            new Parameter(UserCurrencyString, UserUsdString),
        };
        FirebaseAnalytics.LogEvent(UserPaidAdImpressionString, impressionParameters);
        if (impressionData.NetworkName is not (UserAdMobString or UserGoogleAdManagerString or UserGoogleAdMobString))
        {
            FirebaseAnalytics.LogEvent(UserAdImpressionString, impressionParameters);
        }
    }
    public void RevReport_Admob(AdValue adValue, AppmetricaAnalytics.AdFormat adFormat, string adUnitId)
    {
        ShowLogs("Rev reporting for admob");
        double rev = adValue.Value / 1000000f;
        var adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
        adjustAdRevenue.setRevenue(rev, UserUsdString);
        Adjust.trackAdRevenue(adjustAdRevenue);
        AppmetricaAnalytics.ReportRevenue_Admob(adValue, adFormat, adUnitId);
        if (!FirebaseManager.IsFirebaseInitialized()) return;
        FirebaseAnalytics.LogEvent(UserPaidAdImpressionString);
        if(adFormat == AppmetricaAnalytics.AdFormat.Banner)
            FirebaseAnalytics.LogEvent(UserAdBannerString);
    }

    #endregion
}