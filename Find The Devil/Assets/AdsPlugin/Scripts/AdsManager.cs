using System;
using UnityEngine;
public class AdsManager : MonoBehaviour
{
    [SerializeField] private string maxSdkKey = "6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR";
    [SerializeField] private string interstitialAdUnitId = "0bf5dd259a7babe3";
    [SerializeField] private  string rewardedAdUnitId = "5d75002bbc4126b9";
    [SerializeField] private string bannerAdUnitId = "YOUR_BANNER_AD_UNIT_ID";
    [SerializeField] private string mRecAdUnitId = "ENTER_MREC_AD_UNIT_ID_HERE";

    private bool _isBannerReady, _isBannerInitialized;
    private bool _isMRecShowing;
    private int _interstitialRetryAttempt;
    private int _rewardedRetryAttempt;
    private bool
        _isRectBannerReady,
        _isRectBannerInitialized;
    
    public static AdsManager Instance;
    private Action OnReward;
    private void Start()
    {
        if (Instance == null)
            Instance = this;
        DontDestroyOnLoad(this.gameObject);
        InitializeMax();
    }
    private void InitializeMax()
    {

        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            PrintStatus("MAX SDK Initialized");
            RegisterPaidAdEvent();
            InitializeBannerAds();
            InitializeInterstitialAd();
            InitializeRewardedAds();
            InitializeMRecAds();
        };
        MaxSdk.SetSdkKey(maxSdkKey);
        MaxSdk.SetVerboseLogging(AnalyticsManager.Instance.AreLogsEnabled());
        MaxSdk.InitializeSdk();
    }

    #region Interstitial Ad Methods

    private void InitializeInterstitialAd()
    {
        Debug.Log("InitializeInterstitialAd");
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnSimpleInterstitialDismissedEvent;
        LoadInterstitial();
    }
    public void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(interstitialAdUnitId);
        PrintStatus(interstitialAdUnitId + "InterstitialAdUnitId");
    }
    public void ShowInterstitial()
    {
        AppOpenAdCaller.IsInterstitialAdPresent = true;
        MaxSdk.ShowInterstitial(interstitialAdUnitId);
        AnalyticsManager.Instance.SendShowingInterApplovinEvent();
    }
    public bool IsInterstitialReady()
    {
        return MaxSdk.IsInterstitialReady(interstitialAdUnitId);
    }
    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Interstitial loaded");
        _interstitialRetryAttempt = 0;
    }
    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorCode)
    {
        PrintStatus(adUnitId + "InterstitialAdUnitId");
        PrintStatus("Interstitial failed to load with error code: " + errorCode);
        _interstitialRetryAttempt++;
        var retryDelay = Math.Pow(2, _interstitialRetryAttempt);
        Invoke(nameof(LoadInterstitial), (float)retryDelay);
    }
    private void InterstitialFailedToDisplayEvent(string adUnitIdMaxSdkBase, MaxSdkBase.ErrorInfo errorCode, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Interstitial failed to display with error code: " + errorCode);
        LoadInterstitial();
    }
    private void OnSimpleInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Interstitial dismissed");
        LoadInterstitial();
    }

    #endregion

    #region Rewarded Ad Methods

    private void InitializeRewardedAds()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        LoadRewardedAd();
    }
    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(rewardedAdUnitId);
    }
    public void ShowRewardedAd(Action onSuccess)
    {
        AppOpenAdCaller.IsInterstitialAdPresent = true;
        MaxSdk.ShowRewardedAd(rewardedAdUnitId);
        OnReward = onSuccess;
        AnalyticsManager.Instance.SendShowingRewardedApplovinEvent();
    }
    public bool IsRewardedAdAvailable()
    {
        return MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
    }
    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Rewarded ad loaded");
        _rewardedRetryAttempt = 0;
    }
    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorCode)
    {
        PrintStatus("Rewarded ad failed to load with error code: " + errorCode);
        _rewardedRetryAttempt++;
        var retryDelay = Math.Pow(2, _rewardedRetryAttempt);
        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }
    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorCode, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Rewarded ad failed to display with error code: " + errorCode);
        LoadRewardedAd();
    }
    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        AppOpenAdCaller.IsInterstitialAdPresent = true;
        PrintStatus("Rewarded ad displayed");
    }
    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Rewarded ad dismissed");
        LoadRewardedAd();
    }
    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        OnReward?.Invoke();
        PrintStatus("Rewarded ad received reward");
    }

    #endregion

    #region Banner Ad Methods

    private void InitializeBannerAds()
    {
        PrintStatus("InitializeSimpleBannerAds Max");
        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, new Color(0, 0, 0, 0));
        if(_isBannerInitialized) return;
        _isBannerInitialized = true;
        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
    }
    public void ShowBanner()
    {
        MaxSdk.ShowBanner(bannerAdUnitId);
    }
    public void HideBanner()
    {
        MaxSdk.HideBanner(bannerAdUnitId);
    }
    public void DestroyBanner()
    {
        _isBannerReady = false;
        MaxSdk.DestroyBanner(bannerAdUnitId);
    }
    public bool IsBannerAdAvailable()
    {
        return _isBannerReady;
    }
    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        PrintStatus("Max simple banner is loaded");
        _isBannerReady = true;
    }
    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        PrintStatus("Max simple banner is failed");
        _isBannerReady = false;
    }
    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Max simple banner is collapsed");
        _isBannerReady = false;
    }
    #endregion
    
    #region MREC Ad Methods

    private void InitializeMRecAds()
    {
        // MRECs are automatically sized to 300x250.
        MaxSdk.CreateMRec(mRecAdUnitId, MaxSdkBase.AdViewPosition.Centered);

        if (_isRectBannerInitialized) return;
        _isRectBannerInitialized = true;

        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnRectBannerAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnRectBannerAdLoadFailedEvent;
        MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnRectBannerAdCollapsedEvent;
    }

    private void ToggleMRecVisibility()
    {
        if (!_isMRecShowing)
        {
            MaxSdk.ShowMRec(mRecAdUnitId);
            // showMRecButton.GetComponentInChildren<Text>().text = "Hide MREC";
        }
        else
        {
            MaxSdk.HideMRec(mRecAdUnitId);
            // showMRecButton.GetComponentInChildren<Text>().text = "Show MREC";
        }

        _isMRecShowing = !_isMRecShowing;
    }

    public void ShowMREC()
    {
        MaxSdk.ShowMRec(mRecAdUnitId);
    }

    public void HideMREC()
    {
        MaxSdk.HideMRec(mRecAdUnitId);
    }

    public bool IsRectBannerAdAvailable()
    {
        return _isRectBannerReady;
    }

    private void OnRectBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Max simple banner is loaded");
        _isRectBannerReady = true;
    }

    private void OnRectBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.Log("Max simple banner is failed");
        _isRectBannerReady = false;
    }

    private void OnRectBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Max simple banner is collapsed");
        _isRectBannerReady = false;
    }

    #endregion
    
    private void RegisterPaidAdEvent()
    {
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += MaxHandleInterstitialPaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += MaxHandleRewardedPaidEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += MaxHandleBannerPaidEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += MaxHandleMRecPaidEvent;
    }
    private void MaxHandleInterstitialPaidEvent(string sender, MaxSdkBase.AdInfo adInfo)
    {
        AnalyticsManager.Instance.RevReport_Max(interstitialAdUnitId, adInfo);
    }
    private void MaxHandleRewardedPaidEvent(string sender, MaxSdkBase.AdInfo adInfo)
    {
        AnalyticsManager.Instance.RevReport_Max(rewardedAdUnitId, adInfo);
    }
    private void MaxHandleBannerPaidEvent(string sender, MaxSdkBase.AdInfo adInfo)
    {
        AnalyticsManager.Instance.RevReport_Max(bannerAdUnitId, adInfo);
    }
    private void MaxHandleMRecPaidEvent(string sender, MaxSdkBase.AdInfo adInfo)
    {
        AnalyticsManager.Instance.RevReport_Max(mRecAdUnitId, adInfo);
    }
    private static void PrintStatus(string msg)
    {
        AnalyticsManager.Instance.ShowLogs(msg);
    }
}