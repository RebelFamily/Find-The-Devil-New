using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public enum ShopMode
{
    Weapons,
    Scanners
}

public class ShopPanel : UIPanel
{
    
    [Header("Weapons Shop item ----------------------------------------")] 
    [SerializeField] private GameObject weaponPanelObj; 
    [SerializeField] private Transform weaponItemParent; 
    [SerializeField] private GameObject weaponItemPrefab; 
    [SerializeField] private Image weaponTextImg;
    
    [Header("Scanner Shop Item ----------------------------------------")] 
    [SerializeField] private GameObject scannerPanelObj; 
    [SerializeField] private Transform scannerItemParent; 
    [SerializeField] private GameObject scannerItemPrefab; 
    [SerializeField] private Image scannerTextImg;
    
    [Header("General Variables ----------------------------------------")] 
    [SerializeField] private Image bg_1;
    [SerializeField] private Image bg_2;
    [SerializeField] private Image bottom_bg;
    
    [SerializeField] private Button backButton;
    [SerializeField] private Button unlockItemButton;
    [SerializeField] private Button unlockAllItemButton;
    [SerializeField] private Button equipItemButton;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text gemsText; 
    [SerializeField] private List<ShopItemData> premiumShopItems; 
    [SerializeField] private GameObject toolDemoContainer;

    [Header("UI Animations")]
    [SerializeField] private ScaleAnimationHandler backButtonAnimator;
    [SerializeField] private ScaleAnimationHandler unlockItemButtonAnimator;
    [SerializeField] private ScaleAnimationHandler unlockAllItemButtonAnimator;
    [SerializeField] private ScaleAnimationHandler equipItemButtonAnimator;
    [SerializeField] private List<ScaleAnimationHandler> generalUIAnimators;
    [SerializeField] private ScaleAnimationHandler toolDemoContainerAnimator;


    public List<GameObject> _instantiatedItems = new List<GameObject>();
    private ShopItemUI _currentSelectedItemUI;
    public GameObject _currentDemoInstance;

    private ShopMode _currentShopMode = ShopMode.Weapons;

    private void Awake()
    {
        backButton?.onClick.AddListener(OnBackButtonClicked);
        unlockItemButton?.onClick.AddListener(OnUnlockItemButtonClicked);
        unlockAllItemButton?.onClick.AddListener(OnUnlockAllItemsButtonClicked);
        equipItemButton?.onClick.AddListener(OnEquipItemButtonClicked);

        EconomyManager.OnCoinsChanged += UpdateCoinsDisplay;
        EconomyManager.OnGemsChanged += UpdateGemsDisplay;
    }

    public override void Show()
    {
        base.Show();

        bg_1.gameObject.SetActive(true);
        bg_2.gameObject.SetActive(true);
        bottom_bg.gameObject.SetActive(true);
       
        if (bg_1 != null && bg_2 != null && bottom_bg != null)
        {
            Color color1 = bg_1.color;
            color1.a = 0;
            bg_1.color = color1;

            Color color2 = bg_2.color;
            color2.a = 0;
            bg_2.color = color2;
            
            Color bottomColor = bottom_bg.color;
            bottomColor.a = 0;
            bottom_bg.color = bottomColor;
            
            bg_1.DOFade(1, .5f);
            bg_2.DOFade(1, .5f);
            bottom_bg.DOFade(1, .5f);
        }
        
        unlockItemButton.gameObject.SetActive(false);
        unlockAllItemButton.gameObject.SetActive(false);
        
        AnimateStaticUIIn();
        
        UpdateCurrencyDisplay();
    }
    
    private void AnimateStaticUIIn()
    {
        int completedAnimations = 0;
        int totalAnimations = (generalUIAnimators?.Count ?? 0) + 4;

        Action onCompleteAction = () =>
        {
            completedAnimations++;
            if (completedAnimations >= totalAnimations)
            {
                RefreshShopItems();
            }
        };

        if (backButtonAnimator != null) backButtonAnimator.ScaleUp(onCompleteAction); else completedAnimations++;
        if (unlockItemButtonAnimator != null) unlockItemButtonAnimator.ScaleUp(onCompleteAction); else completedAnimations++;
        if (unlockAllItemButtonAnimator != null) unlockAllItemButtonAnimator.ScaleUp(onCompleteAction); else completedAnimations++;
        if (equipItemButtonAnimator != null) equipItemButtonAnimator.ScaleUp(onCompleteAction); else completedAnimations++;
        
        if (generalUIAnimators != null)
        {
            foreach (var animator in generalUIAnimators)
            {
                animator.ScaleUp(onCompleteAction);
            }
        }
    }

