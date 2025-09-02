using System.Collections;
using GameAnalyticsSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 
    
    [Header("Managers References")]
    public LevelManager levelManager;
    public UIManager uiManager;
    public EconomyManager economyManager;
    public ProgressionManager progressionManager;
    public SaveLoadManager saveLoadManager;
    public AudioManager audioManager;
    public RemoteConfigService remoteConfigService;
    public ShopManager shopManager;
    public MetaManager metaManager;
    
    private bool _isClickLocked = false;
    public bool _waitForTryGun = false;
    public bool _isReachedPoint = false;
    
    private WaitForSecondsRealtime _initDelay = new WaitForSecondsRealtime(.1f);
    [Header("Controllers")]
    public PlayerController playerController;

    public bool isLevelFailCalled = false;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
        //InitializeManagers();
        StartCoroutine(InitializeManagers());
        
    }

    // private void InitializeManagers()
    // {
    //     
    //     Vibration.Init();
    //     saveLoadManager.Init();
    //     remoteConfigService.Init();
    //     audioManager.Init();
    //     economyManager.Init();
    //     progressionManager.Init();
    //     levelManager.Init();
    //     uiManager.Init();
    //     shopManager.Init();
    //     playerController.Init();
    //     metaManager.Init();
    // } 
    private IEnumerator InitializeManagers()
    {
        
        //Ads && Analytics Calling
        AdsCaller.Instance.StartAdTimer();
        
        Vibration.Init();
        yield return _initDelay;
        saveLoadManager.Init();
        yield return _initDelay;
        remoteConfigService.Init();
        yield return _initDelay;
        audioManager.Init();
        yield return _initDelay;
        economyManager.Init();
        yield return _initDelay;
        progressionManager.Init("WPN_1","SCN_1");
        yield return _initDelay;
        levelManager.Init();
        yield return _initDelay;
        uiManager.Init();
        yield return _initDelay;
        shopManager.Init();
        yield return _initDelay;
        playerController.Init();
        yield return _initDelay;
        metaManager.Init();
        yield return _initDelay;
        uiManager.UnLoadLoaderUI();
        playerController.ResetSpline();       
        yield return _initDelay;
        uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        
        AdsCaller.Instance.ShowBanner();

        
    }

    public void StartGame()
    {
        //Ads && Analytics Calling
        levelManager.isLevelFail = false;
        audioManager.PlaySFX(AudioManager.GameSound.ButtonClick_Normal);
        
        Debug.Log("levelManager._currentLevelNumber == "+levelManager._currentLevelNumber +" && levelManager.GlobalLevelNumber == " 
                  +levelManager.GlobalLevelNumber+ " levelManager.-total repetation" + levelManager._totalLevelsRepeat);
        
       
        if (levelManager._currentLevelNumber == 0 && levelManager.GlobalLevelNumber == 0)
        {
            
            StartCoroutine( playerController.CheckForTutorialLevel(true));
        }
        else
        {
           
           StartCoroutine( playerController.CheckForTutorialLevel(false));
        }
    }

    public void LevelFail()
    {
        if (!isLevelFailCalled)
        {
            isLevelFailCalled = true;
            
            // uiManager.HideAllPanels();
            AdsCaller.Instance.ShowRectBanner();
            
           // AdsCaller.Instance.ShowTimerAd();

            AnalyticsManager.Instance.ProgressionEventSingleMode(GAProgressionStatus.Fail,
                (levelManager.GlobalLevelNumber + 1).ToString());

            if (levelManager._currentLevelNumber == 0 && levelManager.GlobalLevelNumber == 0)
            {
                uiManager.HidePanel(UIPanelType.TutorialPanel);
            }

            uiManager.HidePanel(UIPanelType.GameOverlayPanel);
            uiManager.HidePanel(UIPanelType.LevelObjectivesPanel);
            uiManager.ShowResultsPanel(false, 0, 0);
            playerController.ResetTools();
            saveLoadManager.SaveGameData();

            Instance.audioManager.PlaySFX(AudioManager.GameSound.Music_Defeat);
        }
    }

    public void OnLevelCompleted()
    {
       
       // AdsCaller.Instance.ShowTimerAd();

        AnalyticsManager.Instance.ProgressionEventSingleMode(GAProgressionStatus.Complete, (levelManager.GlobalLevelNumber+1).ToString());
        
        progressionManager.UnlockNextLevel();
        playerController.ResetTools();
        uiManager.HidePanel(UIPanelType.GameOverlayPanel);
        uiManager.HidePanel(UIPanelType.LevelObjectivesPanel);
       // uiManager.HideAllPanels();
        uiManager.ShowResultsPanel(true,10,levelManager.CurrentLevel.GetNumberOfEnemy());
        
        Instance.audioManager.PlaySFX(AudioManager.GameSound.Music_Victory);
    }

    public void LoadNewScene()
    {
        SceneManager.LoadScene("GamePlay");
    }
    
    public void OnGameOver()
    {
       
    }
    public void LockClicks()
    {
        _isClickLocked = true;
    }

    public void UnlockClicks()
    {
        StartCoroutine(UnlockClicksAfterDelay(0.3f));
    }

    private IEnumerator UnlockClicksAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _isClickLocked = false;
    }

    public bool IsClickLocked()
    {
        return _isClickLocked;
    }
    private void OnDisable()
    {
        //DOTween.KillAll();
    }
    
    public void RemoveAds(PurchaseType type)
    {
        if (PurchaseType.RemoveAds == type)
        {
            AdsCaller.Instance.RemoveAds();
        }
    }
    
}