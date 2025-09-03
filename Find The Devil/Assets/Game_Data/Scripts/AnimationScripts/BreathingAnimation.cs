using UnityEngine;
using DG.Tweening;

public class BreathingAnimation : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("The duration of one full breath cycle (up and down).")]
    [SerializeField] private float breathingDuration = 2.0f;

    [Tooltip("The vertical distance the object moves from its original position.")]
    [SerializeField] private float breathingIntensity = 0.1f;

    [Tooltip("The type of easing for the animation to make it feel smooth.")]
    [SerializeField] private Ease breathingEase = Ease.InOutSine;

    private Sequence _breathingSequence;

    void Start()
    {
        // Start the breathing animation
        StartBreathing();
    }

    private void StartBreathing()
    {
        // Check if a sequence is already running and kill it to prevent duplicates
        if (_breathingSequence != null && _breathingSequence.IsActive())
        {
            _breathingSequence.Kill();
        }

        // Create a new DOTween sequence
        _breathingSequence = DOTween.Sequence();

        // Animate the object up
        _breathingSequence.Append(transform.DOLocalMoveY(transform.localPosition.y + breathingIntensity, breathingDuration / 2f).SetEase(breathingEase));
        
        // Animate the object back down to its original local position
        _breathingSequence.Append(transform.DOLocalMoveY(transform.localPosition.y, breathingDuration / 2f).SetEase(breathingEase));

        // Set the sequence to loop indefinitely (-1)
        _breathingSequence.SetLoops(-1, LoopType.Restart);
    }
    
    // Call this method to stop the animation
    public void StopBreathing()
    {
        if (_breathingSequence != null)
        {
            _breathingSequence.Kill();
            _breathingSequence = null;
        }
    }

    private void OnDisable()
    {
        StopBreathing();
    }
}