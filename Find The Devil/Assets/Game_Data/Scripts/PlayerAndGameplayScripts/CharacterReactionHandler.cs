using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using DG.Tweening;
using Unity.Mathematics;

using Random = Unity.Mathematics.Random;

public enum NPCReactionType
{
    Electricfy,
    RunAway,
    Hide,
    Duck,
    StopReaction,
    Death,
    Shoot,
    RunTowardsPlayer
}

public class CharacterReactionHandler : MonoBehaviour, IReactable
{
    [Header("OnDeath Coins References")]
    //have to find better alternate add coins
    public int onDeathCoins;

    
    
    [SerializeField] private bool isADemoCharacter;
    
    [SerializeField] private bool isKingDevil;
    [SerializeField] private Transform cutSceneCameraPos;
    [SerializeField] private ExpressionController devilExpressionController;
    [SerializeField] private GameObject devilStunEffects;
    [SerializeField] private GameObject devilWings;
    [SerializeField] private GameObject devilKingRunEffects;
    
    [Header("Form GameObjects")]
    [SerializeField] private GameObject humanForm;
    [SerializeField] private GameObject otherForm;

    [Header("Animator Dependencies")]
    [SerializeField] private Animator humanAnimator;
    [SerializeField] private Animator otherFormAnimator;
    [SerializeField] private RuntimeAnimatorController reactionAnimatorController;
    [SerializeField] private RuntimeAnimatorController kingDevilAnimatorController;
    [SerializeField] private RuntimeAnimatorController devilRunTowardsPlayerAnimatorController;

    [Header("Movement Dependencies")]
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Reaction Settings")]
    [SerializeField] private float electrifyEffectDuration = 2.0f;
    [SerializeField] private int electrifyAnimInt = 5;
    [SerializeField] private int deathAnimInt = 6;
    [SerializeField] private int kingDeathAnimInt = 29;
    [SerializeField] private int resetAnimInt = -1;
   
    [Header("RunAway References")]
    [SerializeField] public NPCRunAway _npcRunAway;
    
    
    private Random _mathRandom;
    private quaternion _originalOtherFormRotation;
    private bool _isReactingToHit = false;

   
    public static event Action<GameObject> OnHitReacted;
    //public static event Action<GameObject> OnReactWhenEnemyDie;
    public static event Action<GameObject> OnReactWhenNPCDie;
    public static event Action<GameObject, NPCReactionType, Transform> OnSpecificReactionTriggered;
    public static event Action OnKingDevilReactionComplete;

    public Transform deathEffectPosition;
    // References to renderers
    public Renderer[] humanRenderers;
    public Renderer[] otherRenderers;

    // NEW: Death Effect Particle System
    [Header("Death Effects")]
    [SerializeField] private ParticleSystem deathParticleSystemPrefab; // Assign your particle system prefab here
    [SerializeField] private GameObject devilGun;
    [Header("Expression Checks And References")]
    [SerializeField] private ExpressionController _expressionControllerl;


    [Header("Coin animation References")] 
    [SerializeField] private CoinAnimator _coinAnimator;
    
    
    [Header("Fighting Animtion References")] 
    [SerializeField] private AudioSource characterSound;
    
    [Header("Fighting Animtion References")]
    [SerializeField] private RuntimeAnimatorController _fightingAnimator;

   
    public bool DeathPosSnapCheck = false;

    [SerializeField] private Transform deathPosSnap;

    [SerializeField] private bool runSoundPlay;
    

    private bool isRunAway = false;

    // Store original animator controllers and layer for resetting
    private RuntimeAnimatorController _originalHumanAnimatorController;
    private RuntimeAnimatorController _originalOtherFormAnimatorController;
    private int _originalLayer;
    
