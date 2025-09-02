using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MetaPanelMode
{
    MetaFiller, // For building/filling a meta (Hold to Build, Coin Text)
    BasesShow // For viewing city metas (Left/Right buttons, Village Progress)
}

public class MetaPanel : UIPanel, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Meta Filler Panel References")] 
    [SerializeField] private GameObject metaFillerPanel;
    [SerializeField] public Button _holdToBuildBtn;
    [SerializeField] private Text _coinText;
    
    [Header("Bases Show Panel References")]
    [SerializeField] private GameObject basesShowPanel;
    [SerializeField] private Button _leftBtn;
    [SerializeField] private Button _rightBtn;
    [SerializeField] private Button _closeButton;

    // NEW: Panel for the ad buttons
    [Header("Ad Coins Panel References")]
    [SerializeField] private GameObject adCoinsPanel;
    [SerializeField] private Button get200CoinsBtn; 
    [SerializeField] private Button noThanksbtn;
    [SerializeField] private Image get200CoinBG;
    [SerializeField] private Image spendProgressImage;
    public Image buttonFillImg;
    private int _initialCoinCount;

    
    [Header("Common UI References")] 
    [SerializeField] private GameObject _metaInfoContainer; 
    [SerializeField] private Text _metaNameText; 

    [Header("Hold Button Settings")] 
    [SerializeField] private float _holdTickInterval = 0.1f; 
    [SerializeField] private bool _fireImmediatelyOnPress = true;
    
    [Header("UI Animations")]
    [SerializeField] private ScaleAnimationHandler leftButtonAnimator;
    [SerializeField] private ScaleAnimationHandler rightButtonAnimator;
    [SerializeField] private ScaleAnimationHandler closeButtonAnimator;
    [SerializeField] private ScaleAnimationHandler metaInfoAnimator;

    public UnityEvent OnHold; 
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    private bool isNothanks = false;
    private bool _isHolding = false;
    private float _nextCallTime = 0f;
    
    private void Awake() 
    {
        if (OnHold == null) OnHold = new UnityEvent();
        if (OnPressed == null) OnPressed = new UnityEvent();
        if (OnReleased == null) OnReleased = new UnityEvent();
        
        if(_coinText != null)
            _coinText.text = "X " + GameManager.Instance.economyManager.GetCoins();

        // NEW: Add listeners to the new buttons
        if (get200CoinsBtn != null)
        {
            get200CoinsBtn.onClick.AddListener(OnGet200CoinsClicked);
        }
        if (noThanksbtn != null)
        {
            noThanksbtn.onClick.AddListener(OnNoThanksClicked);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(CloseBasePanel);
        }
    }

    private void OnEnable()
    {
        // --- REMOVED: Initializing coin count here as per your request ---
        // _initialCoinCount = GameManager.Instance.economyManager.GetCoins();
        // --- END REMOVED ---

        if(_coinText != null)
            _coinText.text = "X " + GameManager.Instance.economyManager.GetCoins();
        
        UpdateHoldButtonInteractability();
    }

    public override void Hide()
    {
        // Get all animators that might be active
        List<ScaleAnimationHandler> animatorsToHide = new List<ScaleAnimationHandler>();
        if (leftButtonAnimator != null && leftButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(leftButtonAnimator);
        if (rightButtonAnimator != null && rightButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(rightButtonAnimator);
        if (closeButtonAnimator != null && closeButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(closeButtonAnimator);
        if (metaInfoAnimator != null && metaInfoAnimator.gameObject.activeSelf) animatorsToHide.Add(metaInfoAnimator);

        int totalAnimations = animatorsToHide.Count;
        if (totalAnimations == 0)
        {
            base.Hide();
            return;
        }

        int completedAnimations = 0;
        Action onComplete = () =>
        {
            completedAnimations++;
            if (completedAnimations >= totalAnimations)
            {
                base.Hide();
            }
        };

        foreach (var animator in animatorsToHide)
        {
            animator.ScaleDown(onComplete);
        }
    }

    public override void UpdatePanel()
    {
        Debug.Log("if (_currentMetaController != null)");
        if (basesShowPanel && basesShowPanel.activeSelf)
        {
            Debug.Log("if (_current)");
            _metaNameText.text = GameManager.Instance.metaManager.GetShowCaseMetaName();
        }
        else
        {
            Debug.Log("if (_current)=======================");
            _metaNameText.text = GameManager.Instance.metaManager.currentMetaName;
        }
        
        UpdateHoldButtonInteractability(); 
    }

    private void Update()
    {
        UpdateHoldButtonInteractability(); 

        if (_isHolding)
        {
      
            if (GameManager.Instance.metaManager._currentMetaController.IsProcessingRepair())
            {
                _isHolding = false;
                OnReleased.Invoke();
                return; 
            }

            if (Time.time >= _nextCallTime)
            {
                OnHold.Invoke();
                _nextCallTime = Time.time + _holdTickInterval;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance.metaManager._currentMetaController.IsProcessingRepair()) return;

        if (metaFillerPanel == null || !metaFillerPanel.activeSelf || _holdToBuildBtn == null || !_holdToBuildBtn.interactable) return;

        if (eventData.button == PointerEventData.InputButton.Left) 
        {
            _isHolding = true;
            OnPressed.Invoke();
            if (_fireImmediatelyOnPress)
            {
                _nextCallTime = Time.time;
            }
            else
            {
                _nextCallTime = Time.time + _holdTickInterval;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isHolding)
        {
            _isHolding = false;
            OnReleased.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHolding)
        {
            _isHolding = false;
            OnReleased.Invoke();
        }
    }

    public void UpdateCoinText()
    {
        if (_holdToBuildBtn != null && _holdToBuildBtn.TryGetComponent<DOTweenAnimation>(out DOTweenAnimation anim))
        {
            anim.DORestart();
        }
        if(_coinText != null)
            _coinText.text = "X " + GameManager.Instance.economyManager.GetCoins();
        
        // --- NEW: Call the fill update function with the current coin count ---
        UpdateCoinFill(GameManager.Instance.economyManager.GetCoins());
        // --- END NEW ---
    }
    
    public void UpdateCoinFill(int currentCoins)
    {
        // Avoid division by zero if there are no starting coins.
        if (_initialCoinCount <= 0)
        {
            if (buttonFillImg != null) buttonFillImg.fillAmount = 0f;
            return;
        }

        // Calculate the fill amount as a percentage.
        // Formula: (Current Coins / Total Starting Coins)
        float targetFillAmount = (float)currentCoins / _initialCoinCount;
        
        // --- FIXED: Use a DOTween animation to smoothly update the fill amount ---
        if (buttonFillImg != null)
        {
            buttonFillImg.DOFillAmount(targetFillAmount, 0.25f).SetEase(Ease.OutQuad);
        }
        // --- END FIXED ---
    }


    public void SetPanelMode(MetaPanelMode mode)
    {
        if (mode == MetaPanelMode.MetaFiller)
        {
            // --- NEW: Initialize the initial coin count here as requested ---
            _initialCoinCount = GameManager.Instance.economyManager.GetCoins();
            if (_initialCoinCount > 0)
            {
                UpdateCoinFill(_initialCoinCount);
            }
            // --- END NEW ---
            
            metaFillerPanel?.SetActive(true);
            basesShowPanel?.SetActive(false);
            adCoinsPanel?.SetActive(false);
            _metaInfoContainer?.SetActive(true);
            UpdateHoldButtonInteractability();
        }
        else if (mode == MetaPanelMode.BasesShow)
        {
            // First, hide the meta filler panel
            metaFillerPanel?.SetActive(false);
            adCoinsPanel?.SetActive(false);

            // Now, animate in the basesShowPanel elements with a slight delay
            basesShowPanel?.SetActive(true);
            
            // Animate meta info container
            if (metaInfoAnimator != null)
            {
                _metaInfoContainer.SetActive(true);
                metaInfoAnimator.ScaleUp(null);
            }
            
            // Animate left and right buttons simultaneously after a short delay
            if (leftButtonAnimator != null)
            {
                leftButtonAnimator.ScaleUp(null);
            }

            if (rightButtonAnimator != null)
            {
                rightButtonAnimator.ScaleUp(null);
            }

            // Animate close button after another short delay
            if (closeButtonAnimator != null)
            {
                closeButtonAnimator.ScaleUp(null);
            }
        }
    }

    public void UpdateMetaProgress(string metaName, int current, int total)
    {
        if (_metaNameText != null)
        {
            _metaNameText.text = $"{metaName} {current}/{total}";
        }
    }

    public void CloseBasePanel()
    {
        // Animate out the panels first
        List<ScaleAnimationHandler> animatorsToHide = new List<ScaleAnimationHandler>();
        if (leftButtonAnimator != null && leftButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(leftButtonAnimator);
        if (rightButtonAnimator != null && rightButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(rightButtonAnimator);
        if (closeButtonAnimator != null && closeButtonAnimator.gameObject.activeSelf) animatorsToHide.Add(closeButtonAnimator);
        if (metaInfoAnimator != null && metaInfoAnimator.gameObject.activeSelf) animatorsToHide.Add(metaInfoAnimator);

        int totalAnimations = animatorsToHide.Count;
        if (totalAnimations == 0)
        {
            OnAllHideAnimationsComplete();
            return;
        }

        int completedAnimations = 0;
        Action onComplete = () =>
        {
            completedAnimations++;
            if (completedAnimations >= totalAnimations)
            {
                OnAllHideAnimationsComplete();
            }
        };

        foreach (var animator in animatorsToHide)
        {
            animator.ScaleDown(onComplete);
        }
    }
    
    private void OnAllHideAnimationsComplete()
    {
        // showcase meta deactivation
        //GameManager.Instance.metaManager.DeactivateLoadedMeta();
        GameManager.Instance.metaManager.DeactivateShowcaseMeta();
        StartCoroutine(HideAllAfterTime());
    }

    private IEnumerator HideAllAfterTime()
    {
        yield return new WaitForSecondsRealtime(GameManager.Instance.metaManager._fadeDuration);
        base.Hide();
        GameManager.Instance.levelManager._currentLevelObj.SetActive(true);
        GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
    }
    
    private void UpdateHoldButtonInteractability()
    {
        if (_holdToBuildBtn != null)
        {
            _holdToBuildBtn.interactable = metaFillerPanel.activeSelf && !GameManager.Instance.metaManager._currentMetaController.IsProcessingRepair();
        }
    }

    private void OnDisable()
    {
        _isHolding = false;
    }
    
    public void ShowAdCoinsOption()
    {
        if(_metaInfoContainer != null) _metaInfoContainer.SetActive(false);
        if (metaFillerPanel != null) metaFillerPanel.SetActive(false);
        if (basesShowPanel != null) basesShowPanel.SetActive(false);
        if (adCoinsPanel != null) adCoinsPanel.SetActive(true);

        // Set initial scale to zero
        if (get200CoinsBtn != null) get200CoinsBtn.transform.localScale = Vector3.zero;
        if (noThanksbtn != null) noThanksbtn.transform.localScale = Vector3.zero;
        
        // Set initial alpha to 0 for fade in
        if (get200CoinBG != null)
        {
            Color color = get200CoinBG.color;
            color.a = 0f;
            get200CoinBG.color = color;
        }

        // --- FIXED: Use a Sequence to manage the animations ---
        Sequence adPanelSequence = DOTween.Sequence();
        
        adPanelSequence.Append(get200CoinBG.DOFade(1f, 0.5f).SetEase(Ease.Linear));
        
        adPanelSequence.Append(get200CoinsBtn.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        
        adPanelSequence.AppendInterval(0.5f); // Delay before the next button
        
        adPanelSequence.Append(noThanksbtn.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    public void HideAdCoinsOption()
    {
        if (isNothanks)
        {
            isNothanks = false;
            if (_metaInfoContainer != null) _metaInfoContainer.SetActive(false);
            if (adCoinsPanel != null) adCoinsPanel.SetActive(false);
            if (metaFillerPanel != null) metaFillerPanel.SetActive(false);
            if (basesShowPanel != null) basesShowPanel.SetActive(false);
        }
        else
        {
            if (_metaInfoContainer != null) _metaInfoContainer.SetActive(true);
            if (adCoinsPanel != null) adCoinsPanel.SetActive(false);
            if (metaFillerPanel != null) metaFillerPanel.SetActive(true);
            if (basesShowPanel != null) basesShowPanel.SetActive(false);
        }
    }
   
    private void OnGet200CoinsClicked()
    {
        AdsCaller.Instance.ShowRewardedAd((() =>
        {
            StartCoroutine(GameManager.Instance.metaManager.WatchAdForCoins());
        
        }));
    }

    private void OnNoThanksClicked()
    {
        isNothanks = true;
        GameManager.Instance.metaManager.ProceedToNextLevel();
    }
}