    private void OnDestroy()
    {
        backButton?.onClick.RemoveListener(OnBackButtonClicked);
        unlockItemButton?.onClick.RemoveListener(OnUnlockItemButtonClicked);
        unlockAllItemButton?.onClick.RemoveListener(OnUnlockAllItemsButtonClicked);
        equipItemButton?.onClick.RemoveListener(OnEquipItemButtonClicked);

        EconomyManager.OnCoinsChanged -= UpdateCoinsDisplay;
        EconomyManager.OnGemsChanged -= UpdateGemsDisplay;

        CleanupInstantiatedItems();
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        UpdateCurrencyDisplay();
        if (_currentSelectedItemUI != null)
        {
            UpdateButtonStates(_currentSelectedItemUI.currentItem, GameManager.Instance.progressionManager.HasItem(_currentSelectedItemUI.currentItem.GetItemID()));
        }
    }

    public override void Hide()
    {
        if (bg_1 != null) bg_1.DOFade(0, .5f);
        if (bg_2 != null) bg_2.DOFade(0, .5f);
        if (bottom_bg != null) bottom_bg.DOFade(0, .5f);
        
        float fadeDuration = 0.2f; 
        if (_currentShopMode == ShopMode.Weapons && weaponTextImg != null)
        {
            weaponTextImg.DOFade(0, fadeDuration);
        }
        else if (_currentShopMode == ShopMode.Scanners && scannerTextImg != null)
        {
            scannerTextImg.DOFade(0, fadeDuration);
        }
       
        List<ScaleAnimationHandler> itemAnimators = new List<ScaleAnimationHandler>();
        foreach (var itemGO in _instantiatedItems)
        {
            ScaleAnimationHandler itemAnimator = itemGO.GetComponent<ScaleAnimationHandler>();
            if (itemAnimator != null)
            {
                itemAnimators.Add(itemAnimator);
            }
        }
        
        int itemAnimations = itemAnimators.Count;
        if (itemAnimations > 0)
        {
            int completedItemAnimations = 0;
            Action onItemAnimationComplete = () =>
            {
                completedItemAnimations++;
                if (completedItemAnimations >= itemAnimations)
                {
                    AnimateStaticUIOut();
                }
            };
            
            foreach (var animator in itemAnimators)
            {
                animator.ScaleDown(onItemAnimationComplete);
            }
        }
        else
        {
            AnimateStaticUIOut();
        }
    }

    private void AnimateStaticUIOut()
    {
        List<ScaleAnimationHandler> staticAnimators = new List<ScaleAnimationHandler>();
        if (backButtonAnimator != null) staticAnimators.Add(backButtonAnimator);
        if (unlockItemButtonAnimator != null) staticAnimators.Add(unlockItemButtonAnimator);
        if (unlockAllItemButtonAnimator != null) staticAnimators.Add(unlockAllItemButtonAnimator);
        if (equipItemButtonAnimator != null) staticAnimators.Add(equipItemButtonAnimator);
        if (generalUIAnimators != null) staticAnimators.AddRange(generalUIAnimators);
        
        int completedStaticAnimations = 0;
        int totalStaticAnimations = staticAnimators.Count;

        Action onStaticAnimationComplete = () =>
        {
            completedStaticAnimations++;
            if (completedStaticAnimations >= totalStaticAnimations)
            {
                OnAllHideAnimationsComplete();
            }
        };

        if (totalStaticAnimations > 0)
        {
            foreach (var animator in staticAnimators)
            {
                animator.ScaleDown(onStaticAnimationComplete);
            }
        }
        else
        {
            onStaticAnimationComplete?.Invoke();
        }
    }

    private void OnAllHideAnimationsComplete()
    {
        base.Hide(); 
        CleanupInstantiatedItems();
        if (bg_1 != null) bg_1.gameObject.SetActive(false);
        if (bg_2 != null) bg_2.gameObject.SetActive(false);
        if (bottom_bg != null) bottom_bg.gameObject.SetActive(false);
            
        if(gameObject.activeInHierarchy)
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
    }

    public IEnumerator SetShopMode(ShopMode mode)
    {
        weaponTextImg.fillAmount = 0;
        scannerTextImg.fillAmount = 0;
        yield return new WaitForSeconds(0.1f);

        _currentShopMode = mode;
        weaponPanelObj?.SetActive(mode == ShopMode.Weapons);
        scannerPanelObj?.SetActive(mode == ShopMode.Scanners);
       
        AnimateShopChangeIn();
    }

