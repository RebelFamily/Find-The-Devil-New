using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class MetaShowcaseManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private List<MetaData> allMetaDataList;
    [SerializeField] private Transform metaPrefabSpawnParent;
    [SerializeField] private ProgressionManager progressionManager;
    
    [Header("Animation Settings")]
    [SerializeField] public float fadeDuration = 0.5f;

    // Showcase Meta Variables
    private GameObject _showcaseMetaObj;
    private MetaController _showcaseMetaController;
    private int _showcaseMetaNumber = -1;
    private float _metaOriginalLocalX = 0f;
    private bool _isMetaAnimating = false; // New animation flag
    public string _CurrentMetName;
    public bool IsShowingShowcase => _showcaseMetaObj != null;

    public void ShowcaseMeta(int direction)
    {
        // Don't allow a new showcase transition if one is already in progress
        if (_isMetaAnimating) return;

        // Stop any ongoing animations or coroutines on the main MetaManager
        if (GameManager.Instance.metaManager != null)
        {
            GameManager.Instance.metaManager.DeactivateLoadedMeta();
            GameManager.Instance.metaManager.DeactivateCityMeta();
        }

        // Deactivate the current showcase meta before loading the next one
        if (_showcaseMetaObj != null)
        {
            // We use a coroutine to handle the animation out and then loading the next one
            StartCoroutine(AnimateOutAndLoadNextShowcase(direction));
        }
        else
        {
            // If there's no showcase meta currently, just load the next one directly
            LoadShowcaseMeta(direction);
            ActivateShowcaseMeta();
        }
    }

    private void LoadShowcaseMeta(int metaNumberToLoad)
    {
        if (_showcaseMetaObj != null)
        {
            Destroy(_showcaseMetaObj);
            _showcaseMetaObj = null;
            _showcaseMetaController = null;
        }
        
        
        
        _showcaseMetaNumber = metaNumberToLoad;
        MetaData metaDataToLoad = allMetaDataList[_showcaseMetaNumber];

        if (metaDataToLoad == null || metaDataToLoad.metaPrefab == null)
        {
            return;
        }

        _showcaseMetaObj = Instantiate(metaDataToLoad.metaPrefab);
        
        if (metaPrefabSpawnParent != null)
        {
            _showcaseMetaObj.transform.SetParent(metaPrefabSpawnParent, false);
            _metaOriginalLocalX = _showcaseMetaObj.transform.localPosition.x;
        }

        _CurrentMetName = metaDataToLoad.metaName;
        _showcaseMetaObj.SetActive(true); 
        
        GameManager.Instance.uiManager.UpdateMetaPanel();

        _showcaseMetaController = _showcaseMetaObj.GetComponent<MetaController>();
        if (_showcaseMetaController != null)
        {
            // --- NEW: Set the showcase flag on the MetaController ---
            _showcaseMetaController.IsShowcaseMeta = true;
            
            _showcaseMetaController.ConfigureFromMetaData(metaDataToLoad);
            
            int savedCoins = GameManager.Instance.progressionManager.LoadMetaProgress(metaDataToLoad.metaID);
            _showcaseMetaController.LoadRepairProgress(savedCoins);
        }
        else
        {
            Destroy(_showcaseMetaObj);
            _showcaseMetaObj = null;
        }
    }

    private void ActivateShowcaseMeta()
    {
        if (_showcaseMetaObj != null)
        {
            _showcaseMetaObj.SetActive(true);
            _showcaseMetaObj.transform.localPosition = new Vector3(45f, _showcaseMetaObj.transform.localPosition.y, _showcaseMetaObj.transform.localPosition.z);
            _isMetaAnimating = true; 
            _showcaseMetaObj.transform.DOLocalMoveX(_metaOriginalLocalX, fadeDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => _isMetaAnimating = false);
        }
    }
    
    public void DeactivateShowcaseMeta()
    {
        if (_showcaseMetaObj != null)
        {
            _isMetaAnimating = true;
            _showcaseMetaObj.transform.DOLocalMoveX(-45, fadeDuration).SetEase(Ease.InBack)
                 .OnComplete(() =>
                 {
                     Destroy(_showcaseMetaObj);
                     _showcaseMetaObj = null;
                     _showcaseMetaController = null;
                     _isMetaAnimating = false;
                     _showcaseMetaNumber = -1;
                 });
        }
    }

    private IEnumerator AnimateOutAndLoadNextShowcase(int direction)
    {
        if (_showcaseMetaObj != null)
        {
            _isMetaAnimating = true;
            yield return _showcaseMetaObj.transform.DOLocalMoveX(-45, fadeDuration).SetEase(Ease.InBack).WaitForCompletion();
            _isMetaAnimating = false;
        }
        
        int nextMetaIndex = _showcaseMetaNumber + direction;

        if (nextMetaIndex >= allMetaDataList.Count)
        {
            nextMetaIndex = 0;
        }
        else if (nextMetaIndex < 0)
        {
            nextMetaIndex = allMetaDataList.Count - 1;
        }
        
        LoadShowcaseMeta(nextMetaIndex);
        ActivateShowcaseMeta();
    } 
}