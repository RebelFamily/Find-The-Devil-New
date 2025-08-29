using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//using GameAnalyticsSDK;

public enum LevelType { Linear, Meta, Bonus, Rescue }

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<LevelData> allLevelsDataAssets; 

    private List<ILevelData> _allLevels;
    private ILevelData _currentLevel;
    public int _currentLevelNumber = -1; 

    public GameObject _currentLevelObj;
    public int _currentEnemyNumber;
    public int _currentCapturedNPC = 0;
    public int _totalFreedNPC;
    public ILevelData CurrentLevel => _currentLevel;

    public int totalLevels = 0;
    public int levelCoinsTOGet = 0;
    
    // --- New: Variable to track how many times all levels have been repeated ---
    public int _totalLevelsRepeat = 0;
    
    // --- New: Public property to get the true global level number ---
    public int GlobalLevelNumber => _currentLevelNumber + (_totalLevelsRepeat * totalLevels);

    [Header("Editor Testing")]
    [SerializeField] private bool _editorTestMode = false;
    [SerializeField] private int _testLevelIndex = 0;

    public void Init()
    {
        _allLevels = new List<ILevelData>();
        foreach (var levelAsset in allLevelsDataAssets)
        {
            if (levelAsset != null)
            {
                _allLevels.Add(levelAsset);
            }
        }

        totalLevels = _allLevels.Count;

        // --- NEW: Load the saved total freed NPC count at initialization ---
        _totalFreedNPC = GameManager.Instance.progressionManager.LoadTotalFreedNPC();
//        Debug.Log($"Loaded Total Freed NPC: {_totalFreedNPC}");
        
        // --- NEW: Load the saved total levels repeat count ---
        _totalLevelsRepeat = GameManager.Instance.progressionManager.LoadTotalLevelsRepeat();
       // Debug.Log($"Loaded Total Level Repeats: {_totalLevelsRepeat}");

        if (_editorTestMode)
        {
            _testLevelIndex = Mathf.Clamp(_testLevelIndex, 0, _allLevels.Count - 1);
            LoadLevel(_testLevelIndex);
            if (_testLevelIndex == 0)
            {
                StartCoroutine( GameManager.Instance.playerController.CheckForTutorialLevel(true));  
            }
            else
            {
                StartCoroutine( GameManager.Instance.playerController.CheckForTutorialLevel(false));
            }
        }
        else
        {
            LoadLevel(GameManager.Instance.progressionManager.GetUnlockedLevel());
        }
    }

    public void LoadLevel(int levelNumberToLoad)
    {

        GameManager.Instance._isReachedPoint = false;
        
        if (levelNumberToLoad < 0 || levelNumberToLoad >= _allLevels.Count)
        {
            LoadLevel(0);
            return;
        }
        
        if (_currentLevelObj != null)
        {
            Destroy(_currentLevelObj);
            _currentLevelObj = null; 
        }
        _currentLevelNumber = levelNumberToLoad;
        _currentLevel = _allLevels[_currentLevelNumber];
        levelCoinsTOGet = _currentLevel.GetLevelRewardCoins();
        GameObject levelPrefabToLoad = _currentLevel.GetLevelPrefab();
        if (levelPrefabToLoad != null)
        {
            _currentLevelObj = Instantiate(levelPrefabToLoad);
        }
        else
        {
            return; 
        }
 
        LevelPhaseManager currentLevelPhaseManager = _currentLevelObj.GetComponent<LevelPhaseManager>();
        if (currentLevelPhaseManager != null)
        {
            currentLevelPhaseManager.Init();
            currentLevelPhaseManager.StartLevel();
        }
       
        _currentEnemyNumber = _currentLevel.GetNumberOfEnemy();
        _currentCapturedNPC = currentLevelPhaseManager.GetCapturedNPCCount();
        
        
        if(_currentLevel.GetLevelType() != LevelType.Rescue)
            GameManager.Instance.uiManager.UpdateLevelObjects(0,_currentLevel.GetNumberOfEnemy());
        else
        {
            
        }
        if (GameManager.Instance != null && GameManager.Instance.audioManager != null && currentLevelPhaseManager != null)
        {
            if (CurrentLevel.GetLevelType() == LevelType.Rescue && _currentLevelNumber != 17 && _currentLevelNumber != 23)
            {
                GameManager.Instance.audioManager.PlayMusic(currentLevelPhaseManager.levelBGAudio,3.5f, true);
            }
            else
            {
                GameManager.Instance.audioManager.PlayMusic(currentLevelPhaseManager.levelBGAudio, true);
            }
            GameManager.Instance.audioManager.PlaySFX(currentLevelPhaseManager.levelBasedSFXSound,2.1f);
            
            
        }
        
    }
    
    
    public void LoadNextLevel()
    {
        // banner call
       // AdsCaller.Instance.ShowBanner();

        int nextLevelIndex = GameManager.Instance.progressionManager.GetUnlockedLevel();
        if (nextLevelIndex < _allLevels.Count)
        {
            LoadLevel(nextLevelIndex);
          
            
        } 
        else
        {
            _totalLevelsRepeat++;
            Debug.Log("_currentLevelNumber == "+_currentLevelNumber +" && GlobalLevelNumber == " 
                      +GlobalLevelNumber+ " -total repetation" + _totalLevelsRepeat);
           

            LoadLevel(0);
            StartCoroutine( GameManager.Instance.playerController.CheckForTutorialLevel(true));
        }
    }
    
    public void ReloadCurrentLevel()
    {
       // GameManager.Instance.playerController.ResetSpline();  
        if (_currentLevelNumber != -1 && _currentLevelNumber < _allLevels.Count)
        {
            LoadLevel(_currentLevelNumber); 
            // if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            // {
            //     GameManager.Instance.playerController.ResetTools();
            //    // GameManager.Instance.uiManager.HideAllPanels();
            //     GameManager.Instance.uiManager.ShowPanel(UIPanelType.GameOverlayPanel);
            // }
            //
            // if (_currentLevelNumber == 0)
            // {
            //     StartCoroutine( GameManager.Instance.playerController.CheckForTutorialLevel(true));
            // }
            // else
            // {
            //     StartCoroutine( GameManager.Instance.playerController.CheckForTutorialLevel(false));
            // }
        }
    }

    public void UpdateEnemyCount()
    {
        _currentEnemyNumber -= 1;
        GameManager.Instance.uiManager.UpdateLevelObjects((_currentLevel.GetNumberOfEnemy() - _currentEnemyNumber) ,_currentLevel.GetNumberOfEnemy());
    }

    public void UpdateCapturedNPC()
    {
        _currentCapturedNPC--;
        
    } 
    public void UpdateCapturedNPCSkipped()
    {
        _currentCapturedNPC = 0;
        
    }
    
    public void CheckLevelCompletionCount() 
    {
        Debug.Log("_releasedCount = " + _currentEnemyNumber + " === " + _currentCapturedNPC);
        if (_currentEnemyNumber <= 0 && _currentCapturedNPC <= 0)  
        {
            LevelPhaseManager currentLevelPhaseManager = _currentLevelObj.GetComponent<LevelPhaseManager>();

            
            if (currentLevelPhaseManager != null)
            {
                GameManager.Instance.progressionManager.SaveTotalFreedNPC(_totalFreedNPC);
                Debug.Log($"Level Completed. New Total Freed NPC: {_totalFreedNPC}");
            }
            
            if(CurrentLevel.GetLevelType() != LevelType.Rescue)
             GameManager.Instance.playerController.ResetCameraAngles(0.5f);
            
            //GameManager.Instance.economyManager.AddCoins(_currentLevel.GetLevelRewardCoins());
            
            GameManager.Instance.OnLevelCompleted();
        }
    }

    public void UnLoadCurrentlevel()
    {
        _currentLevelObj.SetActive(false);   
    }

    // fix this 
    public void HideObjectInLevel(bool value)
    {
        if (_currentLevelObj.GetComponent<LevelPhaseManager>().objectToHide != null)
        {
            Debug.Log("HideObjectInLevel(bool value)" + value);
            _currentLevelObj.GetComponent<LevelPhaseManager>().objectToHide.SetActive(value);
            
        }
        
    }
    
    public void ResetCurrentLevelPhase()
    {
        if (_currentLevelObj == null)
        {
            return;
        }

        Debug.Log("LevelManager: ResetCurrentLevelPhase.");
        
        LevelPhaseManager currentLevelPhaseManager = _currentLevelObj.GetComponent<LevelPhaseManager>();
        if (currentLevelPhaseManager != null)
        {
            Debug.Log("currentLevelPhaseManager");
            currentLevelPhaseManager.ResetActiveUnits();
        }
        
    }
    
    /*
    private void SendProgressionEvent(GAProgressionStatus status, string levelDetail)
    {
        AnalyticsManager.Instance.ProgressionEventSingleMode(status,levelDetail);
    }
    */
    
    
    
}