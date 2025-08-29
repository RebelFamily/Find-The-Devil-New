using UnityEngine;
using DG.Tweening;
using System;

public class ScaleAnimationHandler : MonoBehaviour
{
    [Header("Activation Animation")]
    [SerializeField] private Vector3 activeTargetScale = Vector3.one; 
    [SerializeField] private float activeAnimationDuration = 0.3f;
    [SerializeField] private float activeAnimationDelay = 0f; // --- NEW: Added delay variable ---
    [SerializeField] private Ease activeEaseType = Ease.OutBack; 

    [Header("Deactivation Animation")]
    [SerializeField] private Vector3 disabledTargetScale = Vector3.zero; 
    [SerializeField] private float disabledAnimationDuration = 0.2f; 
    [SerializeField] private Ease disabledEaseType = Ease.InBack;
    
    [Tooltip("If true, the GameObject will be deactivated after the ScaleDown animation completes.")]
    [SerializeField] private bool hideOnDeactivate = false;

    public void ScaleUp(Action onComplete = null)
    {
        transform.DOKill(true);
        transform.localScale = Vector3.zero;
        transform.DOScale(activeTargetScale, activeAnimationDuration)
            .SetDelay(activeAnimationDelay) // --- NEW: Apply the delay here ---
            .SetEase(activeEaseType)
            .OnComplete(() => onComplete?.Invoke());
    }
    
    public void ScaleDown(Action onComplete = null)
    {
        transform.DOKill(true);
        transform.DOScale(disabledTargetScale, disabledAnimationDuration)
            .SetEase(disabledEaseType)
            .OnComplete(() =>
            {
                if (hideOnDeactivate)
                {
                    gameObject.SetActive(false);
                }
                onComplete?.Invoke();
            });
    }
}