    private IEnumerator AnimateShopChangeOut()
    {
        float fadeDuration = 0.2f; 
        if (_currentShopMode == ShopMode.Weapons && weaponTextImg != null)
        {
            weaponTextImg.DOFade(0, fadeDuration);
        }
        else if (_currentShopMode == ShopMode.Scanners && scannerTextImg != null)
        {
            scannerTextImg.DOFade(0, fadeDuration);
        }

        yield return new WaitForSecondsRealtime(fadeDuration);
    }

    private void AnimateShopChangeIn()
    {
        float fadeDuration = 0.2f;
        
        if (_currentShopMode == ShopMode.Weapons && weaponTextImg != null)
        {
            weaponTextImg.color = new Color(weaponTextImg.color.r, weaponTextImg.color.g, weaponTextImg.color.b, 0);
            weaponTextImg.DOFade(1, fadeDuration);
        }
        else if (_currentShopMode == ShopMode.Scanners)
        {
            scannerTextImg.color = new Color(scannerTextImg.color.r, scannerTextImg.color.g, scannerTextImg.color.b, 0);
            scannerTextImg.DOFade(1, fadeDuration);
        }
    }

    private void RefreshShopItems() 
    {
        CleanupInstantiatedItems();

        _currentSelectedItemUI = null; 

        IEnumerable<IShopItem> itemsToLoad = null;
        Transform parentTransform = null;
        GameObject itemPrefab = null;

        if (_currentShopMode == ShopMode.Weapons)
        {
            itemsToLoad = GameManager.Instance.shopManager.GetAvailableWeapons();
            parentTransform = weaponItemParent;
            itemPrefab = weaponItemPrefab;
        }
        else if (_currentShopMode == ShopMode.Scanners)
        {
            itemsToLoad = GameManager.Instance.shopManager.GetAvailableScanners();
            parentTransform = scannerItemParent;
            itemPrefab = scannerItemPrefab;
        }

        if (itemsToLoad == null || parentTransform == null || itemPrefab == null)
        {
            return;
        }

        foreach (var item in itemsToLoad)
        {
            GameObject itemGO = Instantiate(itemPrefab, parentTransform);
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();

            if (itemUI != null)
            {
                bool isOwned = GameManager.Instance.progressionManager.HasItem(item.GetItemID());
                itemUI.Setup(item, isOwned, item.GetBaseSprite(), item.GetFillerSprite());
                
                itemUI.OnPurchaseClicked -= (id) => HandlePurchaseRequest(item);
                itemUI.OnItemSelected -= HandleItemSelection;                     
                itemUI.OnPurchaseClicked += (id) => HandlePurchaseRequest(item); 
                itemUI.OnItemSelected += HandleItemSelection; 
                
                _instantiatedItems.Add(itemGO);

                ScaleAnimationHandler itemAnimator = itemGO.GetComponent<ScaleAnimationHandler>();
                if (itemAnimator != null)
                {
                    itemAnimator.ScaleUp();
                }

                if (_currentSelectedItemUI == null || (isOwned && GameManager.Instance.progressionManager.IsEquippedWeapon(item.GetItemID())) || (isOwned && GameManager.Instance.progressionManager.IsEquippedScanner(item.GetItemID())))
                {
                    _currentSelectedItemUI = itemUI;
                } else if (_currentSelectedItemUI == null && !isOwned) {
                    _currentSelectedItemUI = itemUI; 
                }
            }
        }
        
        if (_currentSelectedItemUI != null)
        {
            _currentSelectedItemUI.SetSelected(true);
            HandleItemSelection(_currentSelectedItemUI.currentItem.GetItemID()); 
        } else {
            UpdateButtonStates(null, false);
        }
    
        RectTransform parentRectTransform = parentTransform.GetComponent<RectTransform>();
        ContentSizeFitter fitter = parentRectTransform.GetComponent<RectTransform>().GetComponent<ContentSizeFitter>();


        if (fitter != null)
        {
            fitter.enabled = false;
        }

        parentRectTransform.DOAnchorPosY(0, 0.5f)
            .SetDelay(0.4f)
            .OnComplete(() =>
            {
                
                if (fitter != null)
                {
                    fitter.enabled = true;
                }
            });
    }

    private void HandlePurchaseRequest(IShopItem itemToPurchase)
    {
        if (itemToPurchase == null)
        {
            return;
        }

        string itemID = itemToPurchase.GetItemID();
        
        if (GameManager.Instance.progressionManager.UnlockItem(itemID)) 
        {
            ClearDemoVisual(() =>
            {
                RefreshShopItems();
                HandleItemSelection(itemID);
            });
        }
        else
        {
            GameManager.Instance.uiManager.ShowGeneralMessage("Not enough currency!");
        }
    }

