using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;
using GameAnalyticsSDK;
using System;
using UnityEngine.UI;


public class MetaManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private CityMetaManager _cityMetaManager;
    // --- NEW: Reference to the separate showcase manager ---
    [SerializeField] private MetaShowcaseManager _metaShowcaseManager;
   
    [Header("Level Meta (Building) Settings")]
    [SerializeField] private List<MetaData> allMetaDataList = new List<MetaData>();
    [SerializeField] private Transform metaPrefabSpawnParent;
    [SerializeField] public CoinAnimator _coinAnimator;
    
    [SerializeField] private float _levelLoadDelay = 2.0f;
    [SerializeField] public float _fadeDuration = 0.5f;

    private GameObject _currentMetaObj;
    public MetaController _currentMetaController;
    public MetaData _currentLoadedMeta;

    [Header("Get 200 Coins Chest Settings")]
    public GameObject get200CoinsObj;
    public GameObject coinsChestObj;
    public Transform particleEffectPos;
    public CoinAnimator chestCoinAnimator;
  
    public Transform targetPos;
    public Transform startPos;
    public GameObject chestOpenParticlesPrefab;
    
    public string currentMetaName;
    public int currentMetaNumber { get; private set; } = -1;
    public int totalMetaItems { get; private set; } = 0;
    
    private bool _isHandlingTransition = false;
    private bool _isMetaAnimating = false;
    private bool _isShowingAdOption = false;

    public bool _hasShownAdOptionThisMeta = false;
    public MetaController CurrentMetaController => _currentMetaController;

    [SerializeField] private bool _editorTestMode = false;
    [SerializeField] private int _testMetaIndex = 0;
    
    public int CurrentCityMetaIndex => _cityMetaManager.CurrentCityMetaIndex;
    
    private float _metaOriginalLocalX = 0f;
    private Vector3 _get200CoinsOriginalPosition;
    
    private int _metaCompletionCount = 0;
    public int MetaCompletionCount => _metaCompletionCount;

    private void Awake()
    {
        if (get200CoinsObj != null)
        {
            _get200CoinsOriginalPosition = get200CoinsObj.transform.localPosition;
        }
    }

    public void Init()
    {
        totalMetaItems = allMetaDataList.Count;

        if (totalMetaItems == 0)
        {
            return;
        }

        if (_editorTestMode)
        {
            _testMetaIndex = Mathf.Clamp(_testMetaIndex, 0, totalMetaItems - 1);
            LoadMeta(_testMetaIndex);
        }
        else
        {
            int metaToLoadIndex = 0;
            string lastMetaID = GameManager.Instance.progressionManager.LoadCurrentMetaID();

            if (!string.IsNullOrEmpty(lastMetaID))
            {
                int savedMetaIndex = allMetaDataList.FindIndex(meta => meta.metaID == lastMetaID);
                if (savedMetaIndex != -1)
                {
                    metaToLoadIndex = savedMetaIndex;
                }
            }
            
            LoadMeta(metaToLoadIndex);
        }
        
        _cityMetaManager.LoadCityMetaStates();
    }
    
    public void LoadMeta(int metaNumberToLoad)
    {
        if (metaNumberToLoad > allMetaDataList.Count-1)
        {
            List<string> allMetaIDs = allMetaDataList.Select(meta => meta.metaID).ToList();
            GameManager.Instance.progressionManager.ResetAllMetaProgress(allMetaIDs, 0); 
            metaNumberToLoad = 0;
        }
       
        if (metaNumberToLoad < 0) 
            metaNumberToLoad = 0;

        if (_currentMetaObj != null && currentMetaNumber == metaNumberToLoad)
        {
            return;
        }

        if (_currentMetaObj != null)
        {
            Destroy(_currentMetaObj);
            _currentMetaObj = null;
            _currentMetaController = null;
        }
        
        currentMetaNumber = metaNumberToLoad;
        MetaData metaDataToLoad = allMetaDataList[currentMetaNumber];
        _currentLoadedMeta = metaDataToLoad;
        currentMetaName = metaDataToLoad.metaName;
        
        if (metaDataToLoad == null || metaDataToLoad.metaPrefab == null)
        {
            return;
        }

        _currentMetaObj = Instantiate(metaDataToLoad.metaPrefab);
    
        if (metaPrefabSpawnParent != null)
        {
            _currentMetaObj.transform.SetParent(metaPrefabSpawnParent, false);
            _metaOriginalLocalX = _currentMetaObj.transform.localPosition.x;
        }
        
        _currentMetaObj.SetActive(false); 

        _currentMetaController = _currentMetaObj.GetComponent<MetaController>();
        if (_currentMetaController != null)
        {
            
            _currentMetaController.ConfigureFromMetaData(metaDataToLoad);
            
            int savedCoins = GameManager.Instance.progressionManager.LoadMetaProgress(metaDataToLoad.metaID);
            _currentMetaController.LoadRepairProgress(savedCoins);
        }
        else
        {
            Destroy(_currentMetaObj);
            _currentMetaObj = null;
        }
        GameManager.Instance.uiManager.UpdateMetaPanel();

        if (_currentLoadedMeta != null)
        {
            GameManager.Instance.progressionManager.SaveCurrentMetaID(_currentLoadedMeta.metaID);
        }
    }

    public IEnumerator LoadNextMeta()
    {
        int nextMetaIndex = currentMetaNumber + 1;
        LoadMeta(nextMetaIndex);
        yield return new WaitForSecondsRealtime(1f);
        ActivateLoadedMeta();
    }
    
    public void ActivateLoadedMeta()
    {
        if (_metaShowcaseManager.IsShowingShowcase) return;
        
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue) 
        {
            _cityMetaManager.ActivateCityMetaView();
            if (_currentMetaObj != null)
            {
                _currentMetaObj.SetActive(false);
            }
        } 
        else 
        {
            _cityMetaManager.DeactivateCityMetaView();
            if (_currentMetaObj != null)
            {
                _currentMetaObj.SetActive(true);
                _currentMetaObj.transform.localPosition = new Vector3(45f, _currentMetaObj.transform.localPosition.y, _currentMetaObj.transform.localPosition.z);
                _isMetaAnimating = true;
                _currentMetaObj.transform.DOLocalMoveX(_metaOriginalLocalX, _fadeDuration).SetEase(Ease.OutQuad)
                    .SetDelay(0.35f)
                    .OnComplete(() =>
                    {
                        GameManager.Instance.uiManager.UpdateMetaPanel();
                        _isMetaAnimating = false;
                    });
                    
            }
            GameManager.Instance.audioManager.PlayMusic(_currentLoadedMeta.metaBgSouns);
        }
    }
    
    public void DeactivateLoadedMeta()
    {
        if (_metaShowcaseManager.IsShowingShowcase) return;

        if (_currentMetaObj != null)
        {
            _currentMetaObj.transform.DOKill();
            _isMetaAnimating = true; 
            _currentMetaObj.transform.DOLocalMoveX(-45, _fadeDuration).SetEase(Ease.InBack)
                 .OnComplete(() =>
                 {
                     _currentMetaObj.SetActive(false);
                     _isMetaAnimating = false; 
                 });
        }
    }
    
    public void DeactivateCityMeta()
    {
        _cityMetaManager.DeactivateCityMetaView();
    }

    public void ShowcaseMeta(int direction)
    {
        _metaShowcaseManager.ShowcaseMeta(direction);
    }
    
    public void DeactivateShowcaseMeta()
    {
        _metaShowcaseManager.DeactivateShowcaseMeta();
    }

    public string GetShowCaseMetaName()
    {
        return _metaShowcaseManager._CurrentMetName;
    }
    
    public IEnumerator AnimateOutAndLoadNextMeta()
    {
        if (_currentMetaObj != null)
        {
            _isMetaAnimating = true; 
            yield return _currentMetaObj.transform.DOLocalMoveX(-32, _fadeDuration).SetEase(Ease.InBack).WaitForCompletion();
            _isMetaAnimating = false; 
            
            Destroy(_currentMetaObj);
            _currentMetaObj = null;
        }

        int nextMetaIndex = currentMetaNumber + 1;
       
        LoadMeta(nextMetaIndex);
        
        yield return null;
        
        ActivateLoadedMeta();
    }
    
    public void FillMeta()
    {
        if (_currentMetaController == null) return;
        
        if (_isHandlingTransition || _isMetaAnimating || _currentMetaController.IsProcessingRepair() || _isShowingAdOption)
        {
            return;
        }

        if (_currentMetaController.IsFullyRepaired())
        {
            _metaCompletionCount++;
            
            if (GameManager.Instance.economyManager.GetCoins() > 0)
            {
                AnalyticsManager.Instance.ProgressionEventMetaMode(GAProgressionStatus.Complete, ("ID_"+_currentLoadedMeta.metaID+"_Count " + _metaCompletionCount));

                GameManager.Instance.economyManager.SpendCoins(1);
                StartCoroutine(AnimateOutAndLoadNextMeta());
                Vibration.VibratePop();
                return;
            }
            else
            {
                if (!_isHandlingTransition)
                {
                    _isHandlingTransition = true;
                    StartCoroutine(HandleMetaCompletionAndLoadNextLevel());
                    Vibration.VibratePop();
                }
                return;
            }
        }
        
        int availableCoins = GameManager.Instance.economyManager.GetCoins();
        if (availableCoins > 0)
        {
            int coinsToSpend = UnityEngine.Random.Range(2, 8);

            if (coinsToSpend <= availableCoins)
            {
                GameManager.Instance.economyManager.SpendCoins(coinsToSpend);
                _currentMetaController.SpendCoins(coinsToSpend);
                
                if (_coinAnimator != null && _currentMetaController._coinTargetTransform != null)
                {
                    _coinAnimator.numberOfCoins = coinsToSpend;
                    _coinAnimator.targetTransform = _currentMetaController._coinTargetTransform;
                    _coinAnimator.AnimateCoins();
                }

            }
            else
            {
                int actualCoinsSpent = 0;
                int maxSpend = (coinsToSpend == 4 && availableCoins < 4) ? availableCoins : coinsToSpend;
                maxSpend = Mathf.Min(maxSpend, _currentMetaController.GetRemainingCoinsToRepair());

                for (int i = 0; i < maxSpend && GameManager.Instance.economyManager.GetCoins() > 0; i++)
                {
                    GameManager.Instance.economyManager.SpendCoins(1);
                    _currentMetaController.SpendCoins(1);
                    actualCoinsSpent++;
                    
                }

                if (actualCoinsSpent > 0 && _coinAnimator != null && _currentMetaController._coinTargetTransform != null)
                {
                    _coinAnimator.numberOfCoins = actualCoinsSpent;
                    _coinAnimator.targetTransform = _currentMetaController._coinTargetTransform;
                    _coinAnimator.AnimateCoins();
                   
                }
              
            }
          
            GameManager.Instance.progressionManager.SaveMetaProgress(_currentLoadedMeta.metaID, _currentMetaController.GetCurrentSpentCoins());
            GameManager.Instance.uiManager.UpdateMetaCoins();
        }
        else
        {
            if (!_isHandlingTransition && !_hasShownAdOptionThisMeta)
            {
                HandleNoCoins();
            }
            else
            {
                _isShowingAdOption = false;
                _isHandlingTransition = false;
                get200CoinsObj.SetActive(false);
                _isShowingAdOption = false;
                GameManager.Instance.uiManager.HideAdCoinsOption(); 
                StartCoroutine(HandleMetaCompletionAndLoadNextLevel());
            }
        }
    }

   
    public void HandleNoCoins()
    {
        _isHandlingTransition = true;
        _isShowingAdOption = true;
        _hasShownAdOptionThisMeta = true;
        coinsChestObj.SetActive(true);
        get200CoinsObj.SetActive(true);
        
        get200CoinsObj.transform.position = startPos.position;
        GameManager.Instance.uiManager.ShowAdCoinsOption();
        GameManager.Instance.audioManager.StopMusic();
        //GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Drone,true);
        get200CoinsObj.transform.DOMove(targetPos.position, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            //GameManager.Instance.audioManager.StopLoopingSFX();
            get200CoinsObj.GetComponent<DOTweenAnimation>().DORestart();
        });
    }
    
    public IEnumerator WatchAdForCoins()
    {
        coinsChestObj.SetActive(false);
        
        if (chestOpenParticlesPrefab != null)
        {
            GameObject particlesInstance = Instantiate(chestOpenParticlesPrefab, particleEffectPos.transform.position, Quaternion.identity);
            Destroy(particlesInstance, 2.5f);
        }
        
        GameManager.Instance.audioManager.StopLoopingSFX();
        GameManager.Instance.uiManager.HideAdCoinsOption();
        
        GameManager.Instance.economyManager.AddCoins(200);
        GameManager.Instance.uiManager.UpdateMetaCoins();

        if (chestCoinAnimator != null)
        {
            Debug.Log("Ad watched successfully! Granting 200 coins.");
            chestCoinAnimator.CoinSize(1f);
            chestCoinAnimator.numberOfCoins = 25;
            chestCoinAnimator.animationDuration = 0.8f;
            chestCoinAnimator.spreadRadius = 2.0f;
            
            chestCoinAnimator.AnimateCoinsWithBlast();
        }
        yield return new WaitForSeconds(0.5f);
        get200CoinsObj.transform.DOMove(startPos.position, 0.75f).SetEase(Ease.Linear).OnComplete(() =>
        {
            get200CoinsObj.GetComponent<DOTweenAnimation>().DOPause();
            get200CoinsObj.SetActive(false);
        });

        _isShowingAdOption = false;
        _isHandlingTransition = false;
          
        yield return new WaitForSeconds(0.2f);
    }
    
    public void ProceedToNextLevel()
    {
        GameManager.Instance.uiManager.HideAdCoinsOption(); 
        //GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Drone,true);
        get200CoinsObj.transform.DOMove(startPos.position, 0.75f).SetEase(Ease.Linear).OnComplete(() =>
        {
            get200CoinsObj.GetComponent<DOTweenAnimation>().DOPause();
            get200CoinsObj.SetActive(false);
            
           // GameManager.Instance.audioManager.StopLoopingSFX();
       
            _isShowingAdOption = false;
            _isHandlingTransition = false;
            Debug.Log("ProceedToNextLevel()");
            StartCoroutine(HandleMetaCompletionAndLoadNextLevel());
        });
    }
    
    private IEnumerator HandleMetaCompletionAndLoadNextLevel()
    {
        GameManager.Instance.uiManager.HidePanel(UIPanelType.MetaPanel);
        Debug.Log("HandleMetaCompletionAndLoadNextLevel()");
        yield return new WaitForSeconds(0.15f);
        DeactivateLoadedMeta();

        yield return new WaitForSeconds(_levelLoadDelay);
        
        GameManager.Instance.uiManager.LevelLoaderUI(() =>
        {
            GameManager.Instance.levelManager.LoadNextLevel();
            GameManager.Instance.playerController.Init();
            GameManager.Instance.uiManager.UnLoadLoaderUI();
        });
        
        yield return new WaitForSeconds(0.2f);
        Debug.Log("LoadNextLevel()");
        GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
        RenderSettings.fog = true;
        _isHandlingTransition = false;
        
    }

    public MetaData GetMetaData(string id)
    {
        return allMetaDataList.FirstOrDefault(meta => meta.metaID == id);
    }

    public List<MetaData> GetMetaDataByType(MetaType type)
    {
        return allMetaDataList.Where(meta => meta.metaType == type).ToList();
    }
}