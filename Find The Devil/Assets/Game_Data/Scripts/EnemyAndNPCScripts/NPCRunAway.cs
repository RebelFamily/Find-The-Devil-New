using System;
using UnityEngine;
using System.Collections;
using Dreamteck.Splines;
using DG.Tweening; // Make sure to include DOTween namespace

public class NPCRunAway : MonoBehaviour
{
    [SerializeField] private float runawaySpeed = 5.0f;
    [SerializeField] private SplineComputer _runawaySpline;
    [SerializeField] private SplineFollower _splineFollower;
    [SerializeField] private float despawnDelay = 2.0f;
    [SerializeField] private GameObject _objToDisapear;
    
    private Animator animator;
    private double currentSplinePercent = 0.0;
    private bool isRunningAway = false;
    private Coroutine _poleDanceCoroutine;

    public int enemyCountCheck = 1;
    
    [Header("------ Level based Checks ------")]
    [SerializeField] private bool isFightLevel = false;
    [SerializeField] private Animator runAnimator;
    [SerializeField] private RuntimeAnimatorController afterFightAnimation;

    [Header("------ Level based Checks ------")]
    public bool isRunningINCircle = false;

    [Header("------ Level based Checks ------")]
    [SerializeField] private bool isPoliceCarLevel = false;

    [Header("------ Level based Checks ------")] 
    [SerializeField] private bool isKingDevilRunaway = false;
    [SerializeField] private Transform flyAwayTransform;
    
    
    [Header("------ Level based Checks ------")] 
    public bool isDevilRunTowardsPlayer = false;
    
    
    [Header("------ Level based Checks ------")] 
     public bool isDevilPoleDancePlayer = false;
    [SerializeField] private Transform dancingEndPos;
    [SerializeField] private GameObject objToAnimate;
    [SerializeField] private float poleDanceMoveDuration = 1.5f; // Duration for moving to the pole dance position
    [SerializeField] private float poleDanceRotateDuration = 1.5f; // Duration for rotating during the pole dance move
    [SerializeField] private int poleDanceRotateLoops = 3; // How many times to rotate during the move
    [SerializeField] private Animator poleNPCAnimation;
    [SerializeField] private RuntimeAnimatorController slowrunAnimation;
    [SerializeField] private RuntimeAnimatorController poleDanceAnimation;
    
    
    [Header("------ Level based Checks ------")] 
     public bool isWaiterCafeLevel = false; 
    [SerializeField] private Animator waiterAnimator;
    [SerializeField] private Animator waiterSkeletonrAnimator;
    [SerializeField] private RuntimeAnimatorController walkAnimation;
    [SerializeField] private RuntimeAnimatorController orderTakingAnimation;
    [SerializeField] private SplineComputer alternateSpline;
    [SerializeField] private LevelPhaseManager _levelPhaseManager;
    public bool playAlternateSpline;
    
    [Header("------ Level based Checks ------")] 
    [SerializeField] private bool isNPCFallBackPlayer = false;
    [SerializeField] private RuntimeAnimatorController fallBackAnimation;
    [SerializeField] private Animator fallingNPCAnimator;
    [SerializeField] private GameObject bananaPeel;
    
    [Header("------ Level based Checks ------")] 
    [SerializeField] private bool isOldNPCRunPlayer = false;
    [SerializeField] private Animator oldNPCAnimator;
    [SerializeField] private RuntimeAnimatorController ooldRunAnimation;

