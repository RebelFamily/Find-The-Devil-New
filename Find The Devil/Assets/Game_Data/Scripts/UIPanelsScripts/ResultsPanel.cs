using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Linq;

public class ResultsPanel : UIPanel
{
    [Header("General Background ----------------------------------------")] 
    [SerializeField] private GameObject backGroundOverlay;
    [SerializeField] private Text resultText;
    [SerializeField] private Text scoreText;
    
    [Header("General Panels ----------------------------------------")] 
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private GameObject liberatedPanel;
    [SerializeField] private Text liberatedText; 
    [SerializeField] private GameObject getToolUnlockPanel;
        
    [Header("General Buttons ----------------------------------------")] 
    [SerializeField] private Button continueButton;
    [SerializeField] private Button continueLiberatedButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject killIconContainer;
    [SerializeField] private GameObject killIcon;
    
    [SerializeField] private ToolsUnlockHandler toolsUnlockHandler;

    [SerializeField] private Button claimToolButton;

    [Header("UI Animations")]
    [SerializeField] private ScaleAnimationHandler winPanelAnimator;
    [SerializeField] private ScaleAnimationHandler failPanelAnimator;
    [SerializeField] private ScaleAnimationHandler liberatedPanelAnimator;
    [SerializeField] private ScaleAnimationHandler getToolUnlockPanelAnimator;
    [SerializeField] private ScaleAnimationHandler continueButtonAnimator;
    [SerializeField] private ScaleAnimationHandler continueLiberatedButtonAnimator;
    [SerializeField] private ScaleAnimationHandler restartButtonAnimator;
    [SerializeField] private ScaleAnimationHandler killIconContainerAnimator;

