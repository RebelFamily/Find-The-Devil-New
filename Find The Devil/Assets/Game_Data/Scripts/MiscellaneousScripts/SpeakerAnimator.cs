using UnityEngine;
using DG.Tweening;

public class SpeakerAnimator : MonoBehaviour
{
    [Tooltip("The transform of the speaker part to animate (e.g., the speaker cone).")]
    public Transform speakerCone;

    [Header("Animation Settings")]
    [Tooltip("The local Y-axis distance the speaker will move upward.")]
    public float movementDistance = 0.1f;

    [Tooltip("The ease type for the pulse animation.")]
    public Ease pulseEase = Ease.OutSine;

    private Sequence pulseSequence;
    private Vector3 originalLocalPosition;

    void Start()
    {
        if (speakerCone == null)
        {
            Debug.LogWarning("Speaker cone transform is not assigned on " + gameObject.name + ". Disabling speaker animation.");
            this.enabled = false;
            return;
        }

        originalLocalPosition = speakerCone.localPosition;
        CreateSpeakerPulseAnimation();
    }

    private void CreateSpeakerPulseAnimation()
    {
        // Kill any existing tween to prevent conflicts
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }

        // Create a new DOTween sequence
        pulseSequence = DOTween.Sequence();

        // Animate the local position of the speaker
        // It will move to a new position (e.g., up) and then back to the original position (e.g., down)
        pulseSequence.Append(speakerCone.DOLocalMoveY(originalLocalPosition.y + movementDistance,  0.15f).SetEase(pulseEase));
        pulseSequence.Append(speakerCone.DOLocalMoveY(originalLocalPosition.y,   0.15f).SetEase(pulseEase));

        // Make the animation loop indefinitely
        pulseSequence.SetLoops(-1, LoopType.Yoyo);

        // Start the animation
        pulseSequence.Play();
    }

    private void OnDisable()
    {
        // Clean up the tween when the object is disabled to avoid memory leaks
        if (pulseSequence != null)
        {
            pulseSequence.Kill();
        }
    }
}