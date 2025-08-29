using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image baseImage;
    [SerializeField] private Image fillerImage;

    [SerializeField] private Image newTagImage;
    [SerializeField] private Image vipImage;
    [SerializeField] private Image stateBorderImage; 
    [SerializeField] private Sprite lockedStateSprite;
    [SerializeField] private Sprite ownedStateSprite; 
    [SerializeField] private Sprite selectedStateSprite;
    [SerializeField] private float stateAnimationDuration = 0.2f;

    public event Action<string> OnPurchaseClicked; 
    public event Action<string> OnItemSelected; 

    public IShopItem currentItem; 
    private bool _isOwned; 
    private bool _isSelected; 

    private Button _selectionButton;
    private GameObject _currentDemoInstance;

    private void Awake()
    {
        _selectionButton = GetComponent<Button>();
        if (_selectionButton != null)
        {
            _selectionButton.onClick.AddListener(OnItemSelectionClicked);
        }

        if (purchaseButton != null) 
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
    }

    private void OnDestroy()
    {
        if (_selectionButton != null)
        {
            _selectionButton.onClick.RemoveListener(OnItemSelectionClicked);
        }
        if (purchaseButton != null) 
            purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
            
        ClearDemoVisual();
    }

    public void Setup(IShopItem item, bool isOwned, Sprite baseSp, Sprite fillerSp) 
    { 
        currentItem = item; 
        _isOwned = isOwned; 

        if (baseImage != null) 
        { 
            baseImage.sprite = baseSp; 
            baseImage.gameObject.SetActive(true);  
        } 
        if (fillerImage != null) 
        { 
            fillerImage.sprite = fillerSp; 
            fillerImage.gameObject.SetActive(isOwned);  
        } 

        UpdateStateVisual(_isOwned, false);  
          
        if (purchaseButton != null) 
        { 
            purchaseButton.interactable = !isOwned; 
        } 
        
        if (_selectionButton != null)
        {
            _selectionButton.interactable = true;
        }

        // NEW: Check for VIP status and activate the VIP image
        if (vipImage != null)
        {
            ShopItemData shopItemData = currentItem as ShopItemData;
            if (shopItemData != null && shopItemData.GetIsAVIPTool())
            {
                vipImage.gameObject.SetActive(true);
            }
            else
            {
                vipImage.gameObject.SetActive(false);
            }
        }
        
        // NEW: Check for tool unlock availability and show/hide the "NEW" tag
        if (newTagImage != null)
        {
            if (GameManager.Instance.progressionManager.IsToolUnlockAvailable(currentItem.GetItemID()))
            {
                newTagImage.gameObject.SetActive(true);
            }
            else
            {
                newTagImage.gameObject.SetActive(false);
            }
        }
    } 

    public void SetSelected(bool selected) 
    { 
        _isSelected = selected; 
        UpdateStateVisual(_isOwned, _isSelected); 
        
        if (selected)
        {
            ShowDemoVisual(currentItem.GetDemoPrefab());
        }
        else
        {
            ClearDemoVisual();
        }
    } 

    private void UpdateStateVisual(bool owned, bool selected) 
    { 
        Sprite targetSprite = null; 

        if (selected) 
        { 
            targetSprite = selectedStateSprite; 
        } 
        else if (owned) 
        { 
            targetSprite = ownedStateSprite; 
        } 
        else 
        { 
            targetSprite = lockedStateSprite; 
        } 

        if (stateBorderImage != null) 
        { 
            if (stateBorderImage.sprite != targetSprite) 
            { 
                stateBorderImage.sprite = targetSprite; 
                stateBorderImage.gameObject.SetActive(targetSprite != null); 
            } 
        } 
    } 

    private void OnPurchaseButtonClicked() 
    { 
        if (currentItem != null && !_isOwned) 
        { 
            OnPurchaseClicked?.Invoke(currentItem.GetItemID()); 
        } 
    }

    private void OnItemSelectionClicked()
    {
        if (currentItem != null)
        {
            OnItemSelected?.Invoke(currentItem.GetItemID());

            // NEW: If the item is owned and the new tag is active, claim the unlock
            if (_isOwned && newTagImage != null && newTagImage.gameObject.activeSelf)
            {
                GameManager.Instance.progressionManager.ClaimToolUnlock(currentItem.GetItemID());
                newTagImage.gameObject.SetActive(false);
            }
        }
    }

    private void ShowDemoVisual(GameObject demoPrefab)
    {
        ClearDemoVisual(); 
        if (demoPrefab != null)
        {
            //_currentDemoInstance = Instantiate(demoPrefab, transform); 
            // Position the demo instance if needed, relative to ShopItemUI or a specific child Transform
            // Example: _currentDemoInstance.transform.localPosition = Vector3.zero;
        }
    }

    private void ClearDemoVisual()
    {
        if (_currentDemoInstance != null)
        {
            Destroy(_currentDemoInstance);
            _currentDemoInstance = null;
        }
    }
}