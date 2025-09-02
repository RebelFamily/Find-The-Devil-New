using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class GetVIPPanel : UIPanel
{
    [SerializeField] private RectTransform panelContainer;
    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private Image cardFrontImage;
    [SerializeField] private Image cardBackImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button noThanksButton;
    [SerializeField] private float flipDurationHalf = 0.25f;
    [SerializeField] private float flipDurationFull = 0.25f;
    [SerializeField] private float midFlipYRotation = 90f;
    [SerializeField] private float cardScaleInDuration = 1f;
    [SerializeField] private Ease cardScaleInEase = Ease.OutBack;
    [SerializeField] private float preFlipDelay = 0.5f;
    [SerializeField] private float postFlipButtonDelay = 0.2f;
    [SerializeField] private float panelScaleOutDuration = 0.2f;
    [SerializeField] private Ease panelScaleOutEase = Ease.InBack;

    private bool _isCardFlipping = false;
    private Tween _currentFlipTween;
    private Tween _currentCardScaleTween;
    private Tween _currentPanelScaleTween;

    public override void Show()
    {
        if (panelContainer != null)
        {
            panelContainer.gameObject.SetActive(true);
            panelContainer.localScale = Vector3.one; // Ensure it's at full scale for immediate display
        }

        InitializeCardState();
        StartFlipAnimation();
    }

    public override void Hide()
    {
        if (panelContainer == null)
        {
            panelContainer.gameObject.SetActive(true);
            return;
        }
    }
    public void HideThisPanel()
    {
        //base.Hide();
        if (panelContainer == null)
        {
            return;
        }
        
        _isCardFlipping = true; 
        
        _currentFlipTween?.Kill();
        _currentCardScaleTween?.Kill();
        _currentPanelScaleTween?.Kill(true);

        _currentPanelScaleTween = panelContainer.DOScale(Vector3.zero, panelScaleOutDuration)
            .SetEase(panelScaleOutEase)
            .OnComplete(() =>
            {
                if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                {
                    GameManager.Instance.uiManager.ShowPanel(UIPanelType.MainMenuPanel);
                }
                _isCardFlipping = false;
            });
    }

    private void InitializeCardState()
    {
        if (cardContainer != null)
        {
            cardContainer.gameObject.SetActive(true);
            cardContainer.localScale = Vector3.zero;
            cardContainer.localEulerAngles = Vector3.zero;
        }

        if (cardBackImage != null) cardBackImage.gameObject.SetActive(true);
        if (cardFrontImage != null) cardFrontImage.gameObject.SetActive(false);

        if (buyButton != null) buyButton.gameObject.SetActive(false);
        if (noThanksButton != null) noThanksButton.gameObject.SetActive(false);

        _isCardFlipping = false;
    }

    private void StartFlipAnimation()
    {
        if (cardContainer == null || cardBackImage == null || cardFrontImage == null)
        {
            return;
        }

        _isCardFlipping = true;

        Sequence flipSequence = DOTween.Sequence();

        _currentCardScaleTween = cardContainer.DOScale(Vector3.one, cardScaleInDuration)
            .SetEase(cardScaleInEase).SetDelay(1f);
        flipSequence.Append(_currentCardScaleTween);

        flipSequence.AppendInterval(preFlipDelay);

        _currentFlipTween = cardContainer.DOLocalRotate(new Vector3(0, midFlipYRotation, 0), flipDurationHalf, RotateMode.FastBeyond360)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                if (cardBackImage != null) cardBackImage.gameObject.SetActive(false);
                if (cardFrontImage != null) cardFrontImage.gameObject.SetActive(true);
            });
        flipSequence.Append(_currentFlipTween);

        _currentFlipTween = cardContainer.DOLocalRotate(new Vector3(0, 0, 0), flipDurationFull, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine);
        flipSequence.Append(_currentFlipTween);

        flipSequence.AppendInterval(postFlipButtonDelay);
        flipSequence.AppendCallback(() =>
        {
            if (buyButton != null) buyButton.gameObject.SetActive(true);
            if (noThanksButton != null) noThanksButton.gameObject.SetActive(true);
            _isCardFlipping = false;
        });

        flipSequence.Play();
    }

    public void OnBuyButtonClicked()
    {
        if (_isCardFlipping) return;
        
        IAPManager.Instance.InAppCaller(PurchaseType.VIPGuns);
        
        Hide();
    }

    
    public void OnNoThanksButtonClicked()
    {
        if (_isCardFlipping) return;

        HideThisPanel();
    } 

    void OnDestroy()
    {
        _currentFlipTween?.Kill();
        _currentCardScaleTween?.Kill();
        _currentPanelScaleTween?.Kill();
    }
}