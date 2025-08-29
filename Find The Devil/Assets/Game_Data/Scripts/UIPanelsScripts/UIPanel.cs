using UnityEngine;
using DG.Tweening;

public abstract class UIPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private UIPanelType panelType;
    
    
    
    [Header("Activation Animation")]
    [SerializeField] private Vector3 activeTargetScale = Vector3.one; 
    [SerializeField] private float activeAnimationDuration = 0.3f; 
    [SerializeField] private Ease activeEaseType = Ease.OutBack; 
    [SerializeField] private float activationDelay = 0f; // New: Delay before activation animation starts

    [Header("Deactivation Animation")]
    [SerializeField] private Vector3 disabledTargetScale = Vector3.zero; 
    [SerializeField] private float disabledAnimationDuration = 0.2f; 
    [SerializeField] private Ease disabledEaseType = Ease.InBack;
    [SerializeField] private float deactivationDelay = 0f; // New: Delay before deactivation animation starts

    [SerializeField] public bool isVIPGunAnimationss = false;
    
    private bool _isCurrentlyIntendedActive = false; 
    private Vector3 initialLocalScale;

    public UIPanelType GetPanelType()
    {
        return panelType;
    }

    protected virtual void Awake()
    {
        initialLocalScale = transform.localScale;
    }

    public virtual void Show() 
    {
        if (_isCurrentlyIntendedActive && gameObject.activeSelf && transform.localScale == activeTargetScale)
        {
            return;
        }

        _isCurrentlyIntendedActive = true; 
        
        gameObject.SetActive(true);
        
        ActiveScaleAnimation();
    }

    public virtual void Hide()
    {
        if (!_isCurrentlyIntendedActive && !gameObject.activeSelf && transform.localScale == disabledTargetScale)
        {
            return;
        }

        _isCurrentlyIntendedActive = false; 

        DisableStateAnimation();
    }

    public virtual void UpdatePanel()
    { 
        
    }

    public void ActiveScaleAnimation() 
    {
        transform.DOKill(true); 
      
        transform.DOScale(activeTargetScale, activeAnimationDuration)
            .SetEase(activeEaseType)
            .From(transform.localScale)
            .SetDelay(activationDelay) // Apply activation delay
            .OnComplete(() => { });
    }

    public void DisableStateAnimation() 
    { 
        transform.DOKill(true); 
        
        transform.DOScale(disabledTargetScale, disabledAnimationDuration)
            .SetEase(disabledEaseType)
            .SetDelay(deactivationDelay) // Apply deactivation delay
            .OnComplete(() => {
                if (!_isCurrentlyIntendedActive) 
                {
                    gameObject.SetActive(false);
               
                    transform.localScale = initialLocalScale; 
                }
            });
    }
    
    public void ResetScaleImmediate()
    {
        transform.DOKill(true);
        transform.localScale = initialLocalScale; 
        gameObject.SetActive(false); 
        _isCurrentlyIntendedActive = false;
    }
}
