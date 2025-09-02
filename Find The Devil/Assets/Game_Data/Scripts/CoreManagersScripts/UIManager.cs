using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; 
using System.Collections; 
using System.Linq; // Required for LINQ extension methods
using System; // Required for System.Action
using DG.Tweening; // Required for DOTween

public enum UIPanelType
{
    MainMenuPanel, ShopPanel, GameOverlayPanel, ResultsPanel, SettingsPanel, GetVIPPanel, TutorialPanel, LevelObjectivesPanel, MetaPanel
    
}

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<UIPanel> allPanels; 
    private Dictionary<UIPanelType, IUIPanel> panelDictionary = new Dictionary<UIPanelType, IUIPanel>();
    
    // Tracks panels that are currently active. Using a HashSet for efficient add/remove/check operations.
    private HashSet<IUIPanel> _activePanels = new HashSet<IUIPanel>();

    public static UIManager Instance { get; private set; } 

    [SerializeField] public GameObject switchBlocker;
    [SerializeField] public GameObject loaderUI;
    [SerializeField] public Image LoaderUIImage;
    [SerializeField] public List<Image> LoaderIconImage;
    
    
    
    [Header("General Message System")]
    [SerializeField] private GameObject generalMessagePanel;
    public GameObject bottomBannerPanel;
    [SerializeField] private Text generalMessageText;
    [SerializeField] private float messageDuration = 3.0f;
    public Transform coinsTargetPos;
    private Coroutine _messageCoroutine;

    public GameObject spearEffect;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        foreach (var panel in allPanels)
        {
            if (panel != null) 
            {
                panelDictionary[panel.GetPanelType()] = panel;
                panel.Hide(); 
            }
        }
    
        if (generalMessagePanel != null)
        {
            generalMessagePanel.SetActive(false);
        }
    }
    
    public void ShowPanel(UIPanelType panelType)
    {
        if (AdsCaller.Instance._isRemoveAdsPurchased)
        {
            bottomBannerPanel.SetActive(false);
        }
        
        if (panelDictionary.TryGetValue(panelType, out IUIPanel panel))
        {
            // First, hide any panels that are currently active
           // HideAllActivePanels();
            
            // Now, show the new panel and add it to our active list
            panel.Show();
            _activePanels.Add(panel);
            //panel.UpdatePanel(); 
        } 
    }

    public void UpdateTutorialPanel(UIPanelType panelType)
    {
        if (panelDictionary.TryGetValue(panelType, out IUIPanel panel))
        {
            panel.UpdatePanel();
        } 
    }
     public void ShowTapTOZapPanel(bool value)
    {
        if ((panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel tPanel))
        {
            tPanel.ShowTapToZap(value);  
           
        }  
    }
     public void ShowScannerPanel()
    {
        if ((panelDictionary.TryGetValue(UIPanelType.TutorialPanel, out IUIPanel panel) && panel is TutorialPanel tPanel))
        {
          tPanel.ShowSwipeToScan();  
        } 
    }
    
    public void ShowTutorialPanel(UIPanelType panelType)
    {
        if (panelDictionary.TryGetValue(panelType, out IUIPanel panel))
        {
           // panel.UpdatePanel();
            panel.Show();
            _activePanels.Add(panel);
        } 
    } 
    
    public void ShowMetaPanel(UIPanelType panelType, MetaPanelMode metaPanelMode)
    {
        if (panelDictionary.TryGetValue(UIPanelType.MetaPanel, out IUIPanel panel) && panel is MetaPanel metaPanel)
        {
            panel.Show();
            _activePanels.Add(panel);
            metaPanel.SetPanelMode(metaPanelMode);
        }
    }
    
    public void ShowResultsPanel(bool didWin, int finalScore, int enemiesKilled)
    {
        if (panelDictionary.TryGetValue(UIPanelType.ResultsPanel, out IUIPanel panel))
        {
            if (panel is ResultsPanel resultsPanel) 
            {
               // HideAllActivePanels();
                
                resultsPanel.Show(didWin, finalScore, enemiesKilled);
                _activePanels.Add(resultsPanel);
                
               // SwitchBlocker(false);
            }
            else
            {
                Debug.LogError("UIManager: ResultsPanel retrieved from dictionary is not of type ResultsPanel!");
            }
        }
        else
        {
            Debug.LogError("UIManager: ResultsPanel not found in dictionary!");
        }
    }
    
    public void ShowShopPanel(ShopMode shopMode)
    {
        if (panelDictionary.TryGetValue(UIPanelType.ShopPanel, out IUIPanel panel))
        {
            if (panel is ShopPanel shopPanel) 
            {
                StartCoroutine(shopPanel.SetShopMode(shopMode));
                panel.Show();
                _activePanels.Add(shopPanel);
            }
            else
            {
                Debug.LogError("UIManager: ResultsPanel retrieved from dictionary is not of type ResultsPanel!");
            }
        }
        else
        {
            Debug.LogError("UIManager: ResultsPanel not found in dictionary!");
        }
    }

    public void HidePanel(UIPanelType panelType)
    {
        if (panelDictionary.TryGetValue(panelType, out IUIPanel panel))
        {
            if (_activePanels.Contains(panel))
            {
                panel.Hide();
                _activePanels.Remove(panel);
            }
        }
    } 
    
    public void HideAllPanels()
    {
        HideAllActivePanels();
    }
    
    private void HideAllActivePanels()
    {
        // Convert to a list to avoid issues with modifying the collection while iterating
        var panelsToHide = _activePanels.ToList();
        
        foreach (var p in panelsToHide)
        {
            p.Hide();
        }
        _activePanels.Clear();
    }

    public void UpdateHUD(float score, float health, int coins)
    {
        if (panelDictionary.TryGetValue(UIPanelType.GameOverlayPanel, out IUIPanel panel) && panel is GameOverlayPanel gameOverlay)
        {
           // gameOverlay.UpdateHUD(score, health, coins, gems);
        }
    }
    
    public void SwitchBlocker(bool sectActive)
    {
        Debug.Log("SwitchBlocker(bool sectActive) = " + sectActive);
        switchBlocker.SetActive(sectActive);
    }

    public void SwitchWaitForTryGun(bool value)
    {
        GameManager.Instance._waitForTryGun = value;
        
    }

    public void UpdateMetaPanel()
    {
        if (panelDictionary.TryGetValue(UIPanelType.MetaPanel, out IUIPanel panel) && panel is MetaPanel metaPanel)
        {
            if (_activePanels.Contains(metaPanel))
            {
                metaPanel.UpdatePanel();
            }
        }
    }
    
    public void UpdateLevelObjects(int currentKilledEnemies, int totalEnemiesInLevel)
    {
      if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel levelObjectivesPanel)
      {
          if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
            levelObjectivesPanel.UpdateEnemiesDisplay(currentKilledEnemies,totalEnemiesInLevel);
      }
    } 
    
    public void ActivateVIPGun()
    {
      if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel levelObjectivesPanel)
      {
         
            levelObjectivesPanel.ActivateVIPGunFeature();
         
      }
    }

    public void PlayKingDevilDialogues()
    {
        if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel levelObjectivesPanel)
        {
            levelObjectivesPanel.StartKingDevilCutscene();
        }
    }
    public void NPCRescuedCount(int value)
    {
        if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel levelObjectivesPanel)
        {
            levelObjectivesPanel.StartRescuedNPCCutscene(value);
        }
    }
    
    public void AddCoinsUI(int value)
    {
        if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) && panel is LevelObjectivesPanel levelObjectivesPanel)
        {
            levelObjectivesPanel.AddCoinsToUI(value);
        }
    }
   
    public void UpdateMetaCoins()
    {
        if (panelDictionary.TryGetValue(UIPanelType.MetaPanel, out IUIPanel panel) && panel is MetaPanel metaPanel)
        {
            if (_activePanels.Contains(metaPanel))
            {
                metaPanel.UpdateCoinText();
            }
        }
    }
    
    public void ShowAdCoinsOption()
    {
        if (panelDictionary.TryGetValue(UIPanelType.MetaPanel, out IUIPanel panel) && panel is MetaPanel metaPanel)
        {
            if (_activePanels.Contains(metaPanel))
            {
                metaPanel.ShowAdCoinsOption();
            }
        }
    }
    
    public void HideAdCoinsOption()
    {
        if (panelDictionary.TryGetValue(UIPanelType.MetaPanel, out IUIPanel panel) && panel is MetaPanel metaPanel)
        {
            if (_activePanels.Contains(metaPanel))
            {
                metaPanel.HideAdCoinsOption();
            }
        }
    }
    
    public void ShowGeneralMessage(string message)
    {
        if (generalMessagePanel == null || generalMessageText == null)
        {
            Debug.LogError("General message panel or text not assigned in UIManager!");
            return;
        }
        
        if (_messageCoroutine != null)
        {
            StopCoroutine(_messageCoroutine);
        }

        generalMessageText.text = message;
        generalMessagePanel.SetActive(true);

        _messageCoroutine = StartCoroutine(HideMessageAfterDelay(messageDuration));
    }
    
    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        generalMessagePanel.SetActive(false);
        _messageCoroutine = null;
    }

    public bool GetVIPGunAnimationCheck()
    {
        if (panelDictionary.TryGetValue(UIPanelType.LevelObjectivesPanel, out IUIPanel panel) &&
            panel is LevelObjectivesPanel levelObjectivesPanel)
        {
            if (_activePanels.Contains(levelObjectivesPanel))
            {
                return levelObjectivesPanel.isVIPGunAnimationss;
            }
        }
        return false;
    }

    public void UnlockAllItems(PurchaseType type)
    {
        if (panelDictionary.TryGetValue(UIPanelType.ShopPanel, out IUIPanel panel))
        {
            if (panel is ShopPanel shopPanel) 
            {
               shopPanel.UnlockAllItems(type);
            }
        }
    }

    public void LevelLoaderUI(Action onComplete)
    {
        if (loaderUI == null || LoaderUIImage == null || LoaderIconImage == null || LoaderIconImage.Count == 0)
        {
            Debug.LogError("Loader UI components not assigned in UIManager!");
            onComplete?.Invoke();
            return;
        }

        loaderUI.SetActive(true);
       
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.LevelCompleteLaugh);
        Color initialColor = LoaderUIImage.color;
        initialColor.a = 0f;
        LoaderUIImage.color = initialColor;
        loaderUI.transform.localScale = Vector3.one; 

        foreach (var icon in LoaderIconImage)
        {
            icon.transform.localScale = Vector3.zero;
        }

        Sequence loadSequence = DOTween.Sequence();
        float fadeDuration = 0.15f;
        float iconScaleDuration = 0.1f;
        float finalScaleDuration = 0.2f;
        float totalAnimationTime = (LoaderIconImage.Count * iconScaleDuration) + fadeDuration + finalScaleDuration;
        
        loadSequence.Append(LoaderUIImage.DOFade(1f, fadeDuration));

        for (int i = 0; i < LoaderIconImage.Count; i++)
        {
            loadSequence.Append(LoaderIconImage[i].transform.DOScale(1f, iconScaleDuration).SetEase(Ease.OutBack));
        }
        
        loadSequence.AppendInterval(0.1f);
        
        loadSequence.OnComplete(() =>
        {
            loaderUI.SetActive(false);
            onComplete?.Invoke();
        });

        loadSequence.Play();
    }

    public void UnLoadLoaderUI()
    {
        if (loaderUI == null || LoaderUIImage == null || LoaderIconImage == null || LoaderIconImage.Count == 0)
        {
            Debug.LogError("Loader UI components not assigned in UIManager!");
            return;
        }
        
     
        Sequence unloadSequence = DOTween.Sequence();
        float fadeDuration = 0.12f;
        float iconScaleDuration = 0.12f;

        for (int i = 0; i < LoaderIconImage.Count; i++)
        {
            unloadSequence.Append(LoaderIconImage[i].transform.DOScale(0f, iconScaleDuration).SetEase(Ease.InBack));
        }
        
        unloadSequence.Append(LoaderUIImage.DOFade(0f, fadeDuration));
        
        unloadSequence.OnComplete(() =>
        {
            loaderUI.SetActive(false);
            
        });

        unloadSequence.Play();
    }

    public void HideGetVIPPanel()
    {
        if (panelDictionary.TryGetValue(UIPanelType.GetVIPPanel, out IUIPanel panel) &&
            panel is GetVIPPanel getVipPanel)
        {
            if (_activePanels.Contains(getVipPanel))
            {
                getVipPanel.HideThisPanel();
            }
        }
      
    }
    
    
}