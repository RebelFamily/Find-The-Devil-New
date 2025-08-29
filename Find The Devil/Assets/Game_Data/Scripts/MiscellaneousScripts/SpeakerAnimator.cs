using UnityEngine;
using DG.Tweening;

public class SpeakerAnimator : MonoBehaviour
{
    [Tooltip("The transform of the speaker part to animate (e.g., the speaker cone).")]
    public Transform speakerCone;

    [Header("Animation Settings")]
    [Tooltip("The duration of a single pulse cycle (in and out).")]
    public float pulseDuration = 0.2f;

    [Tooltip("The local Z-axis distance the cone will move during the pulse.")]
    public float movementDistance = 0.5f;

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

        // Animate the local position of the speaker cone
        // It will move to a new position (e.g., in) and then back to the original position (e.g., out)
        pulseSequence.Append(speakerCone.DOLocalMoveZ(originalLocalPosition.z + movementDistance, pulseDuration / 2).SetEase(pulseEase));
        pulseSequence.Append(speakerCone.DOLocalMoveZ(originalLocalPosition.z, pulseDuration / 2).SetEase(pulseEase));

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