    private void Awake()
    {
        _mathRandom = new Random((uint)System.Environment.TickCount + (uint)GetInstanceID());

        if (humanForm != null && humanAnimator == null) humanAnimator = humanForm.GetComponent<Animator>();
        if (otherForm != null && otherFormAnimator == null) otherFormAnimator = otherForm.GetComponent<Animator>();

        if (otherForm != null)
        {
            _originalOtherFormRotation = otherForm.transform.rotation;
        }
        
        if (humanForm != null) humanRenderers = humanForm.GetComponentsInChildren<Renderer>(true); 
        if (otherForm != null) otherRenderers = otherForm.GetComponentsInChildren<Renderer>(true);

        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();

        if (humanForm != null) humanForm.SetActive(true);
        if (otherForm != null) otherForm.SetActive(true);
        
        SetFormVisibility(otherForm, otherRenderers, true); 
        SetFormVisibility(humanForm, humanRenderers, true); 

        // Store original animator controllers and layer
        if (humanAnimator != null) _originalHumanAnimatorController = humanAnimator.runtimeAnimatorController;
        if (otherFormAnimator != null) _originalOtherFormAnimatorController = otherFormAnimator.runtimeAnimatorController;
        _originalLayer = gameObject.layer;

    }

 
    private void SetFormVisibility(GameObject form, Renderer[] renderers, bool isVisible)
    {
        if (form == null) return;

        foreach (var rend in renderers)
        {
            if (rend != null) 
            {
                rend.enabled = isVisible;
            }
        }
    }

    
    public void ReactToHit()
    {
        Debug.Log("reacting to hit ----------" );
        if (_isReactingToHit) return;
        {
            if (gameObject.CompareTag("Devil") && !gameObject.GetComponent<EnemyBase>().enemyIsAProp && _expressionControllerl != null)
            {
                _expressionControllerl.SetDeathExpression(true);
            }
            
        }

        if (characterSound != null)
        {
            characterSound.Stop();
        }
        
        _isReactingToHit = true;

         
        if(!isADemoCharacter)
            OnHitReacted?.Invoke(gameObject);

        int targetLayer = LayerMask.NameToLayer("Default");
        if (targetLayer == -1) targetLayer = 0;

        if (humanForm != null) SetLayerRecursively(humanForm, targetLayer);
        if (otherForm != null) SetLayerRecursively(otherForm, targetLayer);

        if (reactionAnimatorController != null)
        {
            if (humanAnimator != null) humanAnimator.runtimeAnimatorController = reactionAnimatorController;
            if (otherFormAnimator != null) otherFormAnimator.runtimeAnimatorController = reactionAnimatorController;
        }

       
        if (gameObject.GetComponent<CapsuleCollider>() != null)
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
        
        if (_npcRunAway != null)
        {
            _npcRunAway.CloseSpline();
        }

        if (isADemoCharacter)
        {
            StartCoroutine(DemoReactToHitBodySwitching());
        }
        else
        {
            StartCoroutine(ReactToHitBodySwitching());
        }
        
    
    }
    
