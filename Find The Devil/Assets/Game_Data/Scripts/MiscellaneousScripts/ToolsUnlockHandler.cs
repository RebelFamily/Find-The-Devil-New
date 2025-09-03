using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening; 
using System.Linq;

public class ToolsUnlockHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The list of tools to unlock, in order.")]
    public List<ShopItemData> toolsToUnlock;

    [Header("UI Elements")]
    public Image baseContainer;
    public Image fillerContainer;
    public Text fillPercent;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    [SerializeField] private GameObject _unlockEffectPrefab;

    [Header("Demo Object Settings")]
    [SerializeField] private Transform demoSpawnPoint; 
    
    private GameObject _spawnedDemoTool; 
    private ShopItemData _lastUnlockedTool; 
    private string _lastUnlockedToolItemID; 
    
    // NEW: Variable to track if a tool unlock is ready to be claimed
    private string _toolUnlockPendingID = null;

    public Tween UpdateToolDisplay()
    {
        if (GameManager.Instance.progressionManager == null)
        {
            Debug.LogError("ProgressionManager is not assigned!");
            return null;
        }

        // --- CORRECTED: Modify the FirstOrDefault query to account for both unlocked and "unlock available" states. ---
        // Find the first tool that is neither already unlocked nor waiting to be claimed.
        ShopItemData currentToolData = toolsToUnlock.FirstOrDefault(
            tool => !GameManager.Instance.progressionManager.HasItem(tool.GetItemID()) && 
                    !GameManager.Instance.progressionManager.IsToolUnlockAvailable(tool.GetItemID())
        );
        
        // Handle the case where the current tool is already waiting to be claimed.
        // This will find the tool that is available for unlock, and set the UI to 100%
        if (currentToolData == null)
        {
            currentToolData = toolsToUnlock.FirstOrDefault(tool => GameManager.Instance.progressionManager.IsToolUnlockAvailable(tool.GetItemID()));
            if (currentToolData != null)
            {
                 if (baseContainer != null) baseContainer.gameObject.SetActive(true);
                 if (fillerContainer != null)
                 {
                     fillerContainer.gameObject.SetActive(true);
                     fillerContainer.sprite = currentToolData.GetFillerSprite();
                     fillerContainer.type = Image.Type.Filled;
                     fillerContainer.fillAmount = 1f;
                 }
                 if (fillPercent != null)
                 {
                     fillPercent.gameObject.SetActive(true);
                     fillPercent.text = "100 %";
                 }
                return null; // Don't animate, just show the full bar.
            }
        }
        
        if (currentToolData == null)
        {
            if (baseContainer != null) baseContainer.gameObject.SetActive(false);
            if (fillerContainer != null) fillerContainer.gameObject.SetActive(false);
            if (fillPercent != null) fillPercent.gameObject.SetActive(false);
            
            if (_spawnedDemoTool != null) Destroy(_spawnedDemoTool);
            _lastUnlockedToolItemID = null; 
            return null;
        }
        
        int currentProgress = GameManager.Instance.progressionManager.LoadToolUnlockProgress(currentToolData.GetItemID());
        int progressDivisions = currentToolData.GetProgressDivisions();
        
        bool wasUnlockedBeforeUpdate = currentProgress >= progressDivisions;
        
        currentProgress++;
        GameManager.Instance.progressionManager.SaveToolUnlockProgress(currentToolData.GetItemID(), currentProgress);
        
        bool isNowUnlocked = currentProgress >= progressDivisions;
        
        if (_spawnedDemoTool != null)
        {
            Destroy(_spawnedDemoTool);
            _spawnedDemoTool = null;
        }
    
        if (baseContainer != null) baseContainer.gameObject.SetActive(true);
        if (fillerContainer != null) fillerContainer.gameObject.SetActive(true);
        if (fillPercent != null) fillPercent.gameObject.SetActive(true);
        
        if (baseContainer != null)
        {
            baseContainer.sprite = currentToolData.GetBaseSprite();
        }

        if (fillerContainer != null)
        {
            fillerContainer.sprite = currentToolData.GetFillerSprite();
            fillerContainer.type = Image.Type.Filled;
        }
        
        float targetFillAmount = progressDivisions > 0 ? (float)currentProgress / progressDivisions : 1f;

        if (fillerContainer != null)
        {
            fillerContainer.DOKill();
            
            float startFillAmount = progressDivisions > 0 ? (float)(currentProgress - 1) / progressDivisions : 1f;
            fillerContainer.fillAmount = startFillAmount;
            
            Sequence fillSequence = DOTween.Sequence();
            fillSequence.Append(fillerContainer.DOFillAmount(targetFillAmount, animationDuration).SetEase(Ease.OutSine));

            int startPercentage = Mathf.RoundToInt(startFillAmount * 100);
            int endPercentage = Mathf.RoundToInt(targetFillAmount * 100);
            fillSequence.Join(DOTween.To(() => startPercentage, x => fillPercent.text = x.ToString() + " %", endPercentage, animationDuration));
            
            if (!wasUnlockedBeforeUpdate && isNowUnlocked)
            {
                // CHANGE: Instead of unlocking the item, set an "unlock available" flag
                GameManager.Instance.progressionManager.SetToolUnlockAvailable(currentToolData.GetItemID());
                
                _lastUnlockedTool = currentToolData;
                _lastUnlockedToolItemID = currentToolData.GetItemID(); 
                
                fillSequence.AppendCallback(() => {
                    if (baseContainer != null) baseContainer.gameObject.SetActive(false);
                    if (fillerContainer != null) fillerContainer.gameObject.SetActive(false);
                    if (fillPercent != null) fillPercent.gameObject.SetActive(false);

                    if (_unlockEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(_unlockEffectPrefab, fillerContainer.transform.position, Quaternion.identity);
                        Destroy(effect, 2f);
                    }

                    if (_lastUnlockedTool.GetDemoPrefab() != null && demoSpawnPoint != null)
                    {
                        _spawnedDemoTool = Instantiate(_lastUnlockedTool.GetDemoPrefab(), demoSpawnPoint);
                    }
                });
            } else {
                _lastUnlockedToolItemID = null; 
            }
            
            return fillSequence;
        }
        
        _lastUnlockedToolItemID = null; 
        return null;
    }
    
    public GameObject GetSpawnedDemoTool()
    {
        return _spawnedDemoTool;
    }

    public void DestroySpawnedDemoTool()
    {
        if (_spawnedDemoTool != null)
        {
            Destroy(_spawnedDemoTool);
            _spawnedDemoTool = null;
            _lastUnlockedToolItemID = null; 
        }
    }
    
    /// <summary>
    /// Returns the ID of the last unlocked tool and resets the internal state.
    /// This should be called by the UI when a tool unlock popup is shown.
    /// </summary>
    /// <returns>The item ID of the newly unlocked tool, or null if none is pending.</returns>
    public string GetUnlockedToolIdAndReset()
    {
        string unlockedId = _lastUnlockedToolItemID;
        _lastUnlockedToolItemID = null;
        return unlockedId;
    }
     
    /// <summary>
    /// Gets the data for the last tool whose unlock was completed.
    /// </summary>
    /// <returns>The ShopItemData for the tool.</returns>
    public ShopItemData GetLastUnlockedToolData()
    {
        return _lastUnlockedTool;
    }
    
    /// <summary>
    /// Checks if all tools in the toolsToUnlock list have been permanently unlocked.
    /// </summary>
    /// <returns>True if all tools are unlocked, otherwise false.</returns>
    public bool AreAllToolsUnlocked()
    {
        if (GameManager.Instance.progressionManager == null)
        {
            Debug.LogError("ProgressionManager is not assigned!");
            return false;
        }
        
        // Use LINQ's All() method to check if every tool is unlocked.
        return toolsToUnlock.All(tool => GameManager.Instance.progressionManager.HasItem(tool.GetItemID()));
    }
}