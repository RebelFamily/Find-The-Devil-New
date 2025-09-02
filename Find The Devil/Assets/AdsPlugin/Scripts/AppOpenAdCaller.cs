using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
public class AppOpenAdCaller : MonoBehaviour
{
    [SerializeField] private string appOpenID = "";
    [SerializeField] private ScreenOrientation orientation= ScreenOrientation.LandscapeLeft;
    public static bool IsInterstitialAdPresent;
    private AppOpenAd _appOpenAd;
    private readonly TimeSpan _appOpenTimeout = TimeSpan.FromHours(6);
    private DateTime _appOpenExpireTime;
    
    private void OnDestroy()
    {
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }
    public void InitAppOpen()
    {
        PrintStatus("Initializing AppOpen...");
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        Invoke(nameof(ShowAppOpenAd), 3f);
    }

    #region APPOPEN ADS

    private bool IsAppOpenAdAvailable
    {
        get
        {
            return (_appOpenAd != null
                    && _appOpenAd.CanShowAd()
                    && DateTime.Now < _appOpenExpireTime);
        }
    }
    private void OnAppStateChanged(AppState state)
    {
        PrintStatus("App State is " + state);
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (state == AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        });
    }
    private void RequestAndLoadAppOpenAd()
    {
        PrintStatus("Requesting App Open ad.");
        var adUnitId = appOpenID;
        if (_appOpenAd != null)
        {
            DestroyAppOpenAd();
        }
        AppOpenAd.Load(adUnitId, orientation, CreateAdRequest(),
            (AppOpenAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("App open ad failed to load with error: " + loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("App open ad failed to load.");
                    return;
                }
                PrintStatus("App Open ad loaded. Please background the app and return.");
                this._appOpenAd = ad;
                this._appOpenExpireTime = DateTime.Now + _appOpenTimeout;

                ad.OnAdFullScreenContentClosed += () =>
                {
                    PrintStatus("App open ad closed."); 
                    AdsCaller.Instance.ShowBanner();
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    PrintStatus("App open ad failed to show with error: " + error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    AnalyticsManager.Instance.RevReport_Admob(adValue, AppmetricaAnalytics.AdFormat.AppOpen, adUnitId);
                };
            });
    }
    private void DestroyAppOpenAd()
    {
        if (this._appOpenAd == null) return;
        this._appOpenAd.Destroy();
        this._appOpenAd = null;
    }
    public void ShowAppOpenAd()
    {
        if (AdsCaller.Instance._isRemoveAdsPurchased)
        {
            return;
        }

        if (IsInterstitialAdPresent)
        {
            IsInterstitialAdPresent = false;
            return;
        }
        
        if (!IsAppOpenAdAvailable)
        {
            RequestAndLoadAppOpenAd();
            return;
        }
        _appOpenAd.Show();
        AdsCaller.Instance.HideBanner();
        AdsCaller.Instance.DestroyRectBanner();
    }

    #endregion
    
    #region HELPER METHODS

    private const string UserStr = "unity-admob-sample";
    private static AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword(UserStr)
            .Build();
    }

    #endregion
    
    #region Utility

    private static void PrintStatus(string message)
    {
        AnalyticsManager.Instance.ShowLogs(message);
    }

    #endregion
}