    private void HandleItemSelection(string selectedItemId)
    {
        
        ShopItemUI newSelectedItemUI = _instantiatedItems
            .Select(go => go.GetComponent<ShopItemUI>())
            .FirstOrDefault(itemUI => itemUI != null && itemUI.currentItem?.GetItemID() == selectedItemId);

        if (newSelectedItemUI != null && newSelectedItemUI != _currentSelectedItemUI)
        {
            _currentSelectedItemUI?.SetSelected(false);
            _currentSelectedItemUI = newSelectedItemUI;
            _currentSelectedItemUI.SetSelected(true);
            
           
            UpdateDemoVisual(newSelectedItemUI.currentItem.GetDemoPrefab());
        }
        else if (newSelectedItemUI != null && newSelectedItemUI == _currentSelectedItemUI)
        {
            
            UpdateDemoVisual(newSelectedItemUI.currentItem.GetDemoPrefab());
        }

        if (newSelectedItemUI != null)
        {
            bool isOwned = GameManager.Instance.progressionManager.HasItem(newSelectedItemUI.currentItem.GetItemID());
            UpdateButtonStates(newSelectedItemUI.currentItem, isOwned);
        }
        else
        {
            UpdateButtonStates(null, false);
        }
    }

    private void UpdateCurrencyDisplay()
    {
        UpdateCoinsDisplay(GameManager.Instance.economyManager.GetCoins());
        UpdateGemsDisplay(GameManager.Instance.economyManager.GetGems());
    }

    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
    }

    private void UpdateGemsDisplay(int gems)
    {
        if (gemsText != null) gemsText.text = gems.ToString();
    }

    private void OnBackButtonClicked()
    {
        GameManager.Instance.audioManager.StopLoopingSFX();
        GameManager.Instance.levelManager.HideObjectInLevel(true);
        Hide();
        ClearDemoVisual();
    }

    private void OnUnlockItemButtonClicked()
    {
        if (_currentSelectedItemUI != null && _currentSelectedItemUI.currentItem != null && !_currentSelectedItemUI.currentItem.GetIsAVIPTool())
        {
            AdsCaller.Instance.ShowRewardedAd((() =>
            {
                HandlePurchaseRequest(_currentSelectedItemUI.currentItem);
            }));
        }
       
    }

    // InApp function call
    public void UnlockAllItems(PurchaseType type)
    {
        
        List<IShopItem> itemsToUnlock = new List<IShopItem>();


        if (type == PurchaseType.AllGuns)
        {
            
            itemsToUnlock.AddRange(GameManager.Instance.shopManager.GetAvailableWeapons());
        }
        else if (type == PurchaseType.AllScanners)
        {
            
            
            itemsToUnlock.AddRange(GameManager.Instance.shopManager.GetAvailableScanners());
        }

        if (itemsToUnlock.Count == 0)
        {
            return;
        }

        foreach (var item in new List<IShopItem>(itemsToUnlock))
        {
            if (!GameManager.Instance.progressionManager.HasItem(item.GetItemID()))
            {
                GameManager.Instance.progressionManager.UnlockItem(item.GetItemID());
            }
        }
        
        ClearDemoVisual(() =>
        {
            RefreshShopItems();
            if (_currentSelectedItemUI != null)
            {
                HandleItemSelection(_currentSelectedItemUI.currentItem.GetItemID());
            }
            else if (_instantiatedItems.Any())
            {
                HandleItemSelection(_instantiatedItems.FirstOrDefault()?.GetComponent<ShopItemUI>()?.currentItem.GetItemID());
            }
        });
        
    }

    private void OnUnlockAllItemsButtonClicked()
    {
       
        if (_currentShopMode == ShopMode.Weapons)
        {
            IAPManager.Instance.InAppCaller(PurchaseType.AllGuns);
        }
        else if (_currentShopMode == ShopMode.Scanners)
        {
            IAPManager.Instance.InAppCaller(PurchaseType.AllScanners);
        }
    }

    private void OnEquipItemButtonClicked()
    {
        if (_currentSelectedItemUI != null && _currentSelectedItemUI.currentItem != null)
        {
            if (_currentShopMode == ShopMode.Weapons)
            {
                GameManager.Instance.progressionManager.EquipWeapon(_currentSelectedItemUI.currentItem.GetItemID());
                GameManager.Instance.playerController.EquipLaserGun(GameManager.Instance.shopManager.GetEquippedWeaponPrefab());
            }
            else if (_currentShopMode == ShopMode.Scanners)
            {
                GameManager.Instance.progressionManager.EquipScanner(_currentSelectedItemUI.currentItem.GetItemID());
                GameManager.Instance.playerController.EquipScanner(GameManager.Instance.shopManager.GetEquippedScannerPrefab());
            }
            
            UpdateButtonStates(_currentSelectedItemUI.currentItem, true);  
        }

        OnBackButtonClicked();
    }
    
    // --- UPDATED METHOD ---
   private void UpdateButtonStates(IShopItem selectedItem, bool isOwned)
{
    if (unlockItemButton == null || unlockAllItemButton == null || equipItemButton == null)
    {
        return;
    }

    if (selectedItem == null)
    {
        unlockItemButton.gameObject.SetActive(false);
        unlockAllItemButton.gameObject.SetActive(false);
        equipItemButton.gameObject.SetActive(false);
        return;
    }
    
    // Check if the item is a VIP tool
    if (selectedItem.GetIsAVIPTool())
    {
        if (isOwned)
        {
            // If the VIP item is owned, show only the "Equip" button.
            unlockItemButton.gameObject.SetActive(false);
            unlockAllItemButton.gameObject.SetActive(false);
            equipItemButton.gameObject.SetActive(true);
            
            bool isCurrentlyEquipped = false;
            if (_currentShopMode == ShopMode.Weapons)
            {
                isCurrentlyEquipped = GameManager.Instance.progressionManager.IsEquippedWeapon(selectedItem.GetItemID());
            }
            else if (_currentShopMode == ShopMode.Scanners)
            {
                isCurrentlyEquipped = GameManager.Instance.progressionManager.IsEquippedScanner(selectedItem.GetItemID());
            }
            equipItemButton.interactable = !isCurrentlyEquipped;
        }
        else
        {
            // If not owned, show only the "Unlock All" button.
            unlockItemButton.gameObject.SetActive(false);
            unlockAllItemButton.gameObject.SetActive(true);
            equipItemButton.gameObject.SetActive(false);
            
            // The Unlock All button is always interactive unless the IAP is already purchased
            unlockAllItemButton.interactable = true; 
        }
    }
    else
    {
        // This is the existing logic for regular, non-VIP items
        bool isCurrentlyEquipped = false;
        if (_currentShopMode == ShopMode.Weapons)
        {
            isCurrentlyEquipped = GameManager.Instance.progressionManager.IsEquippedWeapon(selectedItem.GetItemID());
        }
        else if (_currentShopMode == ShopMode.Scanners)
        {
            isCurrentlyEquipped = GameManager.Instance.progressionManager.IsEquippedScanner(selectedItem.GetItemID());
        }

        if (isOwned)
        {
            unlockItemButton.gameObject.SetActive(false);
            unlockAllItemButton.gameObject.SetActive(false);
            equipItemButton.gameObject.SetActive(true);
            equipItemButton.interactable = !isCurrentlyEquipped;
        }
        else
        {
            unlockItemButton.gameObject.SetActive(true);
            unlockAllItemButton.gameObject.SetActive(true);
            equipItemButton.gameObject.SetActive(false);
            unlockItemButton.interactable = true;
            unlockAllItemButton.interactable = true;
        }
    }
}
    private void UpdateDemoVisual(GameObject demoPrefab)
    {
        ClearDemoVisual(() => AnimateNewDemo(demoPrefab));
    }

    // --- UPDATED METHOD ---
    private void AnimateNewDemo(GameObject demoPrefab)
    {
        if (toolDemoContainer != null && demoPrefab != null)
        {
            _currentDemoInstance = Instantiate(demoPrefab, toolDemoContainer.transform);
            // additional check
            _currentDemoInstance.GetComponent<LaserReactionAutomator>().SetGunToPosition();
            toolDemoContainer.SetActive(true);
        }
    }

    // --- UPDATED METHOD ---
    private void ClearDemoVisual(Action onComplete = null)
    {
        if (toolDemoContainer != null)
        {
            foreach (Transform child in toolDemoContainer.transform)
            {
                Destroy(child.gameObject);
            }
            _currentDemoInstance = null;
            toolDemoContainer.SetActive(false);
        }
        onComplete?.Invoke();
    }

    private void CleanupInstantiatedItems()
    {
        foreach (var itemGO in _instantiatedItems)
        {
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                itemUI.OnPurchaseClicked -= (id) => HandlePurchaseRequest(itemUI.currentItem);
                itemUI.OnItemSelected -= HandleItemSelection;
            }
            Destroy(itemGO);
        }
        _instantiatedItems.Clear();
    }
}