    private IEnumerator DemoReactToHitBodySwitching()
    {
        
        if (humanAnimator != null)
            humanAnimator.SetInteger("AnimInt", electrifyAnimInt);
        if (otherFormAnimator != null)
            otherFormAnimator.SetInteger("AnimInt", electrifyAnimInt);

        quaternion randomOtherRotation1 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomHumanRotation1 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomOtherRotation2 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomHumanRotation2 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));

        float stepDuration = electrifyEffectDuration / 4f;
        
        SetFormVisibility(humanForm, humanRenderers, false); 
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = randomOtherRotation1;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        if (humanForm != null)
            SetFormVisibility(otherForm, otherRenderers, false);
        else 
        {
            SetFormVisibility(otherForm, otherRenderers, true);
        }
        SetFormVisibility(humanForm, humanRenderers, true); 
        transform.rotation = randomHumanRotation1;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        SetFormVisibility(humanForm, humanRenderers, false);
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = randomOtherRotation2;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        if (humanForm != null)
            SetFormVisibility(otherForm, otherRenderers, false);
        else
        {
            SetFormVisibility(otherForm, otherRenderers, true);
        }
        SetFormVisibility(humanForm, humanRenderers, true); 
        transform.rotation = randomHumanRotation2;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        SetFormVisibility(humanForm, humanRenderers, false);
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = _originalOtherFormRotation;
        
        if (DeathPosSnapCheck)
        {
            transform.position = deathPosSnap.position;
        }
        otherFormAnimator.SetInteger("AnimInt", kingDeathAnimInt);
        yield return new WaitForSecondsRealtime(3f);
        otherFormAnimator.SetInteger("AnimInt", 32);
        yield return new WaitForSecondsRealtime(3f);   
       
        _isReactingToHit = false;
    }

    
   
    private IEnumerator ReactToHitBodySwitching()
    {
        
        if (humanAnimator != null)
            humanAnimator.SetInteger("AnimInt", electrifyAnimInt);
        if (otherFormAnimator != null)
            otherFormAnimator.SetInteger("AnimInt", electrifyAnimInt);

        quaternion randomOtherRotation1 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomHumanRotation1 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomOtherRotation2 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));
        quaternion randomHumanRotation2 = quaternion.Euler(_mathRandom.NextFloat(-180, 180),
            _mathRandom.NextFloat(0, 360), _mathRandom.NextFloat(-180, 180));

        float stepDuration = electrifyEffectDuration / 4f;

        if (devilExpressionController != null)
        {
            Debug.Log("Devil Expression controller");
            devilStunEffects.SetActive(true);
            devilExpressionController.ResetAllBlendShapes();
           devilExpressionController.DevilSadWoow(true);
        }

        if(_npcRunAway!=null && _npcRunAway.isWaiterCafeLevel)
            _npcRunAway.isWaiterCafeLevel = false;
        
        if ( _npcRunAway != null && _npcRunAway.isDevilPoleDancePlayer )
        {
            _npcRunAway.isDevilRunTowardsPlayer = false;
            _npcRunAway.isDevilPoleDancePlayer = false;
             _npcRunAway.StopPoleDance();
        }


        
        SetFormVisibility(humanForm, humanRenderers, false); 
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = randomOtherRotation1;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        if(humanForm !=null)
            SetFormVisibility(otherForm, otherRenderers, false);
        else
        {
            SetFormVisibility(otherForm, otherRenderers, true);
            
        }
        
        SetFormVisibility(humanForm, humanRenderers, true); 
        transform.rotation = randomHumanRotation1;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        SetFormVisibility(humanForm, humanRenderers, false);
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = randomOtherRotation2;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        if(humanForm !=null)
            SetFormVisibility(otherForm, otherRenderers, false);
        else
        {
            SetFormVisibility(otherForm, otherRenderers, true);
        }
        
        SetFormVisibility(humanForm, humanRenderers, true); 
        transform.rotation = randomHumanRotation2;
        yield return new WaitForSecondsRealtime(stepDuration);
        
        SetFormVisibility(humanForm, humanRenderers, false);
        SetFormVisibility(otherForm, otherRenderers, true); 
        transform.rotation = _originalOtherFormRotation; 
        OnDeathLookAt(); 
        
        if (DeathPosSnapCheck)
        {
            transform.position = deathPosSnap.position;
        }

       
        // GameManager.Instance.economyManager.AddCoins(GameManager.Instance.levelManager.CurrentLevel.GetLevelRewardCoins()/GameManager.Instance.levelManager._currentEnemyNumber); 
        
        if (isKingDevil)
        {
            yield return StartCoroutine(WaitForOtherEnemiesToDie());
            yield return StartCoroutine(ReactToHitKingDevilEndSequence());
        }
        else 
        {
            yield return StartCoroutine(ReactToHitNormalDevilEndSequence());
        }
        
        _isReactingToHit = false;
    }
    
    private IEnumerator WaitForOtherEnemiesToDie()
    {
        devilStunEffects.SetActive(false); 

        if (otherFormAnimator != null)
        {
            otherFormAnimator.SetInteger("AnimInt", kingDeathAnimInt);
        }

        while (GameManager.Instance.levelManager._currentEnemyNumber > 1)
        {
            yield return new WaitForSeconds(1.0f); 
        }

        yield return new WaitForSecondsRealtime(3f); 
    }
    
    private IEnumerator ReactToHitKingDevilEndSequence()
    {
        
        devilExpressionController.DevilSadWoow(false);
        devilExpressionController.DevilBlink(false);
        
        if(GameManager.Instance.levelManager._currentLevelNumber == 11)
            OnDeathLookAt();
        
        otherFormAnimator.SetInteger("AnimInt", 33);
        GameManager.Instance.playerController.ResetTools();
        GameManager.Instance.uiManager.HidePanel(UIPanelType.GameOverlayPanel);

        yield return new WaitForSecondsRealtime(2f);   
        StartCoroutine( GameManager.Instance.playerController.KingDevilCutScene(cutSceneCameraPos));
        yield return new WaitForSecondsRealtime(2f);   
        devilExpressionController.DevilTalking(true);
        
        otherFormAnimator.SetInteger("AnimInt", 24);
        yield return new WaitForSecondsRealtime(3f); 
        otherFormAnimator.SetInteger("AnimInt", 15);
        yield return new WaitForSecondsRealtime(3f); 
        otherFormAnimator.SetInteger("AnimInt", 24);
        yield return new WaitForSecondsRealtime(5f); 
        otherFormAnimator.runtimeAnimatorController = kingDevilAnimatorController;
        devilExpressionController.DevilEvilSmile(true);
      
        if (devilWings != null)
        {
            devilWings.SetActive(true);
            devilWings.transform.DOScale(1, 0.5f);
        }
      
        if (devilKingRunEffects != null)
        {
            Vector3 spawnPosition = (deathEffectPosition != null) ? deathEffectPosition.position : transform.position;
            GameObject effectInstance = Instantiate(devilKingRunEffects, spawnPosition, Quaternion.identity);

            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(effectInstance, ps.main.duration);
            }
            else
            {
                Destroy(effectInstance, 2.0f);
            }
        }
        
        _npcRunAway.HandleRunawayReaction();
        yield return new WaitForSecondsRealtime(1f); 
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Devil_Laugh);
        yield return new WaitForSecondsRealtime(2f); 
       
        if (gameObject.CompareTag("Devil"))
        {
            GameManager.Instance.levelManager.UpdateEnemyCount();
            GameManager.Instance.levelManager.CheckLevelCompletionCount();
        }
        OnKingDevilReactionComplete?.Invoke();
    }
    
    private IEnumerator ReactToHitNormalDevilEndSequence()
    {
        
        if (_coinAnimator != null)
        {
            _coinAnimator.CoinSize(0.35f);
            _coinAnimator.numberOfCoins = 15;
            _coinAnimator.animationDuration = 1f;
            _coinAnimator.spreadRadius = 1.2f;
            _coinAnimator.startTransform = deathEffectPosition;
            _coinAnimator.targetTransform = GameManager.Instance.uiManager.coinsTargetPos;
            GameManager.Instance.uiManager.AddCoinsUI(onDeathCoins);
            _coinAnimator.AnimateCoinsDevilDeath();
        }
        
        if (otherFormAnimator != null)
            otherFormAnimator.SetInteger("AnimInt", deathAnimInt);

        yield return new WaitForSecondsRealtime(3f);

        if (deathParticleSystemPrefab != null)
        {
            Vector3 spawnPos = otherForm.transform.position;
            Quaternion spawnRot = deathParticleSystemPrefab.transform.rotation;
            ParticleSystem deathEffect = Instantiate(deathParticleSystemPrefab, spawnPos, spawnRot);
            deathEffect.Play();
            Destroy(deathEffect.gameObject, deathEffect.main.duration + deathEffect.main.startLifetime.constantMax);
        }
        else
        {
            Debug.LogWarning("CharacterReactionHandler: Death Particle System Prefab is not assigned! No death effect will play.");
        }
   
        if (gameObject.CompareTag("Devil"))
        {
            GameManager.Instance.levelManager.UpdateEnemyCount();
            GameManager.Instance.levelManager.CheckLevelCompletionCount();
         
           
            Destroy(gameObject);
        }
    }
    
    
    
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform) 
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public IEnumerator ReactWhenEnemyDie()
    {
        if(gameObject.CompareTag("NPC"))
            _expressionControllerl.Terrify(true);
        
        if (characterSound != null)
        {
            characterSound.Stop();
        }
        if (_isReactingToHit) yield break;
        if (humanAnimator != null) humanAnimator.runtimeAnimatorController = reactionAnimatorController;
        if (otherFormAnimator != null) otherFormAnimator.runtimeAnimatorController = reactionAnimatorController;
        
        
        if (humanAnimator != null)
        {
            humanAnimator.SetInteger("AnimInt",resetAnimInt);
            humanAnimator.SetTrigger("Duck");
        }
        if (otherFormAnimator != null)
        {
             otherFormAnimator.SetInteger("AnimInt",resetAnimInt);
            otherFormAnimator.SetTrigger("Duck"); 
        }

        yield return new WaitForSecondsRealtime(1.75f);
        
    } 

    public IEnumerator ReactWhenNPCDie()
    {
        if (_isReactingToHit) yield break; 
        if (humanAnimator != null) humanAnimator.runtimeAnimatorController = reactionAnimatorController;
        if (otherFormAnimator != null) otherFormAnimator.runtimeAnimatorController = reactionAnimatorController;
     
        if (DeathPosSnapCheck)
        {
            transform.position = deathPosSnap.position;
        }
        
        if (characterSound != null)
        {
            characterSound.Stop();
        }
        if(devilGun != null)
            devilGun.SetActive(true);

      
        if (TryGetComponent(out IEnemy enemyComponent))
        {
            if (humanAnimator != null)
            {
                humanAnimator.SetInteger("AnimInt", 8);
            } 
            if (otherFormAnimator != null)
            {
                otherFormAnimator.SetInteger("AnimInt", 8);
            }
        }
        int targetLayer = LayerMask.NameToLayer("Default"); 
        if (humanForm != null) SetLayerRecursively(humanForm, targetLayer);
        if (otherForm != null) SetLayerRecursively(otherForm, targetLayer);

        if (humanForm != null) humanForm.SetActive(false);
        OnDeathLookAt();
        
        
        yield return new WaitForSecondsRealtime(4f);  
        Debug.Log("ReactWhenNPCDie() ==== ");
        OnReactWhenNPCDie?.Invoke(gameObject);
    } 

    public void TriggerSpecificReaction(NPCReactionType type, Transform targetDestination = null)
    {
        if ( _npcRunAway != null && _npcRunAway.isRunningINCircle)
        {
            _npcRunAway.CloseSpline();
        }
        
        switch (type) 
        {
            case NPCReactionType.RunAway:
                isRunAway = true;
                break;
            case NPCReactionType.Hide:
                HandleHide();
                break;
            case NPCReactionType.Duck: 
                StartCoroutine(ReactWhenEnemyDie());
                break;
            case NPCReactionType.StopReaction:
                HandleStopReaction();
                break;
            case NPCReactionType.Death:
                break;
            case NPCReactionType.Shoot:
                StartCoroutine(ReactWhenNPCDie());
                break;
            case NPCReactionType.RunTowardsPlayer:
                DevilRunTowardsPlayer();
                break;
            default:
                break;
        }
    }
    
    public IEnumerator HandleRunAwayCoroutine(int enemyCount) 
    {

        if (_npcRunAway != null && !_npcRunAway.isDevilPoleDancePlayer)
        {
            
        }
        
        if (_isReactingToHit) yield break;
        yield return new WaitForSecondsRealtime(1.75f);
        
        if(!runSoundPlay)
            GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Alien_Footstep,true);
        
        _npcRunAway.enemyCountCheck = enemyCount;
        if ( GameManager.Instance.levelManager._currentLevelNumber != 21)
        {
            humanAnimator.speed = 7;
            humanAnimator.SetInteger("AnimInt", 1);
            humanAnimator.SetBool("Force Run", true);
        }

        if (_npcRunAway != null)
        {
            _npcRunAway.HandleRunawayReaction();
        }
        yield return new WaitForSecondsRealtime(3.2f);
        GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Alien_Footstep,false);
       
    }

    public void DevilRunTowardsPlayer()
    {
        if(_npcRunAway != null)
            _npcRunAway.DevilRunTowardsPlayer();
        
        otherFormAnimator.runtimeAnimatorController = devilRunTowardsPlayerAnimatorController;
        otherFormAnimator.applyRootMotion = false;

    }
    
    private void HandleHide()
    {
        humanAnimator.SetInteger("AnimInt",19);
    }

    private void HandleStopReaction()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }
        if (GetComponent<Animator>() != null) GetComponent<Animator>().SetBool("IsRunning", false);
    }

    private void OnDeathLookAt()
    {
        Vector3 targetDirection = GameManager.Instance.playerController.enemyLookAt.position - otherForm.transform.position;
       // Vector3 targetDirection = GameManager.Instance.playerController.enemyLookAt.position - transform.position;
        targetDirection.y = 0;
        
        if (targetDirection == Vector3.zero)
        {
            Debug.LogWarning("OnDeathLookAt: Target and otherForm are at the same horizontal position. Cannot look at target.");
            return;
        }
        
        Quaternion targetRotationY = Quaternion.LookRotation(targetDirection, Vector3.up);
        
        otherForm.transform.rotation = Quaternion.Euler(
            otherForm.transform.rotation.eulerAngles.x, 
            targetRotationY.eulerAngles.y,  
            otherForm.transform.rotation.eulerAngles.z 
        );
    }
    
    public bool ReactingToHit() 
    {
        return _isReactingToHit;
    }

    public void ActivateJumpAnimation()
    {
        Debug.Log("Actviate jump animatioon");
        otherFormAnimator.Play("Jump Over");
    }

    private IEnumerator StartFighting()
    {
        if (_isReactingToHit) yield break; 
        if (humanAnimator != null) humanAnimator.runtimeAnimatorController = reactionAnimatorController;
        if (otherFormAnimator != null) otherFormAnimator.runtimeAnimatorController = reactionAnimatorController;
        
        if (humanAnimator != null)
        {
            humanAnimator.SetInteger("AnimInt", 27);
        } 
        if (otherFormAnimator != null)
        {
            otherFormAnimator.SetInteger("AnimInt", 27);
        }
        
        yield return new WaitForSecondsRealtime(6f);  
        
        if (humanAnimator != null)
        {
            humanAnimator.runtimeAnimatorController = _fightingAnimator;
        } 
        if (otherFormAnimator != null)
        {
            otherFormAnimator.runtimeAnimatorController = _fightingAnimator;
        }
    }
    
    public void ActivateFightAnimation()
    {
        StartCoroutine(StartFighting());
    }


}