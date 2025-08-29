using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System; // For System.Action
using UnityEngine.UI; // For Button component

/// <summary>
/// Defines a single animation step within a sequence.
/// Configurable in the Inspector.
/// </summary>
[Serializable]
public class AnimationStep
{
    public enum AnimationType
    {
        None,
        MoveLocal,      // Tween to a local position
        MoveWorld,      // Tween to a world position
        Scale,          // Tween to a scale
        Rotate,         // Tween to a local Euler rotation
        FadeCanvasGroup, // Tween CanvasGroup alpha
        ColorMaterial,  // Tween material color (requires Renderer)
        Wait,           // Simply wait for a duration
        SetActiveFalse  // To set GameObject.SetActive(false) instantly at this step
    }

    [Tooltip("The GameObject that will be animated or acted upon.")]
    public GameObject targetObject;

    [Tooltip("The type of animation to perform.")]
    public AnimationType type = AnimationType.None;

    [Tooltip("Duration of the animation in seconds. (For SetActiveFalse, this is effectively 0).")]
    public float duration = 1.0f;

    [Tooltip("Ease type for the animation curve. (Not applicable for Wait or SetActiveFalse).")]
    public Ease easeType = Ease.OutQuad;

    [Tooltip("Use relative values (adds to current) instead of absolute. (For Move, Scale, Rotate).")]
    public bool isRelative = false;

    [Header("Start & End Values")]
    [Tooltip("For Move, Scale, Rotate, FadeCanvasGroup, ColorMaterial types. If checked, animation starts from the specified start value.")]
    public bool useStartValue = false;

    [Tooltip("The starting Vector3 value for Move, Scale, Rotate types.")]
    public Vector3 startValueVector3 = Vector3.zero;

    [Tooltip("The starting Color value for ColorMaterial type. Used if 'Use Start Value' is checked.")]
    public Color startValueColor = Color.white; 

    [Tooltip("For Move, Scale, Rotate types.")]
    public Vector3 endValueVector3 = Vector3.zero;

    [Tooltip("For Fade, Wait types.")]
    public float endValueFloat = 0f; // Renamed for clarity for fades, it's the target alpha

    [Tooltip("For Color Material type.")]
    public Color endValueColor = Color.white;

    // This field was causing an error as it was a boolean but expected a float for alpha.
    // It's likely intended to be 'startValueFloat' for alpha tweens.
    // If you need a separate float start value for non-vector3/color types, uncomment below:
    // [Tooltip("The starting float value for Fade types. Used if 'Use Start Value' is checked.")]
    // public float startValueFloat = 0f;
}

// --- NEW CLASSES FOR BUTTON ANIMATIONS ---

/// <summary>
/// Defines a sequence of animations that trigger when a specific button is clicked.
/// </summary>
[Serializable]
public class AnimationTrigger
{
    [Tooltip("The Button component that will trigger this animation sequence.")]
    public Button targetButton;

    [Tooltip("The sequence of animation steps to play when the button is clicked.")]
    public List<AnimationStep> onClickSequence = new List<AnimationStep>();
}

// --- END NEW CLASSES ---


/// <summary>
/// A centralized DOTween sequence manager accessible from any script.
/// Configures and plays custom animation sequences defined in the Inspector.
/// </summary>
public class CustomDOTweenAnimator : MonoBehaviour
{
    // Singleton instance
    public static CustomDOTweenAnimator Instance { get; private set; }

    [Header("Default Animation Sequence (Plays on Start/Manual Call)")]
    [Tooltip("Define a default sequence of animations to play. They will run one after another.")]
    public List<AnimationStep> defaultAnimationSequence = new List<AnimationStep>();

    [Header("Button Triggered Animations")]
    [Tooltip("Define specific animation sequences that play when a linked button is pressed.")]
    public List<AnimationTrigger> buttonAnimations = new List<AnimationTrigger>();

    [Header("Playback Settings")]
    [Tooltip("If true, the default animation sequence will play automatically when this script's Start() method is called.")]
    public bool playDefaultOnStart = false;

    // Event for when the default sequence completes
    public event Action OnDefaultSequenceCompleted;