    [Header("------ Level based Checks ------")] 
    [SerializeField] private bool isNPCFallDown = false; // New check for falling down
    [SerializeField] private Transform fallDownEndPos; // Target position for falling
    [SerializeField] private GameObject objToFallAnimate; // Object that will fall and animate
    [SerializeField] private Animator fallNPCAnimator; // Animator for the falling NPC
    [SerializeField] private RuntimeAnimatorController fallingAnimation; // Animation while falling
    [SerializeField] private RuntimeAnimatorController deadAnimation; // Animation after falling (dead)
    [SerializeField] private float fallDownDuration = 1.0f; // Duration for the fall animation

    
    
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        
    }

    private void Start()
    {
        if (isRunningINCircle)
        {
            _splineFollower.spline = _runawaySpline;
            _splineFollower.follow = true;
            _splineFollower.followSpeed = runawaySpeed;
        }
    }

    public void FollowStartSpline()
    {
        _splineFollower.spline = _runawaySpline;
        _splineFollower.follow = true;
    }

    public void HandleRunawayReaction()
    {
       
            StartCoroutine(FollowSplinePath());
       
    }
    
    public void StopPoleDance()
    {
        if (_poleDanceCoroutine != null)
        {
            StopCoroutine(_poleDanceCoroutine);
            _poleDanceCoroutine = null;
            // Also stop any DOTween animations on the object
            if (objToAnimate != null)
            {
                objToAnimate.transform.DOKill();
            }
            // Resume spline following or reset to a default state
            _splineFollower.follow = true;
            isDevilPoleDancePlayer = false;
            isDevilRunTowardsPlayer = false;
            if (poleNPCAnimation != null && slowrunAnimation != null)
            {
               // poleNPCAnimation.runtimeAnimatorController = slowrunAnimation;
            }
        }
    }

    private IEnumerator PerformPoleDanceAndRunaway()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        poleNPCAnimation.runtimeAnimatorController = poleDanceAnimation;
        // Stop any current spline following temporarily
        _splineFollower.follow = false;
        Debug.Log("PerformPoleDanceAndRunaway()");
        // Animate position to dancingEndPos
        objToAnimate.transform.DOMove(dancingEndPos.position, poleDanceMoveDuration).SetEase(Ease.Linear);
        
        // Animate rotation around Y-axis simultaneously
        objToAnimate.transform.DORotate(new Vector3(0, 360 * poleDanceRotateLoops, 0), poleDanceRotateDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear);

        yield return new WaitForSeconds(Mathf.Max(poleDanceMoveDuration, poleDanceRotateDuration));
        if (isDevilPoleDancePlayer)
        {
            isDevilPoleDancePlayer = false;
            poleNPCAnimation.runtimeAnimatorController = slowrunAnimation;
            
        }
        
        
        DevilRunTowardsPlayer();
        
    }
    
   private IEnumerator FollowSplinePath()
   {
       if (isKingDevilRunaway)
       {
           
           transform.DOMove(flyAwayTransform.position, 10f);
           yield break;
       }

       if (isRunningINCircle )
       {
           isRunningINCircle = false;
       }
       
        _splineFollower.spline = _runawaySpline;
        _splineFollower.follow = true;
        _splineFollower.followSpeed = runawaySpeed;
        
        CheckForLevelBasedBehaviours();
        yield return new WaitForSeconds(0.1f);
    }

    public void CheckForLevelBasedBehaviours()
    {

        if (isFightLevel)
        {
            StartCoroutine(SetNewMovemntSpeedAfter(3f));
        }
        
        if (isOldNPCRunPlayer)
        {
               oldNPCAnimator.runtimeAnimatorController = ooldRunAnimation;
               oldNPCAnimator.speed = 1f; 
        }
        
        if (isPoliceCarLevel)
        {
            GetComponent<CharacterReactionHandler>().TriggerSpecificReaction(NPCReactionType.Hide);
        }

    
        if ( _levelPhaseManager != null && _levelPhaseManager.isWaiterlevelCheck)
        {
            _splineFollower.spline = alternateSpline;
            _objToDisapear.SetActive(false);
            _splineFollower.Restart();
            _splineFollower.followSpeed = 4;
        }
        
        if(isWaiterCafeLevel && playAlternateSpline)
        {
            
            _objToDisapear.SetActive(false);
            _splineFollower.Restart();
            _splineFollower.followSpeed = 4;
           
        }
        
    }
    public void BaloonStoreDisapear()
    {
        _objToDisapear.transform.SetParent(this.transform);
    }

    public void CloseSpline()
    {
           
        Debug.Log("Close spline = " + enemyCountCheck);
        if (enemyCountCheck > 0)
        {
            _splineFollower.follow = false;
        }
    }

    public void DevilRunTowardsPlayer()
    {
        if (isDevilPoleDancePlayer && objToAnimate != null && dancingEndPos != null)
        {
            Debug.Log("PerformPoleDanceAndRunaway()");
            // Store the coroutine reference
            _poleDanceCoroutine = StartCoroutine(PerformPoleDanceAndRunaway());
        }
        else
        {
            Debug.Log("DevilRunTowardsPlayer()");
            if (isDevilRunTowardsPlayer)
            {
                _splineFollower.spline = _runawaySpline;
                _splineFollower.follow = true;
            }
        }
    }

    public void SetNewMovementSpeed(float value)
    {
        StartCoroutine(SetNewMovemntSpeedAfter(value));

    }

    public void SetWaiterOrderTaking()
    {
        waiterSkeletonrAnimator.runtimeAnimatorController = orderTakingAnimation;
        waiterAnimator.runtimeAnimatorController = orderTakingAnimation;
    }
    
    private IEnumerator SetNewMovemntSpeedAfter(float value)
    {
        yield return new WaitForSecondsRealtime(4);

        if (isFightLevel)
        {
            _splineFollower.followSpeed = value;
        }
        
        if (isNPCFallBackPlayer)
        {
            _splineFollower.followSpeed = value;
           yield break;
        }
        SetAlternateSpline(true);
        
        if (isWaiterCafeLevel && playAlternateSpline && !_levelPhaseManager.isWaiterlevelCheck)
        {
            Debug.Log("SetNewMovemntSpeedAfter(float value)");
            waiterSkeletonrAnimator.runtimeAnimatorController = walkAnimation;
            waiterAnimator.runtimeAnimatorController = walkAnimation;
            
            _splineFollower.followSpeed = value;
           
        }
        else
        {
            _splineFollower.spline = alternateSpline;
        }
    }

    public void PlayFallBackAnimation()
    {
        
        if (isNPCFallBackPlayer)
        {
            GameManager.Instance.audioManager.StopSFX();
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Character_Slip);
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Character_Falldown,1f);
            
            _splineFollower.followSpeed = 0;
            if (fallingNPCAnimator != null && fallBackAnimation != null)
            {
                fallingNPCAnimator.runtimeAnimatorController = fallBackAnimation;
                fallingNPCAnimator.speed = 1f; 
                AnimateBananaPeel(bananaPeel);
                if (_splineFollower != null)
                {
                    StartCoroutine(SetNewMovemntSpeedAfter(1.5f));
                }
                
            }  
            else  
            {
                Debug.LogWarning("NPCRunAway: Falling NPC Animator or Fall Back Animation not assigned for PlayFallBackAnimation.");
            }
        }
    }

    private void AnimateBananaPeel(GameObject peel)
    {
        if (peel == null)
        {
            Debug.LogWarning("Banana Peel object is not assigned. Cannot animate.");
            return;
        }
    
        // Create a Sequence to chain multiple animations
        Sequence sequence = DOTween.Sequence();
    
        // Animate the banana peel's movement with a parabolic arc (jump)
        sequence.Append(peel.transform.DOJump(
            peel.transform.position + new Vector3(1f, 0f, 0f), // End position slightly up and to the side
            2.8f,                                                 // Jump power (height)
            1,                                                  // Number of jumps
            1.25f                                                // Duration of the jump
        ).SetEase(Ease.OutQuad));                               // Use an easing function for a smoother effect
    
        // Animate a quick rotation while it flies away
        sequence.Join(peel.transform.DORotate(
            new Vector3(0, 360, 0),
            .75f,
            RotateMode.FastBeyond360
        ).SetRelative(true));
    
        // Wait for a short moment, then fade and shrink the peel
        sequence.AppendInterval(0.2f);
        sequence.Append(peel.transform.DOScale(
            Vector3.zero, // Shrink to nothing
            0.3f
        ).SetEase(Ease.InSine));
    
        // When the animation is complete, destroy the banana peel object
        sequence.OnComplete(() => Destroy(peel));
    }
    
    
    // private void AnimateBananaPeel(GameObject peel)
    // {
    //     if (peel == null)
    //     {
    //         Debug.LogWarning("Banana Peel object is not assigned. Cannot animate.");
    //         return;
    //     }
    //
    //     // Create a Sequence to chain multiple animations
    //     Sequence sequence = DOTween.Sequence();
    //
    //     // The current position of the peel when the animation starts.
    //     Vector3 startPosition = peel.transform.position;
    //
    //     // Define the peak of the jump arc.
    //     Vector3 jumpPeak = startPosition + new Vector3(1.2f, 1.5f, 0f);
    //
    //     // 1. Animate the upward arc of the jump.
    //     // We use a regular DOMoveY to get to the peak of the jump.
    //     sequence.Append(peel.transform.DOMoveY(jumpPeak.y, 0.4f)
    //         .SetEase(Ease.OutQuad));
    //
    //     // 2. Animate the forward movement simultaneously with the upward and downward arcs.
    //     sequence.Join(peel.transform.DOMoveX(jumpPeak.x, 0.8f)
    //         .SetEase(Ease.OutQuad));
    //
    //     // 3. Animate the fall back to the ground.
    //     // This starts immediately after the upward movement in the sequence, making the transition seamless.
    //     sequence.Append(peel.transform.DOMoveY(0f, 0.4f)
    //         .SetEase(Ease.InQuad));
    //
    //     // Animate a quick rotation while it flies and falls
    //     sequence.Join(peel.transform.DORotate(
    //         new Vector3(0, 360, 0),
    //         0.8f, // The total duration of the flight and fall combined
    //         RotateMode.FastBeyond360
    //     ).SetRelative(true));
    //
    //     // Wait for a short moment, then fade and shrink the peel
    //     sequence.AppendInterval(0.2f);
    //     sequence.Append(peel.transform.DOScale(
    //         Vector3.zero, // Shrink to nothing
    //         0.3f
    //     ).SetEase(Ease.InSine));
    //
    //     // When the animation is complete, destroy the banana peel object
    //     sequence.OnComplete(() => Destroy(peel));
    // }

    public void IsRunningInCircle()
    {
        if (!isRunningINCircle )
        {
            _splineFollower.follow = false;
        }
    }
    public void SetAlternateSpline(bool value)
    {
        playAlternateSpline = value;
    }
}