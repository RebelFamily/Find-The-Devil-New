using UnityEngine;
using DG.Tweening;

public class PressAndReleaseAnimator : MonoBehaviour
{
    [Header("Waiting State (Loop)")]
    [Tooltip("The vertical distance the object moves down while waiting.")]
    [SerializeField] private float waitDistance = -0.1f;
    [Tooltip("The total duration of one full wait cycle.")]
    [SerializeField] private float waitDuration = 1.2f;
    [Tooltip("The scale to squash to while waiting.")]
    [SerializeField] private Vector3 waitSquashScale = new Vector3(1.1f, 0.9f, 1.1f);
    [Tooltip("The easing for the waiting animation.")]
    [SerializeField] private Ease waitEase = Ease.InOutSine;

    [Header("Release State (One-Shot)")]
    [Tooltip("The vertical distance the object overshoots when released.")]
    [SerializeField] private float releaseBounceHeight = 0.5f;
    [Tooltip("The duration of the release animation.")]
    [SerializeField] private float releaseDuration = 0.6f;
    [Tooltip("The easing for the elastic release animation.")]
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Sequence _currentSequence;

    void Awake()
    {
        // Store the original local position and scale
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// Starts the continuous, rhythmic waiting animation.
    /// Call this when the user presses a button or a process begins.
    /// </summary>
    public void StartWaitingAnimation()
    {
        // Kill any existing sequence to prevent conflicts
        _currentSequence?.Kill();

        // Create a new sequence for the waiting state
        _currentSequence = DOTween.Sequence();

        // Animate down and squash simultaneously
        _currentSequence.Append(transform.DOLocalMoveY(_originalPosition.y + waitDistance, waitDuration / 2f).SetEase(waitEase));
        _currentSequence.Join(transform.DOScale(waitSquashScale, waitDuration / 2f).SetEase(waitEase));

        // Animate back up to the original position and scale
        _currentSequence.Append(transform.DOLocalMoveY(_originalPosition.y, waitDuration / 2f).SetEase(waitEase));
        _currentSequence.Join(transform.DOScale(_originalScale, waitDuration / 2f).SetEase(waitEase));

        // Set the sequence to loop indefinitely
        _currentSequence.SetLoops(-1, LoopType.Restart);
    }

    /// <summary>
    /// Stops the waiting animation and plays a satisfying one-shot release animation.
    /// Call this when the process is complete.
    /// </summary>
    public void StopAndRelease()
    {
        // Stop the waiting loop immediately
        _currentSequence?.Kill();
        
        // Ensure the object is in a squashed state before starting the release animation
        transform.localPosition = new Vector3(_originalPosition.x, _originalPosition.y + waitDistance, _originalPosition.z);
        transform.localScale = waitSquashScale;

        // Create a new sequence for the release state
        _currentSequence = DOTween.Sequence();
        
        // Animate up with an overshoot and back to the original position
        _currentSequence.Append(transform.DOLocalMoveY(_originalPosition.y, releaseDuration).SetEase(releaseEase));
        _currentSequence.Join(transform.DOScale(_originalScale, releaseDuration).SetEase(releaseEase));
    }
    
    /// <summary>
    /// Resets the object to its original position and scale.
    /// </summary>
    public void ResetToOriginalState()
    {
        _currentSequence?.Kill();
        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
    }

    private void OnDisable()
    {
        // Best practice: Kill the sequence when the object is disabled
        _currentSequence?.Kill();
    }
}