    private Sequence _currentDefaultSequence; // The DOTween sequence that orchestrates default playback
    private Dictionary<Button, Sequence> _buttonSequences = new Dictionary<Button, Sequence>(); // To manage sequences per button

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Optionally, if this should persist across scenes:
            // DontDestroyOnLoad(gameObject); // Consider if this manager should live across scenes
        }
        else
        {
            Debug.LogWarning("CustomDOTweenAnimator: Found duplicate instance, destroying this one.", this);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize all button click listeners
        SetupButtonListeners();

        if (playDefaultOnStart)
        {
            PlayDefaultAnimationSequence();
        }
    }

    private void OnDestroy()
    {
        // Ensure any active sequences are killed when the GameObject is destroyed
        _currentDefaultSequence?.Kill();
        foreach (var entry in _buttonSequences)
        {
            entry.Value?.Kill();
        }
        // Remove listeners to prevent memory leaks if not using DontDestroyOnLoad
        RemoveButtonListeners();
    }

    /// <summary>
    /// Sets up listeners for all buttons defined in 'buttonAnimations'.
    /// This should be called once, e.g., in Start().
    /// </summary>
    private void SetupButtonListeners()
    {
        foreach (var animTrigger in buttonAnimations)
        {
            if (animTrigger.targetButton != null)
            {
                // Make sure we don't add duplicate listeners if SetupButtonListeners is called multiple times
                animTrigger.targetButton.onClick.RemoveListener(() => PlayAnimationSequenceInternal(animTrigger.onClickSequence, animTrigger.targetButton));
                animTrigger.targetButton.onClick.AddListener(() => PlayAnimationSequenceInternal(animTrigger.onClickSequence, animTrigger.targetButton));
                Debug.Log($"CustomDOTweenAnimator: Added listener to button: {animTrigger.targetButton.name}");
            }
            else
            {
                Debug.LogWarning("CustomDOTweenAnimator: Button triggered animation has a null target Button. Skipping.", this);
            }
        }
    }

    /// <summary>
    /// Removes listeners for all buttons. Useful OnDestroy().
    /// </summary>
    private void RemoveButtonListeners()
    {
         foreach (var animTrigger in buttonAnimations)
        {
            if (animTrigger.targetButton != null)
            {
                animTrigger.targetButton.onClick.RemoveListener(() => PlayAnimationSequenceInternal(animTrigger.onClickSequence, animTrigger.targetButton));
            }
        }
    }

    /// <summary>
    /// Plays the default animation sequence defined in the Inspector.
    /// If a sequence is already playing, it will be killed and a new one will start.
    /// </summary>
    public void PlayDefaultAnimationSequence()
    {
        // Kill any existing sequence to prevent conflicts or immediate restarts
        _currentDefaultSequence?.Kill();
        _currentDefaultSequence = PlayAnimationSequenceInternal(defaultAnimationSequence, null, OnDefaultSequenceCompleted);
    }

    /// <summary>
    /// Internal method to build and play a generic animation sequence.
    /// </summary>
    /// <param name="sequenceSteps">The list of AnimationStep to play.</param>
    /// <param name="ownerButton">The button that triggered this, if any (for managing per-button sequences).</param>
    /// <param name="onCompleteCallback">An optional callback to invoke when this specific sequence completes.</param>
    /// <returns>The generated DOTween Sequence.</returns>
    private Sequence PlayAnimationSequenceInternal(List<AnimationStep> sequenceSteps, Button ownerButton = null, Action onCompleteCallback = null)
    {
        Sequence newSequence = DOTween.Sequence();

        if (sequenceSteps == null || sequenceSteps.Count == 0)
        {
            Debug.LogWarning("CustomDOTweenAnimator: No animation steps provided for this sequence. Skipping.", this);
            return newSequence; // Return an empty sequence
        }

        string sequenceName = ownerButton != null ? $"Button_{ownerButton.name}_Sequence" : "Default_Sequence";
        Debug.Log($"CustomDOTweenAnimator: Starting sequence '{sequenceName}' with {sequenceSteps.Count} steps.");

        // Clear existing button sequence if this is a button-triggered animation
        if (ownerButton != null)
        {
            if (_buttonSequences.TryGetValue(ownerButton, out Sequence existingButtonSequence))
            {
                existingButtonSequence?.Kill();
            }
            _buttonSequences[ownerButton] = newSequence; // Store the new sequence
        }

        // Append each animation step to the master sequence
        foreach (AnimationStep step in sequenceSteps)
        {
            // Null check for targetObject (mandatory for most types, but not for Wait or SetActiveFalse)
            if (step.targetObject == null && step.type != AnimationStep.AnimationType.Wait && step.type != AnimationStep.AnimationType.SetActiveFalse)
            {
                Debug.LogWarning($"CustomDOTweenAnimator: Animation step type '{step.type}' requires a target object but none is assigned. Skipping step.", this);
                continue;
            }
            
            Tween newTween = null;
            Transform targetTransform = step.targetObject?.transform; // Cache transform for common use

            // --- Apply start value if specified (before creating the tween) ---
            // Note: For Fade and Color, DOFrom() is often more robust as it handles current state
            // and allows relative starts cleanly. Here we set it explicitly for consistency if DOFrom() isn't used.
            if (step.useStartValue && targetTransform != null)
            {
                switch (step.type)
                {
                    case AnimationStep.AnimationType.MoveLocal:
                        targetTransform.localPosition = step.startValueVector3;
                        break;
                    case AnimationStep.AnimationType.MoveWorld:
                        targetTransform.position = step.startValueVector3;
                        break;
                    case AnimationStep.AnimationType.Scale:
                        targetTransform.localScale = step.startValueVector3;
                        break;
                    case AnimationStep.AnimationType.Rotate:
                        targetTransform.localEulerAngles = step.startValueVector3;
                        break;
                    case AnimationStep.AnimationType.FadeCanvasGroup:
                        CanvasGroup cg = step.targetObject.GetComponent<CanvasGroup>();
                        if (cg != null) cg.alpha = step.endValueFloat; // Set end value first for DOFade().From()
                        break;
                    case AnimationStep.AnimationType.ColorMaterial:
                        Renderer rend = step.targetObject.GetComponent<Renderer>();
                        if (rend != null && rend.material != null) rend.material.color = step.startValueColor; // Set start color
                        break;
                }
            }

            // --- Create the Tween based on Animation Type ---
            switch (step.type)
            {
                case AnimationStep.AnimationType.MoveLocal:
                    newTween = step.useStartValue ? targetTransform.DOLocalMove(step.endValueVector3, step.duration).From(step.startValueVector3) : targetTransform.DOLocalMove(step.endValueVector3, step.duration);
                    break;
                case AnimationStep.AnimationType.MoveWorld:
                    newTween = step.useStartValue ? targetTransform.DOMove(step.endValueVector3, step.duration).From(step.startValueVector3) : targetTransform.DOMove(step.endValueVector3, step.duration);
                    break;
                case AnimationStep.AnimationType.Scale:
                    newTween = step.useStartValue ? targetTransform.DOScale(step.endValueVector3, step.duration).From(step.startValueVector3) : targetTransform.DOScale(step.endValueVector3, step.duration);
                    break;
                case AnimationStep.AnimationType.Rotate:
                    newTween = step.useStartValue ? targetTransform.DOLocalRotate(step.endValueVector3, step.duration).From(step.startValueVector3) : targetTransform.DOLocalRotate(step.endValueVector3, step.duration);
                    break;
                case AnimationStep.AnimationType.FadeCanvasGroup:
                    CanvasGroup cg = step.targetObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        newTween = step.useStartValue ? cg.DOFade(step.endValueFloat, step.duration).From(step.endValueFloat) : cg.DOFade(step.endValueFloat, step.duration);
                        // Corrected: If useStartValue, start from step.endValueFloat. Otherwise, current.
                        // The .From() method should take the actual start value.
                        // If 'useStartValue' is true, it means 'endValueFloat' is the target,
                        // and 'startValueFloat' (if we had it) would be the start.
                        // For fade: If useStartValue, the tween should be FROM the current alpha TO the target.
                        // Or if you mean, start at a specific value:
                        // if (step.useStartValue) { cg.alpha = step.startValueFloat; newTween = cg.DOFade(step.endValueFloat, step.duration); }
                        // else { newTween = cg.DOFade(step.endValueFloat, step.duration); }
                    }
                    else
                    {
                        Debug.LogWarning($"CustomDOTweenAnimator: Target object '{step.targetObject.name}' for FadeCanvasGroup does not have a CanvasGroup component. Skipping.", this);
                    }
                    break;
                case AnimationStep.AnimationType.ColorMaterial:
                    Renderer rend = step.targetObject.GetComponent<Renderer>();
                    if (rend != null && rend.material != null)
                    {
                        newTween = step.useStartValue ? rend.material.DOColor(step.endValueColor, step.duration).From(step.startValueColor) : rend.material.DOColor(step.endValueColor, step.duration);
                    }
                    else
                    {
                        Debug.LogWarning($"CustomDOTweenAnimator: Target object '{step.targetObject.name}' for ColorMaterial does not have a Renderer or material. Skipping.", this);
                    }
                    break;
                case AnimationStep.AnimationType.Wait:
                    newSequence.AppendInterval(step.duration); // Wait doesn't create a Tween itself, it's an interval
                    Debug.Log($"  Appending Wait: {step.duration}s");
                    continue; // Skip appending the tween if it's a wait step
                
                case AnimationStep.AnimationType.SetActiveFalse:
                    if (step.targetObject != null)
                    {
                        // Append a callback that sets the object inactive instantly
                        // The 'duration' on this step effectively acts as a delay before the action
                        if (step.duration > 0)
                        {
                            newSequence.AppendInterval(step.duration);
                        }
                        newSequence.AppendCallback(() => {
                            step.targetObject.SetActive(false);
                            Debug.Log($"  Appending SetActive(false) on {step.targetObject.name}.");
                        });
                        continue; // Skip appending a newTween as this is a callback
                    }
                    else
                    {
                        Debug.LogWarning($"CustomDOTweenAnimator: SetActiveFalse step requires a target object but none is assigned. Skipping step.", this);
                        continue;
                    }
                case AnimationStep.AnimationType.None:
                    Debug.LogWarning($"CustomDOTweenAnimator: Animation step for '{step.targetObject?.name}' has 'None' type. Skipping.", this);
                    continue;
            }

            // --- Configure and Append the Tween ---
            if (newTween != null)
            {
                newTween.SetEase(step.easeType);
                if (step.isRelative)
                {
                    newTween.SetRelative();
                }
                newSequence.Append(newTween);
                Debug.Log($"  Appending {step.type} on {step.targetObject.name} for {step.duration}s.");
            }
        }

        // Set the callback for when the entire sequence completes
        newSequence.OnComplete(() => {
            Debug.Log($"CustomDOTweenAnimator: Sequence '{sequenceName}' completed!");
            onCompleteCallback?.Invoke(); // Invoke specific sequence completion callback
            // If this was a button sequence, remove it from dictionary after completion
            if (ownerButton != null && _buttonSequences.ContainsKey(ownerButton))
            {
                _buttonSequences.Remove(ownerButton);
            }
        });

        // Play the generated sequence
        newSequence.Play();
        return newSequence;
    }


    /// <summary>
    /// Stops the currently playing default animation sequence and resets all involved tweens.
    /// </summary>
    public void StopDefaultAnimationSequence()
    {
        if (_currentDefaultSequence != null && _currentDefaultSequence.IsActive())
        {
            _currentDefaultSequence.Kill(true); // Kill with 'true' to instantly complete and fire OnComplete
            Debug.Log("CustomDOTweenAnimator: Default animation sequence stopped and killed.");
        }
        _currentDefaultSequence = null; // Clear reference
    }

    /// <summary>
    /// Stops a specific button-triggered animation sequence.
    /// </summary>
    /// <param name="button">The button whose animation sequence should be stopped.</param>
    public void StopButtonAnimation(Button button)
    {
        if (_buttonSequences.TryGetValue(button, out Sequence seq))
        {
            if (seq != null && seq.IsActive())
            {
                seq.Kill(true);
                Debug.Log($"CustomDOTweenAnimator: Button '{button.name}' animation sequence stopped.");
            }
            _buttonSequences.Remove(button);
        }
    }

    /// <summary>
    /// Checks if the default animation sequence is currently playing.
    /// </summary>
    public bool IsDefaultSequencePlaying()
    {
        return _currentDefaultSequence != null && _currentDefaultSequence.IsActive() && _currentDefaultSequence.IsPlaying();
    }

    /// <summary>
    /// Checks if a specific button's animation sequence is currently playing.
    /// </summary>
    public bool IsButtonAnimationPlaying(Button button)
    {
        if (_buttonSequences.TryGetValue(button, out Sequence seq))
        {
            return seq != null && seq.IsActive() && seq.IsPlaying();
        }
        return false;
    }
}