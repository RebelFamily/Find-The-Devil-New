using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Linq;

public class AdsCaller : MonoBehaviour
{
    [SerializeField] private GameObject adsUI;
    public static AdsCaller Instance;
    private float _time = 0;
    private bool _startTimer = false;
    private bool _adReady = false;

    public bool _isAppOpenAddReady = false;
    public bool _isRemoveAdsPurchased = false; // New variable to track the purchase
    public bool _isGetVIPPurchased = false; // New variable to track the purchase

    private void Start()
    {
        if(Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        // Load the purchase status from PlayerPrefs
        _isRemoveAdsPurchased = PlayerPrefs.GetInt("RemoveAdsPurchased", 0) == 1;
        // NEW: Load the VIP purchase status
        _isGetVIPPurchased = PlayerPrefs.GetInt("VIPPurchased", 0) == 1;
    }
    private void Update()
    {
        if (!_startTimer) return;
        if (_adReady) return;
        _time -= Time.deltaTime;
        if (_time <= 0)
        {
            _adReady = true;
            _startTimer = false;
        }
    }
    public void StartAdTimer()
    {
        _time = (float)GameInitializer.Instance.adTimer;
        _startTimer = true;
    }

    private void ResetInterTimer()
    {
        _adReady = false;
        _time = (float)GameInitializer.Instance.adTimer;
        _startTimer = true;
       
    }
    public void EndAdTimer()
    {
        _startTimer = false;
    }
    public void ShowTimerAd()
    {
        Debug.Log("Ads, Showing Ad Inter");
        if(!_adReady) return;
        Debug.Log("Ads, Showing  _adReady");
        if (!IsInterstitialAdAvailable()) return;
        _startTimer = false;
        _adReady = false;
        ShowInterstitialAd();
    }
     private void ShowInterstitialAd()
    {
       
            Debug.Log("Ads, showing interstitial ad.");
        if (_isRemoveAdsPurchased)
        {
            Debug.Log("Ads removed, not showing interstitial ad.");
            return;
        }
        
        //if(AppOpenAdCaller.)
        
        if (AdsManager.Instance.IsInterstitialReady())
        {
            AdsManager.Instance.ShowInterstitial();
            ResetInterTimer();
        }
        else
        {
            AdmobManager.Instance.ShowInterstitial();
            ResetInterTimer();
        }
    }
    private bool IsInterstitialAdAvailable()
    {
        return AdsManager.Instance.IsInterstitialReady() || AdmobManager.Instance.IsInterstitialReady();
    }
    public void ShowBanner()
    {
        // Check if ads have been removed before showing
        if (_isRemoveAdsPurchased)
        {
            Debug.Log("Ads removed, not showing banner ad.");
            return;
        }

        AdsManager.Instance.ShowBanner();
    }
    public void HideBanner()
    {
        AdsManager.Instance.HideBanner();
    }
    public void ShowRectBanner()
    {
        if (_isRemoveAdsPurchased)
        {
            Debug.Log("Ads removed, not showing banner ad.");
            return;
        }
        AdsManager.Instance.ShowMREC();
    }
    public void DestroyRectBanner()
    {
       AdsManager.Instance.HideMREC();
    }
    private bool _isRewardedAdCall = false;
    public void ShowRewardedAd(Action onSuccess)
    {
        if (_isRewardedAdCall) return;
        _isRewardedAdCall = true;
        Invoke(nameof(SetRewardedBool),0.5f);
        if (AdsManager.Instance.IsRewardedAdAvailable())
        {
            AdsManager.Instance.ShowRewardedAd(onSuccess);
            ResetInterTimer();
        }
        else if(AdmobManager.Instance.IsRewardedAdReady())
        {
            AdmobManager.Instance.ShowRewardedAd(onSuccess);
            ResetInterTimer();
        }
    }
    private void SetRewardedBool()
    {
        _isRewardedAdCall = false;
    }
    public bool IsRewardedAdAvailable()
    {
        return AdsManager.Instance.IsRewardedAdAvailable() || AdmobManager.Instance.IsRewardedAdReady();
    }
    public void ShowAdUI()
    {
        adsUI.SetActive(IsInterstitialAdAvailable());
    }

    public void RemoveAds()
    {
        _isRemoveAdsPurchased = true;
        PlayerPrefs.SetInt("RemoveAdsPurchased", 1);
        PlayerPrefs.Save();
        HideBanner();
        GameManager.Instance.uiManager.bottomBannerPanel.SetActive(false);
        Debug.Log("Ads have been successfully removed.");
    }
    
    /// <summary>
    /// Sets the status of the VIP purchase and saves it to PlayerPrefs.
    /// </summary>
    public void SetVIPPurchased()
    {
        _isGetVIPPurchased = true;
        PlayerPrefs.SetInt("VIPPurchased", 1);
        PlayerPrefs.Save();
        Debug.Log("VIP purchased status updated and saved.");
    }
}