using System;
using GameAnalyticsSDK;
using UnityEngine;
using GoogleMobileAds.Api;
public class AdmobManager : MonoBehaviour
{
    public static AdmobManager Instance;
    private RewardedAd _rewardedAd;
    private BannerView _banner, _rectBanner;
    private AdSize _rectangleAdSize;
    private InterstitialAd _interstitialAd;
    private bool _isAdmobInitialized = false, _isBannerReady, _isRectBannerReady;
    [SerializeField] public string bannerID, rectBannerID, interstitialAdID, rewardedAdID;
    [SerializeField] private AppOpenAdCaller appOpenAdCaller;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        GameAnalytics.Initialize();
        MobileAds.Initialize(initStatus =>
        {
            PrintStatus("Admob Initialize Successfully");
            _isAdmobInitialized = true;
            LoadAds();
        });
    }
    private void LoadAds()
    {
        CreateInterstitial();
        RequestRewarded();
        appOpenAdCaller.InitAppOpen();
    }

    #region RectBanner
    public void RequestRectBanner()
    {
        if (IsRectBannerReady())
        {
            ShowRectBanner();
            return;
        }
        var adUnitId = rectBannerID;
        PrintStatus("Admob RequestRectBanner");
        _rectBanner = new BannerView(adUnitId, AdSize.MediumRectangle, AdPosition.BottomLeft);
        _rectBanner.OnBannerAdLoaded += () =>
        {
            PrintStatus("RectBanner ad loaded.");
            _isRectBannerReady = true;
        };
        _rectBanner.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            PrintStatus("RectBanner ad failed to load with error: " + error.GetMessage());
            _isRectBannerReady = false;
        };
        _rectBanner.OnAdPaid += (AdValue adValue) =>
        {
            SendPaidEvent(adValue, AppmetricaAnalytics.AdFormat.MREC, adUnitId);
        };
        _rectBanner.LoadAd(CreateAdRequest());
    }
    public void HideRectBanner()
    {
        PrintStatus("Admob HideRectBanner");
        _rectBanner?.Hide();

    }
    public void DestroyRectBanner()
    {
        PrintStatus("Admob DestroyRectBanner");
        _isRectBannerReady = false;
        _rectBanner?.Destroy();
    }
    public void ShowRectBanner()
    {
        PrintStatus("Admob ShowRectBanner:" + IsRectBannerReady());
        if (IsRectBannerReady())
        {
            _rectBanner?.Show();
        }
    }
    public bool IsRectBannerReady()
    {
        return _isRectBannerReady;
    }

    #endregion

    #region Banner

    private void RequestBanner()
    {
        var adUnitId = bannerID;
        PrintStatus("Admob RequestBanner");
        _banner?.Destroy();
        var widthInPixels = Screen.safeArea.width > 0 ? Screen.safeArea.width : Screen.width;
        var width = (int)(widthInPixels / MobileAds.Utils.GetDeviceScale());
        _rectangleAdSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);
        _banner = new BannerView(adUnitId, _rectangleAdSize, AdPosition.Top);
        _banner.OnBannerAdLoaded += () =>
        {
            PrintStatus("Banner ad loaded.");
            _isBannerReady = true;
        };
        _banner.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            PrintStatus("Banner ad failed to load with error: " + error.GetMessage());
            _isBannerReady = false;
        };
        _banner.OnAdPaid += (AdValue adValue) =>
        {
            SendPaidEvent(adValue, AppmetricaAnalytics.AdFormat.Banner, adUnitId);
        };
        _banner.LoadAd(CreateAdRequest());
    }
    public void HideBanner()
    {
        PrintStatus("Admob HideBanner");
        _banner?.Hide();
    }
    public void DestroyBanner()
    {
        PrintStatus("Admob DestroyBanner");
        _isBannerReady = false;
        _banner?.Destroy();
    }
    public void ShowBanner()
    {
        PrintStatus("Admob ShowBanner:" + IsBannerReady());
        if (IsBannerReady())
        {
            _banner?.Show();
        }
        else
        {
            DestroyBanner();
            _isBannerReady = false;
            RequestBanner();
        }
    }
    public bool IsBannerReady()
    {
        return _isBannerReady;
    }

    #endregion

    #region Interstitial

    private void CreateInterstitial()
    {
        var adUnitId = interstitialAdID;
        Debug.Log("CreateInterstitial");

        PrintStatus("Admob RequestInterstitial");
        _interstitialAd?.Destroy();
        InterstitialAd.Load(adUnitId, CreateAdRequest(),
            (InterstitialAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("Interstitial ad failed to load with error: " + loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("Interstitial ad failed to load.");
                    return;
                }
                PrintStatus("Interstitial ad loaded.");
                _interstitialAd = ad;
                ad.OnAdFullScreenContentClosed += () =>
                {
                    PrintStatus("Interstitial ad closed.");
                    CreateInterstitial();
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    PrintStatus("Interstitial ad failed to show with error: " + error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    SendPaidEvent(adValue, AppmetricaAnalytics.AdFormat.Interstitial, adUnitId);
                };
            });
    }
    public void ShowInterstitial()
    {
        PrintStatus("Admob Show Interstitial Low");
        if (IsInterstitialReady())
        {
            AppOpenAdCaller.IsInterstitialAdPresent = true;
            _interstitialAd.Show();
            AnalyticsManager.Instance.SendShowingInterAdmobEvent();
        }
        else
        {
            PrintStatus("Interstitial Failed");
            AnalyticsManager.Instance.SendFailedInterEvent();
        }
    }
    public bool IsInterstitialReady()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
            return true;
        else
        {
            CreateInterstitial();
            return false;
        }
    }

    #endregion

    #region Rewarded
    private void RequestRewarded()
    {
        var adUnitId = rewardedAdID;
        PrintStatus("Admob RequestRewarded");
        RewardedAd.Load(adUnitId, CreateAdRequest(),
            (RewardedAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("Rewarded ad failed to load with error: " + loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("Rewarded ad failed to load.");
                    return;
                }

                PrintStatus("Rewarded ad loaded.");
                _rewardedAd = ad;

                ad.OnAdFullScreenContentClosed += () =>
                {
                    RequestRewarded();
                    PrintStatus("Rewarded ad closed.");
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    PrintStatus("Rewarded ad failed to show with error: " +error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    SendPaidEvent(adValue, AppmetricaAnalytics.AdFormat.Rewarded, adUnitId);
                };
            });
    }
    public void ShowRewardedAd(Action onSuccess)
    {
        if (!_isAdmobInitialized)
        {
            PrintStatus("No Video Ad Available");
            return;
        }
        PrintStatus("Admob ShowRewardedAd:"+IsRewardedAdReady());
        if (IsRewardedAdReady())
        {
            AppOpenAdCaller.IsInterstitialAdPresent = true;
            AnalyticsManager.Instance.SendShowingRewardedAdmobEvent();
            _rewardedAd.Show((Reward reward) =>
            {
                onSuccess?.Invoke();
            });
        }
        else
        {
            PrintStatus("Rewarded Failed");
            AnalyticsManager.Instance.SendFailedRewardedEvent();
        }
    }
    public bool IsRewardedAdReady()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
            return true;
        else
        {
            RequestRewarded();
            return false;
        }
    }

    #endregion
    
    private const string UserStr = "unity-admob-sample";
    private static AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword(UserStr)
            .Build();
    }
    private static void PrintStatus(string msg)
    {
        AnalyticsManager.Instance.ShowLogs(msg);
    }
    private static void SendPaidEvent(AdValue adValue, AppmetricaAnalytics.AdFormat adFormat, string adUnit, string placementName = null)
    {
        AnalyticsManager.Instance.RevReport_Admob(adValue, adFormat, adUnit);
    }
}