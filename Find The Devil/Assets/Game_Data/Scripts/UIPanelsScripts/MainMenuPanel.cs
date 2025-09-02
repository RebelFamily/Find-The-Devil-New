using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Purchasing;

public class MainMenuPanel : UIPanel
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button scannerShopButton;
    [SerializeField] private Button weaponsShopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button getVIPButton;
    [SerializeField] private Button removeAdsButton;

    [SerializeField] private Button basesbutton;
    [SerializeField] private GameObject leftMenu;
    [SerializeField] private GameObject rightMenu;
    [SerializeField] private GameObject levelProgressBar;

    [SerializeField] private LevelProgressBarHandler _levelProgressBarHandler;
    
    
    [Header("UI Animations")] 
    // --- NEW: Animator for the play button ---
    [SerializeField] private ScaleAnimationHandler playButtonAnimator;
    [SerializeField] private ScaleAnimationHandler scannerShopButtonAnimator;
    [SerializeField] private ScaleAnimationHandler weaponsShopButtonAnimator;
    [SerializeField] private ScaleAnimationHandler getVIPButtonAnimator;
    [SerializeField] private ScaleAnimationHandler basesButtonAnimator;
    [SerializeField] private ScaleAnimationHandler levelProgressBarAnimator;
    [SerializeField] private ScaleAnimationHandler removeAdsButtonAnimator;

    private void Awake() 
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (weaponsShopButton != null) weaponsShopButton.onClick.AddListener(OnShopButtonClicked);
        if (scannerShopButton != null) scannerShopButton.onClick.AddListener(OnScannerShopButtonClicked);
        if (basesbutton != null) basesbutton.onClick.AddListener(OnBasesButtonClicked);
        
        if (getVIPButton != null) getVIPButton.onClick.AddListener(OnGetVIPButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        // NEW: Add listener for the remove ads button
        if (removeAdsButton != null) removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
    }

    private void OnDestroy()
    {
       if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
       if (weaponsShopButton != null) weaponsShopButton.onClick.RemoveListener(OnShopButtonClicked);
       if (scannerShopButton != null) scannerShopButton.onClick.RemoveListener(OnScannerShopButtonClicked);
       if (basesbutton != null) basesbutton.onClick.RemoveListener(OnBasesButtonClicked);

       if (getVIPButton != null) getVIPButton.onClick.RemoveListener(OnGetVIPButtonClicked);
       if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
       // NEW: Remove listener for the remove ads button
       if (removeAdsButton != null) removeAdsButton.onClick.RemoveListener(OnRemoveAdsClicked);
    }
    
    private void OnPlayButtonClicked() 
    { 
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        
        HideWithCallbacks(() =>
        {
            GameManager.Instance.StartGame();
            GameManager.Instance.UnlockClicks();
        });
    }

    public override void Show()  
    {
        base.Show();
        if (GameManager.Instance.levelManager.GlobalLevelNumber == 0)
        {
            Debug.Log("GlobalLevelNumber == 0");
            gameObject.SetActive(true);
            playButton.gameObject.SetActive(true);
            leftMenu.SetActive(false);
            rightMenu.SetActive(false);
            levelProgressBar.SetActive(false);
        }
        else
        {
            Debug.Log("GlobalLevelNumber != 0");
            gameObject.SetActive(true);
            playButton.gameObject.SetActive(true);
            leftMenu.SetActive(true);
            rightMenu.SetActive(true);
            levelProgressBar.SetActive(true);
        }
        
        
        if (playButtonAnimator != null) playButtonAnimator.ScaleUp();
        if (scannerShopButtonAnimator != null) scannerShopButtonAnimator.ScaleUp();
        if (weaponsShopButtonAnimator != null) weaponsShopButtonAnimator.ScaleUp();
        if (getVIPButtonAnimator != null && !AdsCaller.Instance._isGetVIPPurchased) getVIPButtonAnimator.ScaleUp();
        if (basesButtonAnimator != null) basesButtonAnimator.ScaleUp();
        if (levelProgressBarAnimator != null) levelProgressBarAnimator.ScaleUp(() =>
        {
            _levelProgressBarHandler.UpdateProgressBar();});
        // NEW: Animate the remove ads button
        if (removeAdsButtonAnimator != null && !AdsCaller.Instance._isRemoveAdsPurchased) removeAdsButtonAnimator.ScaleUp();
    }
    
    public override void Hide()
    {
        HideWithCallbacks();
    }
    
    public void HideWithCallbacks(Action onHideComplete = null)
    {
        List<ScaleAnimationHandler> animatorsToAnimate = new List<ScaleAnimationHandler>();

        if (playButtonAnimator != null) animatorsToAnimate.Add(playButtonAnimator);
        if (scannerShopButtonAnimator != null) animatorsToAnimate.Add(scannerShopButtonAnimator);
        if (weaponsShopButtonAnimator != null) animatorsToAnimate.Add(weaponsShopButtonAnimator);
        if (getVIPButtonAnimator != null) animatorsToAnimate.Add(getVIPButtonAnimator);
        if (basesButtonAnimator != null) animatorsToAnimate.Add(basesButtonAnimator);
        if (levelProgressBarAnimator != null) animatorsToAnimate.Add(levelProgressBarAnimator);
        // NEW: Add the remove ads button animator to the list
        if (removeAdsButtonAnimator != null) animatorsToAnimate.Add(removeAdsButtonAnimator);
        
        if (animatorsToAnimate.Count > 0)
        {
            int completedAnimations = 0;
            
            Action onCompleteAction = () =>
            {
                completedAnimations++;
                if (completedAnimations >= animatorsToAnimate.Count)
                {
                    base.Hide();
                    onHideComplete?.Invoke();
                }
            };

            foreach (var animator in animatorsToAnimate)
            {
                animator.ScaleDown(onCompleteAction);
            }
        }
        else
        {
            base.Hide();
            onHideComplete?.Invoke();
        }
    }

    private void OnShopButtonClicked() 
    {
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        
        HideWithCallbacks(() =>
        {
            
            GameManager.Instance.levelManager.HideObjectInLevel(false);
            GameManager.Instance.uiManager.ShowShopPanel(ShopMode.Weapons);
            GameManager.Instance.UnlockClicks();
        });
    } 
    
    private void OnScannerShopButtonClicked() 
    {
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();

        HideWithCallbacks(() =>
        {

            GameManager.Instance.levelManager.HideObjectInLevel(false);
            GameManager.Instance.uiManager.ShowShopPanel(ShopMode.Scanners);
            GameManager.Instance.UnlockClicks();
        });
    }
    
    private void OnGetVIPButtonClicked()
    {
       // IAPManager.Instance.InAppCaller(PurchaseType.);

        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        if (!AdsCaller.Instance._isRemoveAdsPurchased)
        {
            GameManager.Instance.uiManager.HidePanel(UIPanelType.MainMenuPanel);
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.GetVIPPanel);
            
        }
        else
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("This feature have been already bought.");
        }
        
        GameManager.Instance.UnlockClicks();
    }
    
    private void OnSettingsButtonClicked() 
    {
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        
        GameManager.Instance.uiManager.ShowGeneralMessage("This feature is currently being assembled by a team of highly caffeinated squirrels.");
        
        GameManager.Instance.UnlockClicks();
    }

    public void OnBasesButtonClicked()
    {
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        
        HideWithCallbacks(() =>
        {
            GameManager.Instance.uiManager.ShowMetaPanel(UIPanelType.MetaPanel, MetaPanelMode.BasesShow);
            GameManager.Instance.metaManager.ShowcaseMeta(0);
            GameManager.Instance.levelManager.UnLoadCurrentlevel();
            RenderSettings.fog = false;
            GameManager.Instance.UnlockClicks();
        });
    }
    
    public void OnRemoveAdsClicked()
    {
        if (GameManager.Instance.IsClickLocked()) return;
        GameManager.Instance.LockClicks();
        
        if(!AdsCaller.Instance._isRemoveAdsPurchased)
            IAPManager.Instance.InAppCaller(PurchaseType.RemoveAds);
        else
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("Remove Ads Have been purchased already.");
        }
        
        GameManager.Instance.UnlockClicks();
    }
    
}