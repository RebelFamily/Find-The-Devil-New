using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public enum EnemyType { DevilLord, PrimeDevil, FlyingObject, SilentDevil }

[Serializable]
public struct PhaseEnemys
{
    public EnemyType type;
    public GameObject enemyObject;
}

[Serializable]
public struct PhaseNPCs
{
    public int ID;
    public GameObject npcObject;
}

[Serializable]
public class LevelPhaseData
{
    public string phaseName;
    public float phaseDuration;
    [Header("Units for this Phase")]
    public PhaseEnemys[] enemiesInPhase;
    public PhaseNPCs[] npcsInPhase;
}

public class LevelPhaseManager : MonoBehaviour
{
    [Header("Phase Definitions")]
    [SerializeField] private LevelPhaseData[] phases;

    private List<IReactable> _activeEnemiesInPhase;
    private List<IReactable> _activeNPCsInPhase;
    public List<IReactable> _allActiveReactableUnits;

    // A new list to hold references for resetting characters
    private List<CharacterReactionHandler> _allCharactersInCurrentPhase;

    private int _currentPhaseIndex = -1;
    private LevelPhaseData _currentPhaseData;
    private int _enemiesEliminatedInCurrentPhase;
    private int _npcEliminatedInCurrentPhase;
    private int _targetEnemiesForPhaseCompletion;
    public AudioManager.GameSound levelBGAudio;
    public AudioManager.GameSound levelBasedSFXSound;
    public float levelSkipPoint;
    
    
    
    
    [Header("Player Movement Controller")]
    public SplineData _SplineData;
    public int _playerMovementSpeed;
    public bool movePlayerToNextPoint;
    public bool isKindDevilLevel;
    
    public bool isRecentNPCDied;
    
    
    [Header("Phase Important Checks")]
    [SerializeField] private bool isElevatorLevel = false;
    [SerializeField] private ElevatorController _elevatorController;

    [SerializeField] private bool isRessuceLevel = false;
    [SerializeField] private CaptureDataContainer _captureDataContainer;

    private bool _hasTriggeredDevilRun = false; 

    [Header("Phase Important Checks")]
    [SerializeField] private bool isCafeLevel = false;
    [SerializeField] private GameObject waiterStartMovement;
    
    [Header("Phase Important Checks")]
    [SerializeField] private bool dontDuckLevel = false;
    
    
    [Header("Phase Important Checks")]
    [SerializeField] public bool isLadderLevel = false;
    [SerializeField] private GameObject ladderObject;
 
    [Header("Phase Important Checks")]
    public bool isWaiterlevelCheck = false;
   
    [Header("Ladder Level Specifics")]
    [SerializeField] private Transform ladderFallTargetPos;
    [SerializeField] private GameObject characterToFall;
    [SerializeField] private Animator fallingCharacterAnimator;
    [SerializeField] private Animator fallingCharacterOtherAnimator;
    [SerializeField] private RuntimeAnimatorController ladderFallAnimation;
    [SerializeField] private RuntimeAnimatorController ladderDeadAnimation;
    [SerializeField] private float ladderFallDuration = 1.5f;

    

    [Header("Ladder Level Specifics")] 
    public GameObject objectToHide;
    
    public void Init()   
    {
        _activeEnemiesInPhase = new List<IReactable>();
        _activeNPCsInPhase = new List<IReactable>();
        _allActiveReactableUnits = new List<IReactable>();
        _allCharactersInCurrentPhase = new List<CharacterReactionHandler>();
        _hasTriggeredDevilRun = false; 
    }

    private void OnEnable()
    {
        CharacterReactionHandler.OnHitReacted += HandleDuckAnimations;
        CharacterReactionHandler.OnKingDevilReactionComplete += HandleKingDevilReactionComplete;
    }

    public void HandleDuckAnimations(GameObject obj)
    {
        
        if (obj.CompareTag("Devil"))
        {
            foreach (var reactableUnit in _allActiveReactableUnits.ToList()) 
            {
                if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)  
                {
                    if (monoUnit.TryGetComponent(out CharacterReactionHandler charReactionHandler))
                    {
                        isRecentNPCDied = false;
                        if (isRessuceLevel)
                        {
                            charReactionHandler.TriggerSpecificReaction(NPCReactionType.RunTowardsPlayer);
                            
                        }
                        else
                        {
                            if(!dontDuckLevel)
                                charReactionHandler.TriggerSpecificReaction(NPCReactionType.Duck);
                        }
                    }
                }
            } 
        } 