    private void Awake()
    {
        if (claimToolButton != null)
        {
            claimToolButton.onClick.AddListener(OnClaimToolButtonClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        if (continueLiberatedButton != null)
        {
            continueLiberatedButton.onClick.AddListener(OnContinueLiberatedButtonClicked);
        }
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnContinueRestartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (claimToolButton != null)
        {
            claimToolButton.onClick.RemoveListener(OnClaimToolButtonClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
        if (continueLiberatedButton != null)
        {
            continueLiberatedButton.onClick.RemoveListener(OnContinueLiberatedButtonClicked);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnContinueRestartButtonClicked);
        }
    }

    public void OnContinueButtonClicked()
    {
        //AdsCaller.Instance.ShowTimerAd();
        HideWithCallbacks(LoadLevelMeta);
    }

    private void OnContinueLiberatedButtonClicked()
    {
       // AdsCaller.Instance.ShowTimerAd();
        HideWithCallbacks(LoadLevelLiberated);
    }
    
    public void Show(bool didWin, int finalScore, int enemiesKilled)
    {
        if (getToolUnlockPanel != null) getToolUnlockPanel.SetActive(false);
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
        {
            backGroundOverlay.SetActive(true);
        }
        
        if (didWin)
        {
            if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
            {
                winPanel.SetActive(false);
                failPanel.SetActive(false);
                liberatedPanel.SetActive(true);
                continueLiberatedButton.interactable = true;
                continueLiberatedButton.gameObject.SetActive(true);
                if(continueLiberatedButtonAnimator != null) continueLiberatedButtonAnimator.ScaleUp();
                AnimatePanelIn(liberatedPanelAnimator, () =>
                {
                    int citizensFreed = GameManager.Instance.levelManager._totalFreedNPC;
                    if (liberatedText != null)
                    {
                        liberatedText.text = $"{citizensFreed} / 100 Citizens Freed";
                    }
                    //Ads calling 
                   
                   GameManager.Instance.uiManager.SwitchBlocker(false);
                    
                });
                //AdsCaller.Instance.ShowTimerAd();

                
                if (toolsUnlockHandler != null) toolsUnlockHandler.gameObject.SetActive(false);
            }
            else
            {
                winPanel.SetActive(true);
                failPanel.SetActive(false);
                liberatedPanel.SetActive(false);
                continueButton.interactable = false;
                continueButton.gameObject.SetActive(true);
                if(continueButtonAnimator != null) continueButtonAnimator.ScaleUp();
                
                AnimatePanelIn(winPanelAnimator, () =>
                {
                   
                    if (toolsUnlockHandler != null)
                    {
                        toolsUnlockHandler.gameObject.SetActive(true);
                        Tween fillTween = toolsUnlockHandler.UpdateToolDisplay();
                        
                        if (fillTween != null)
                        {
                            fillTween.OnComplete(() =>
                            {
                                if (toolsUnlockHandler.GetSpawnedDemoTool() != null)
                                {
                                    if (getToolUnlockPanel != null)
                                    {
                                        getToolUnlockPanel.SetActive(true);
                                        winPanel.SetActive(false);
                                        continueButton.interactable = false;
                                        AnimatePanelIn(getToolUnlockPanelAnimator); 
                                        //Ads calling 
                                       // AdsCaller.Instance.ShowTimerAd();

                                    }
                                }
                                else
                                {
                                    continueButton.interactable = true;
                                }
                                
                            });
                        }
                        else
                        {
                            continueButton.interactable = true;
                            //Ads calling 
                            //AdsCaller.Instance.ShowTimerAd();

                        }
                    }
                    else
                    {
                        continueButton.interactable = true;
                    }
                   
                    GameManager.Instance.uiManager.SwitchBlocker(false);
                });
               // AdsCaller.Instance.ShowTimerAd();
                if(killIconContainerAnimator != null) AnimatePanelIn(killIconContainerAnimator);
            }
        }
        else
        {
            winPanel.SetActive(false);
            liberatedPanel.SetActive(false);
            failPanel.SetActive(true);
            
            restartButton.interactable = true;
            restartButton.gameObject.SetActive(true);
            restartButtonAnimator.gameObject.SetActive(true);
            if(restartButtonAnimator != null) restartButtonAnimator.ScaleUp();
            
           // AdsCaller.Instance.ShowRectBanner();
            
            AnimatePanelIn(failPanelAnimator, () =>
            {
                //Ads calling 
                
                Debug.Log("  AnimatePanelIn(failPanelAnimator,");
                GameManager.Instance.uiManager.SwitchBlocker(false);
            });
            
            //AdsCaller.Instance.ShowTimerAd();
            if (toolsUnlockHandler != null)
            {
                toolsUnlockHandler.gameObject.SetActive(false);
            }
        }
        base.Show();  
        
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
        {
            ClearKillIcons();
            StartCoroutine(AnimateKillIcons(enemiesKilled));
        }
        else
        {
            ClearKillIcons();
        }
      
        if ((GameManager.Instance.levelManager._currentLevelNumber + 1) % 6 == 0)
        {
            if(didWin)
                StartCoroutine(ActivateMetaWithDelay(2f));
        }
    }

    public override void Hide()
    {
       // AdsCaller.Instance.DestroyRectBanner();
        HideWithCallbacks();
    }

    public void HideWithCallbacks(Action onHideComplete = null)
    {
        List<ScaleAnimationHandler> animatorsToHide = new List<ScaleAnimationHandler>();

        if (winPanelAnimator != null && winPanelAnimator.gameObject.activeSelf) animatorsToHide.Add(winPanelAnimator);
        if (failPanelAnimator != null && failPanelAnimator.gameObject.activeSelf) animatorsToHide.Add(failPanelAnimator);
        if (liberatedPanelAnimator != null && liberatedPanelAnimator.gameObject.activeSelf) animatorsToHide.Add(liberatedPanelAnimator);
        if (getToolUnlockPanelAnimator != null && getToolUnlockPanelAnimator.gameObject.activeSelf) animatorsToHide.Add(getToolUnlockPanelAnimator);

        if (continueButtonAnimator != null && continueButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(continueButtonAnimator);
        if (continueLiberatedButtonAnimator != null && continueLiberatedButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(continueLiberatedButtonAnimator);
        if (restartButtonAnimator != null && restartButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(restartButtonAnimator);
        if (killIconContainerAnimator != null && killIconContainerAnimator.gameObject.activeSelf) animatorsToHide.Add(killIconContainerAnimator);

        int totalAnimations = animatorsToHide.Count;
        if (totalAnimations == 0)
        {
            backGroundOverlay.SetActive(false);
            base.Hide();
            onHideComplete?.Invoke();
            return;
        }

        int completedAnimations = 0;
        Action onComplete = () =>
        {
            completedAnimations++;
            if (completedAnimations >= totalAnimations)
            {
                backGroundOverlay.SetActive(false);
                base.Hide();
                onHideComplete?.Invoke();
                
            }
        };

        foreach (var animator in animatorsToHide)
        {
            AnimatePanelOut(animator, onComplete);
        }
    }
    
    private IEnumerator AnimateKillIcons(int enemiesKilled)
    {
        killIconContainer.GetComponent<ScaleAnimationHandler>().ScaleUp();
        
        yield return new WaitForSeconds(1f);
    
        if (killIconContainer != null)
        {
            foreach (Transform child in killIconContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
             
        if (killIcon == null)
        {
            Debug.LogWarning("KillIcon prefab is not assigned.");
            yield break;
        }

        for (int i = 0; i < enemiesKilled; i++)
        {
            GameObject newIcon = Instantiate(killIcon, killIconContainer.transform);
            Transform iconChild = newIcon.transform.GetChild(0);
        
            iconChild.gameObject.SetActive(false);

            Sequence iconSequence = DOTween.Sequence();
        
            iconSequence.Append(newIcon.transform.DOScale(1f, .85f).From(0f).SetEase(Ease.OutBack));
        
            iconSequence.OnComplete(() =>
            {
                iconChild.gameObject.SetActive(true);
            
                iconChild.DOScale(1.2f, 0.75f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    iconChild.DOScale(1f, 0.65f);
                });
                iconChild.DOShakeRotation(0.5f, 15f, 10);
            });

            yield return new WaitForSeconds(0.2f);
        }
    }
    

    private void OnClaimToolButtonClicked()
    {
       // AdsCaller.Instance.ShowRewardedAd((() =>
       // {
             if (toolsUnlockHandler == null)
        {
            Debug.LogWarning("ToolsUnlockHandler is not assigned to ResultsPanel.");
            if (getToolUnlockPanel != null) getToolUnlockPanel.SetActive(false);
            continueButton.interactable = true;
            return;
        }

        ShopItemData toolToUnlockData = toolsUnlockHandler.GetLastUnlockedToolData();
        if (toolToUnlockData == null)
        {
            Debug.LogWarning("No tool data available to claim from ToolsUnlockHandler.");
            GameManager.Instance.uiManager.ShowGeneralMessage("No tool to claim!");
            if (getToolUnlockPanel != null) getToolUnlockPanel.SetActive(false);
            toolsUnlockHandler.DestroySpawnedDemoTool();
            continueButton.interactable = true;
            return;
        }

        string toolIdToUnlock = toolToUnlockData.GetItemID();
        CurrencyType requiredCurrency = toolToUnlockData.GetCurrencyType();

        bool claimSuccessful = GameManager.Instance.shopManager.ClaimItem(toolIdToUnlock, requiredCurrency);

        if (claimSuccessful)
        {
            Debug.Log($"Tool {toolIdToUnlock} claimed and unlocked successfully!");
        }
        else
        {
            Debug.LogWarning($"Failed to claim tool {toolIdToUnlock}. Check ShopManager logs for details.");
        }

        if (getToolUnlockPanel != null)
        {
            AnimatePanelOut(getToolUnlockPanelAnimator, () =>
            {
                 toolsUnlockHandler.DestroySpawnedDemoTool();
                 continueButton.interactable = true;
                 if (toolToUnlockData.GetItemType() == ShopItemType.Weapon)
                 {
                     GameObject equippedWeaponPrefab = GameManager.Instance.shopManager.GetEquippedWeaponPrefab();
                     if (equippedWeaponPrefab != null)
                     {
                         GameManager.Instance.playerController.EquipLaserGun(equippedWeaponPrefab);
                     }
                 }
                 else if (toolToUnlockData.GetItemType() == ShopItemType.Scanner)
                 {
                     GameObject equippedScannerPrefab = GameManager.Instance.shopManager.GetEquippedScannerPrefab();
                     if (equippedScannerPrefab != null)
                     {
                         GameManager.Instance.playerController.EquipScanner(equippedScannerPrefab);
                     }
                 }
                 
                 LoadLevelMeta();
            });
        }
        else
        {
            toolsUnlockHandler.DestroySpawnedDemoTool();
            continueButton.interactable = true;

            if (toolToUnlockData.GetItemType() == ShopItemType.Weapon)
            {
                GameObject equippedWeaponPrefab = GameManager.Instance.shopManager.GetEquippedWeaponPrefab();
                if (equippedWeaponPrefab != null)
                {
                    GameManager.Instance.playerController.EquipLaserGun(equippedWeaponPrefab);
                }
            }
            else if (toolToUnlockData.GetItemType() == ShopItemType.Scanner)
            {
                GameObject equippedScannerPrefab = GameManager.Instance.shopManager.GetEquippedScannerPrefab();
                if (equippedScannerPrefab != null)
                {
                    GameManager.Instance.playerController.EquipScanner(equippedScannerPrefab);
                }
            }
            
            LoadLevelMeta();
        }
            
       // }));
       
    }
        
    public void LoadLevelMeta() 
    { 
        if (getToolUnlockPanel != null)
        { 
            getToolUnlockPanel.SetActive(false);
        }
        if (toolsUnlockHandler != null)
        {
            toolsUnlockHandler.DestroySpawnedDemoTool();
        }
        GameManager.Instance.playerController.ResetCameraAngles(0.01f);
        
        killIconContainer.GetComponent<ScaleAnimationHandler>().ScaleDown();

        if (GameManager.Instance.levelManager._currentLevelNumber >= 2 )
        {
            if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
                GameManager.Instance.playerController.ResetCameraAngles(0.01f);
            
            Debug.Log("LoadLevelMeta() ");
            GameManager.Instance.levelManager.UnLoadCurrentlevel();
            GameManager.Instance.uiManager.HidePanel(UIPanelType.ResultsPanel);
            GameManager.Instance.uiManager.ShowMetaPanel(UIPanelType.MetaPanel,MetaPanelMode.MetaFiller);
            GameManager.Instance.metaManager.ActivateLoadedMeta(); 
            GameManager.Instance.metaManager._hasShownAdOptionThisMeta = false; 
            
            RenderSettings.fog = false;
        }
        else
        {
            Debug.Log("LoadLevelMeta() mainmenu calling");
            GameManager.Instance.levelManager.LoadNextLevel();
            GameManager.Instance.playerController.Init();
            GameManager.Instance.uiManager.HidePanel(UIPanelType.ResultsPanel);
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
            RenderSettings.fog = true;
            
        }

    }
    
    public void LoadLevelLiberated() 
    {
        GameManager.Instance.uiManager.LevelLoaderUI(() =>
        {
            GameManager.Instance.levelManager.LoadNextLevel();
            GameManager.Instance.playerController.Init();
            GameManager.Instance.uiManager.UnLoadLoaderUI();
        });
      //  GameManager.Instance.levelManager.LoadNextLevel();
       // GameManager.Instance.playerController.Init();
       
        GameManager.Instance.uiManager.HidePanel(UIPanelType.ResultsPanel);
        GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        GameManager.Instance.metaManager.DeactivateCityMeta();
        RenderSettings.fog = true;
    }
    
    private IEnumerator ActivateMetaWithDelay(float delay)
    {
        Debug.Log("ActivateMetaWithDelay(float delay)");
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
        {
            GameManager.Instance.playerController.StartCityMetaRotation(GameManager.Instance.levelManager._currentLevelNumber);
        }
        
        yield return new WaitForSeconds(delay);
        Debug.Log("ActivateMetaWithDelay(float delay)");
        GameManager.Instance.metaManager.ActivateLoadedMeta();
    } 
   
    public void OnContinueRestartButtonClicked()
    {
       // AdsCaller.Instance.ShowTimerAd();
        HideWithCallbacks(() =>
        {
            GameManager.Instance.levelManager.ReloadCurrentLevel();
            GameManager.Instance.playerController.Init();
            GameManager.Instance.isLevelFailCalled = false; 
           // AdsCaller.Instance.DestroyRectBanner();
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        });
        // AdsCaller.Instance.ShowRewardedAd(() =>
        // {
        //     HideWithCallbacks(() =>
        //     {
        //         Debug.Log("ActivateMetaWithDelay(float delay)");
        //         GameManager.Instance.levelManager.ResetCurrentLevelPhase();
        //         GameManager.Instance.uiManager.HidePanel(UIPanelType.ResultsPanel);
        //         GameManager.Instance.uiManager.ShowPanel(UIPanelType.LevelObjectivesPanel);
        //         //GameManager.Instance.levelManager.ReloadCurrentLevel();
        //         //GameManager.Instance.playerController.Init();
        //         //GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        //     });
        // });
       
    }

    public void OnContinueNoThanksClicked() 
    {
        // HideWithCallbacks(() =>
        // {
        //     GameManager.Instance.levelManager.ReloadCurrentLevel();
        //     GameManager.Instance.playerController.Init();
        //     GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        // });
    }
    
    private void ClearKillIcons()
    {
        if (killIconContainer != null)
        {
            foreach (Transform child in killIconContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    private void AnimatePanelIn(ScaleAnimationHandler panelAnimator, Action onComplete = null)
    {
        if (panelAnimator != null)
        {
            panelAnimator.gameObject.SetActive(true);
            panelAnimator.ScaleUp(onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    private void AnimatePanelOut(ScaleAnimationHandler panelAnimator, Action onComplete = null)
    {
        if (panelAnimator != null)
        {
            panelAnimator.ScaleDown(() =>
            {
                panelAnimator.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
}