        if (obj.CompareTag("NPC"))
        {
            foreach (var reactableUnit in _allActiveReactableUnits.ToList())
            {
                if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)
                {

                    isRecentNPCDied = true;
                    if (monoUnit.TryGetComponent(out CharacterReactionHandler charReactionHandler) && monoUnit.CompareTag("Devil"))
                    {
                        GameManager.Instance.uiManager.SwitchBlocker(true);

                        charReactionHandler.TriggerSpecificReaction(NPCReactionType.Shoot); 
                    }
                    else
                    {
                        if (!isWaiterlevelCheck)
                        {
                            Debug.Log("SetNewMovemntSpeedAfter(float value)");
                            isWaiterlevelCheck= true;
                        }
                        if(!dontDuckLevel) 
                            charReactionHandler.TriggerSpecificReaction(NPCReactionType.RunAway);
                        charReactionHandler.TriggerSpecificReaction(NPCReactionType.Duck); 
                    }
                    
                }
            }
        }
        
        HandleUnitDeath(obj.GetComponent<IReactable>());
    }

    public void RescueLevelFail()
    {
        foreach (var reactableUnit in _allActiveReactableUnits.ToList())
        {
            if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)
            {
                if (monoUnit.TryGetComponent(out CharacterReactionHandler charReactionHandler) && monoUnit.CompareTag("Devil"))
                {
                    charReactionHandler._npcRunAway.CloseSpline();
                    charReactionHandler.TriggerSpecificReaction(NPCReactionType.Shoot);
                    StartCoroutine(LevelFailCall());
                }
            }
        }
        
    }

    public IEnumerator LevelFailCall()
    {
        GameManager.Instance.uiManager.SwitchBlocker(true);
        yield return new WaitForSecondsRealtime(2f);
       //check
        GameManager.Instance.uiManager.SwitchBlocker(false);
        
        GameManager.Instance.LevelFail();
    }
    
    private void OnDisable()
    {
        foreach (var enemy in _activeEnemiesInPhase.ToList())
        {
            if (enemy != null)
            {
               
            }
        }
        CharacterReactionHandler.OnHitReacted -= HandleDuckAnimations;
        CharacterReactionHandler.OnKingDevilReactionComplete -= HandleKingDevilReactionComplete;
    }

    public void StartLevel() 
    {
        if (phases == null || phases.Length == 0)
        {
            return;
        }
        
        if (isCafeLevel)
            GameManager.Instance.playerController.SetNewMovementSpeed(_playerMovementSpeed);

        _currentPhaseIndex = -1;
        
        AdvancePhase();
    }

    public void AdvancePhase()
    {
        if (_currentPhaseIndex >= 0)
        {
            CompleteCurrentPhase();
        }

        _currentPhaseIndex++;

        if (_currentPhaseIndex < phases.Length)
        {
            _currentPhaseData = phases[_currentPhaseIndex];
            ResetPhaseState();
            AssignPhaseUnits();
            TriggerPhaseStartUnitReactions();
        }
        else
        {
            if(isKindDevilLevel) return;
            
            if (isRessuceLevel)
            {
                if (_SplineData != null)
                {
                    Debug.Log("AdvancePhase()");
                    _SplineData.SetNewMovementSpeed(4f,5.1f);
                }
            }
        }
    }

    private void ResetPhaseState()
    {
        _npcEliminatedInCurrentPhase = 0;
        _enemiesEliminatedInCurrentPhase = 0;
        _targetEnemiesForPhaseCompletion = _currentPhaseData.enemiesInPhase.Length;
    }
    
    private void AssignPhaseUnits()
    {
        
        _activeEnemiesInPhase.Clear();
        _activeNPCsInPhase.Clear();
        _allActiveReactableUnits.Clear();
        _allCharactersInCurrentPhase.Clear();

        if (_currentPhaseData.enemiesInPhase != null)
        {
            foreach (var enemyAssignment in _currentPhaseData.enemiesInPhase)
            {
                if (enemyAssignment.enemyObject == null)
                {
                    continue;
                }

                IReactable newEnemy = enemyAssignment.enemyObject.GetComponent<IReactable>();
                if (newEnemy != null)
                {
                    enemyAssignment.enemyObject.SetActive(true);
                    _activeEnemiesInPhase.Add(newEnemy);
                    RegisterReactableUnit(newEnemy);

                    // Add to the new permanent list
                    if (enemyAssignment.enemyObject.TryGetComponent(out CharacterReactionHandler handler))
                    {
                        _allCharactersInCurrentPhase.Add(handler);
                    }
                }
            }
        }

        if (_currentPhaseData.npcsInPhase != null)
        {
            foreach (var npcAssignment in _currentPhaseData.npcsInPhase)
            {
                if (npcAssignment.npcObject == null)
                {
                    continue;
                }

                IReactable newNPC = npcAssignment.npcObject.GetComponent<IReactable>();
                if (newNPC != null)
                {
                    npcAssignment.npcObject.SetActive(true);
                    _activeNPCsInPhase.Add(newNPC);
                    RegisterReactableUnit(newNPC);

                    // Add to the new permanent list
                    if (npcAssignment.npcObject.TryGetComponent(out CharacterReactionHandler handler))
                    {
                        _allCharactersInCurrentPhase.Add(handler);
                    }
                }
            }
        }
    }

    private void TriggerPhaseStartUnitReactions()
    {
        
    }

    private void RegisterReactableUnit(IReactable unit)
    {
        if (unit != null && !_allActiveReactableUnits.Contains(unit))
        {
            _allActiveReactableUnits.Add(unit);
        }
    }

    private void UnregisterReactableUnit(IReactable unit)
    {
        if (unit != null && _allActiveReactableUnits.Contains(unit))
        {
            _allActiveReactableUnits.Remove(unit);
        }
    }

    private void HandleEnemyDeath(IReactable eliminatedEnemy)
    {
        if (_activeEnemiesInPhase.Contains(eliminatedEnemy))
        {
            _activeEnemiesInPhase.Remove(eliminatedEnemy);
            UnregisterReactableUnit(eliminatedEnemy as IReactable);

            foreach (var reactableUnit in _allActiveReactableUnits.ToList())
            {
                if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)
                {
                    if (monoUnit.TryGetComponent(out IEnemy enemyComponent) && enemyComponent == eliminatedEnemy)
                    {
                        continue;
                    }

                    if (monoUnit.TryGetComponent(out IEnemy otherEnemy))
                    {
                        reactableUnit.ReactWhenEnemyDie();
                    }
                    else if (monoUnit.TryGetComponent(out UrbanNPC npcComponent))
                    {
                        reactableUnit.ReactWhenEnemyDie();
                    }
                }
            }
        }

        if (eliminatedEnemy is MonoBehaviour monoEnemy && monoEnemy.gameObject != null)
        {
            monoEnemy.gameObject.SetActive(false);
        }
    }
    
    private void HandleUnitDeath(IReactable eliminatedUnit)
    {
        UnregisterReactableUnit(eliminatedUnit); 

        if (_activeEnemiesInPhase.Contains(eliminatedUnit))
        {
            _enemiesEliminatedInCurrentPhase++;
            _activeEnemiesInPhase.Remove(eliminatedUnit);
        }
        else if (_activeNPCsInPhase.Contains(eliminatedUnit))
        {
            _npcEliminatedInCurrentPhase++;
            _activeNPCsInPhase.Remove(eliminatedUnit);
        }
        
        CheckPhaseCompletion();
    }

    public void HandleNPCDie(IReactable eliminatedNPC)
    {
        if (_activeNPCsInPhase.Contains(eliminatedNPC))
        {
            _activeNPCsInPhase.Remove(eliminatedNPC);
            UnregisterReactableUnit(eliminatedNPC);

            foreach (var reactableUnit in _allActiveReactableUnits.ToList())
            {
                if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)
                {
                    if (monoUnit.TryGetComponent(out UrbanNPC npcComponent) && npcComponent == eliminatedNPC)
                    {
                        continue;
                    }

                    if (monoUnit.TryGetComponent(out IEnemy enemyComponent))
                    {
                        reactableUnit.ReactWhenNPCDie();
                    }
                    else if (monoUnit.TryGetComponent(out UrbanNPC otherNPC) && otherNPC != eliminatedNPC)
                    {
                        reactableUnit.ReactWhenNPCDie();
                    }
                }
            }
        }

        if (eliminatedNPC is MonoBehaviour monoNPC && monoNPC.gameObject != null)
        {
            monoNPC.gameObject.SetActive(false);
        }
    }

    private void CheckPhaseCompletion()
    {
        if (_enemiesEliminatedInCurrentPhase >= _targetEnemiesForPhaseCompletion)
        {
            Debug.Log("SwitchBlocker(true)");
            GameManager.Instance.uiManager.SwitchBlocker(true);

            if (movePlayerToNextPoint)
            {
                    if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
                        _SplineData.SetNewMovementSpeed(_playerMovementSpeed,2f);
                    else
                    {
                        _SplineData.SetNewMovementSpeed(_playerMovementSpeed,4f);
                    }   
            }
            
            ElevatorTriggerCheck();
            AdvancePhase();
            RescueLevelTriggerCheck();
            StartCoroutine( LadderCutScene());
        }
        else  
        {
            Debug.Log("SwitchBlocker(false)"); 
            //GameManager.Instance.uiManager.SwitchBlocker(false);
            
            if(_npcEliminatedInCurrentPhase > 0)
                CompleteCurrentPhase();
        }
    }

    private void HandleKingDevilReactionComplete()
    {
        Debug.Log("King Devil reaction complete. Moving player to next point.");
        if(isKindDevilLevel)
        {
            if(GameManager.Instance.levelManager._currentLevelNumber == 11)
                _SplineData.SetNewMovementSpeed(_playerMovementSpeed,2f);
            else
            {
                _SplineData.SetNewMovementSpeed(_playerMovementSpeed,4f);
            }
        }
    }

    private void CompleteCurrentPhase() 
    {
        DeactivateAllUnitsInScene();
    }
    
    public IEnumerator LadderCutScene()
    {
        if (isLadderLevel)
        {
            yield return new WaitForSecondsRealtime(1.5f);
            if (ladderObject != null)
            {
                Animator ladderAnim = ladderObject.GetComponent<Animator>();
                if (ladderAnim != null)
                {
                    Debug.Log("Ladder object does not have an Animation component.");
                    ladderAnim.enabled = true;
                }
            }
            
            GameManager.Instance.playerController.StartCameraRotationForLevel(GameManager.Instance.levelManager._currentLevelNumber);

            if (characterToFall != null && ladderFallTargetPos != null && fallingCharacterAnimator != null && ladderFallAnimation != null && ladderDeadAnimation != null)
            {
                fallingCharacterOtherAnimator.runtimeAnimatorController = ladderFallAnimation;
                fallingCharacterAnimator.runtimeAnimatorController = ladderFallAnimation;
                fallingCharacterAnimator.speed = 1f;

                characterToFall.transform.DOMove(ladderFallTargetPos.position, ladderFallDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        
                        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.DevilKingFall_Down);
                        fallingCharacterOtherAnimator.runtimeAnimatorController = ladderDeadAnimation;
                        fallingCharacterAnimator.runtimeAnimatorController = ladderDeadAnimation;
                        fallingCharacterAnimator.speed = 1f;
                        CheckForBlocker(false);
                        isLadderLevel = false;
                        GameManager.Instance.playerController.GunScannerSwitch();
                        
                       
                    });
                yield return new WaitForSecondsRealtime(2f);

            }
            else
            {
                Debug.LogWarning("LadderCutScene: Missing references for character falling animation. Please assign characterToFall, ladderFallTargetPos, fallingCharacterAnimator, ladderFallAnimation, and ladderDeadAnimation in the Inspector.");
            }
        }
        else
        {
            if(ladderObject != null)
                ladderObject.SetActive(false);
        }
    } 
    
    private void ElevatorTriggerCheck()
    {
        if (isElevatorLevel)
        {
            _elevatorController.MoveElevator(2.7f);
        }
    }

    private void RescueLevelTriggerCheck()
    {
        if (isRessuceLevel && _SplineData != null )
        {
            if (GameManager.Instance.levelManager._currentLevelNumber == 5)
            {
                Debug.Log(" RescueLevelTriggerCheck()");
                _SplineData.SetNewMovementSpeed(_playerMovementSpeed,1f);
            }
            else
            {
                GameManager.Instance.uiManager.SwitchBlocker(false); 
                foreach (var reactableUnit in _allActiveReactableUnits.ToList())
                {
                    if (reactableUnit is MonoBehaviour monoUnit && monoUnit.gameObject != null)  
                    {
                        if (monoUnit.TryGetComponent(out CharacterReactionHandler charReactionHandler))
                        {
                            if (isRessuceLevel)
                            {
                                charReactionHandler.TriggerSpecificReaction(NPCReactionType.RunTowardsPlayer);
                            }
                        }
                    }
                } 
            }
        }
    }
    
    private void DeactivateAllUnitsInScene()  
    {
        foreach (var enemy in new List<IReactable>(_activeEnemiesInPhase))
        {
            if (enemy != null && enemy is MonoBehaviour monoEnemy && monoEnemy.gameObject != null)
            {
                UnregisterReactableUnit(enemy as IReactable);
            }
        }
        _activeEnemiesInPhase.Clear();

        foreach (var npc in new List<IReactable>(_activeNPCsInPhase))
        {
            if (npc != null && npc is MonoBehaviour monoNPC && monoNPC.gameObject != null)
            {
                UnregisterReactableUnit(npc);
                if (monoNPC.TryGetComponent(out CharacterReactionHandler npcReactionHandler))
                {
                    StartCoroutine( npcReactionHandler.HandleRunAwayCoroutine( _targetEnemiesForPhaseCompletion - _enemiesEliminatedInCurrentPhase));
                }
            }
        }
        _activeNPCsInPhase.Clear();
        _allActiveReactableUnits.Clear();
    }

    public int GetCapturedNPCCount()
    {
        if (_captureDataContainer != null)
        {
            return _captureDataContainer._capturedNPC; 
        }
        return 0;
    }

    public void CheckForBlocker(bool check)
    {
        if (GameManager.Instance._waitForTryGun)
        {
            GameManager.Instance._waitForTryGun = false;
        }

        GameManager.Instance._isReachedPoint = true;
            
        if(GameManager.Instance.levelManager.GlobalLevelNumber == 0)
            return;
        
        GameManager.Instance.uiManager.SwitchBlocker(check);
    }

    public void PlayDoorOpenSound()
    {
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.DoorOpen);
        GameManager.Instance.audioManager.StopLoopingMusic();
    }

    public void GlassBreakingSound()
    {
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Glass_Breaking);
    }
    public int GetCurrentPhaseIndex() => _currentPhaseIndex;
    public LevelPhaseData GetCurrentPhaseData() => _currentPhaseData;
    public List<IReactable> GetActiveEnemies() => _activeEnemiesInPhase;
    
    public void ResetActiveUnits()
    {
        Debug.Log("Resetting units for level restart.");

        List<GameObject> originalPrefabs = new List<GameObject>();
        List<Vector3> originalPositions = new List<Vector3>();
        List<Quaternion> originalRotations = new List<Quaternion>();

        foreach (var unitHandler in _allCharactersInCurrentPhase.ToList())
        {
            if (unitHandler != null && unitHandler.CompareTag("Devil"))
            {
                originalPrefabs.Add(unitHandler.gameObject);
                originalPositions.Add(unitHandler.transform.position);
                originalRotations.Add(unitHandler.transform.rotation);
                Destroy(unitHandler.gameObject);
            }
        }
        
        _allCharactersInCurrentPhase.Clear();
        _activeEnemiesInPhase.Clear();
        _allActiveReactableUnits.Clear();
        
        for (int i = 0; i < originalPrefabs.Count; i++)
        {
            GameObject newEnemyObj = Instantiate(originalPrefabs[i], originalPositions[i], originalRotations[i]);

            IReactable newEnemyReactable = newEnemyObj.GetComponent<IReactable>();
            if (newEnemyReactable != null)
            {
                _activeEnemiesInPhase.Add(newEnemyReactable);
                RegisterReactableUnit(newEnemyReactable);

                if (newEnemyObj.TryGetComponent(out CharacterReactionHandler handler))
                {
                    _allCharactersInCurrentPhase.Add(handler);
                }
            }
        }

        Debug.Log("Instantiated " + _allCharactersInCurrentPhase.Count + " new enemies for level restart.");
        _enemiesEliminatedInCurrentPhase = 0;
        _targetEnemiesForPhaseCompletion = _currentPhaseData.enemiesInPhase.Length;
    }
}