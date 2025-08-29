using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using PilotoStudio; 
using Unity.VisualScripting;

public class BeamGunController : MonoBehaviour, IWeapon
{
    
    [Header("Gun References")]
    public GameObject gunModel; 
    public Transform laserOriginPoint; 

    [Header("Laser Effect")] 
    public AudioManager.GunSounds GunSound;
    
    [Header("Laser Effect")]
    public GameObject beamEmitterPrefab; 
    public float beamLifetime = 0.5f; 
    public LayerMask tappableLayer; 
   
    [Header("Impact Effect (Optional)")]
    public GameObject impactEffectPrefab; 
    public GameObject missImpactEffectPrefab; 
    public float impactEffectDuration = 0.5f;
    public Transform demoTargetPos;


    [Header("Gun Effects")] 
    [SerializeField] private GameObject gunModelEffects;
    
    [Header("Fire Rate")]
    [SerializeField] private float fireRateCooldown = 0.25f;
    private float _lastFireTime = -1f;

    private BeamEmitter _currentBeamEmitter;
    
    
    public static event Action OnLaserFired;
    public bool IsActive { get; private set; } = false;

    public void Init()
    {
        IsActive = false;
    }

    public void Activate()
    {
        if(gunModelEffects != null)
            gunModelEffects.SetActive(true);
        
        IsActive = true;
    }

    public void Deactivate()
    {
        if (gunModelEffects != null)
        {
            gunModelEffects.SetActive(false);
        }
        
        IsActive = false;
        
        if (_currentBeamEmitter != null)
        {
            Destroy(_currentBeamEmitter.gameObject);
            _currentBeamEmitter = null;
        }
    }
    
    void Start()
    {
        if (gunModel == null)
        {
            Debug.LogError("Gun Model is not assigned in " + gameObject.name + "!");
            enabled = false;
        }
        if (laserOriginPoint == null)
        {
    
            Debug.LogError("Laser Origin Point is not assigned in " + gameObject.name + "!");
            enabled = false;
        }
        if (beamEmitterPrefab == null)
        {
            Debug.LogWarning("Beam Emitter Prefab is not assigned. Laser visual will not appear.");
        }
    }

    public void HandleTapInput(Vector3 screenPosition)
    {
        if (IsActive) 
        {
            if (Time.time < _lastFireTime + fireRateCooldown)
            {
                return;
            }
            _lastFireTime = Time.time;
            
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit; 
        
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tappableLayer))
            {
               if (!hit.transform.gameObject.CompareTag("Devil"))
               {
                   Debug.Log("i am deactivating laser gun = " + hit.transform.gameObject.tag);
                   IsActive = false;
               }

               GameObject _parentRef;
                   
               if (hit.transform.GetComponent<ParentRefdHandler>())
               {
                   _parentRef = hit.transform.GetComponent<ParentRefdHandler>().parentRef;
               }
               else
               {
                   _parentRef = hit.transform.gameObject;
               }
               
               _parentRef.GetComponent<IReactable>().ReactToHit();
               
               OnLaserFired?.Invoke();
               gunModel.transform.LookAt(hit.point);
               Vibration.VibratePop();
               GameManager.Instance.audioManager.PlayGunSFX(GunSound);
               
               Fire(_parentRef.GetComponent<CharacterReactionHandler>().deathEffectPosition.transform);
               
            }
            else if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    gunModel.transform.LookAt(hit.point);
                    Vibration.VibratePop();
                    GameManager.Instance.audioManager.PlayGunSFX(GunSound);
                    GameObject impact2 = Instantiate(missImpactEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(impact2, impactEffectDuration);
                    
                    GameObject tempTarget = new GameObject("TemporaryLaserEndPoint");
                    tempTarget.transform.position = hit.point;
                    Fire(tempTarget.transform);
                    Destroy(tempTarget);
                }
            }
        }
    }

    
    
    public void Fire()
    {
        throw new NotImplementedException();
    }

    // New Fire method that uses Transform
    public void Fire(Transform targetTransform)
    {
       // GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.HitSound,0.3f);

        // --- Deactivate gun model effects when firing starts ---
        if (gunModelEffects != null)
        {
            gunModelEffects.SetActive(false);
        }

        if (beamEmitterPrefab == null) return;
    
        if (_currentBeamEmitter != null)
        {
            Destroy(_currentBeamEmitter.gameObject);
        }
    
        GameObject beamObj = Instantiate(beamEmitterPrefab, laserOriginPoint.position, laserOriginPoint.rotation, laserOriginPoint);
        _currentBeamEmitter = beamObj.GetComponent<BeamEmitter>();

        if (_currentBeamEmitter != null)
        {
            _currentBeamEmitter.beamTarget.position = targetTransform.position;
            _currentBeamEmitter.beamLifetime = beamLifetime;
            _currentBeamEmitter.PlayBeam();
        }
        else
        {
            Debug.LogError("Beam Emitter Prefab does not have a BeamEmitter component!");
        }
    
        if (impactEffectPrefab != null)
        {
           // GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.HitSound);

            // --- REFACTORED: Add a Vector3 to the position for a clean offset ---
            Vector3 impactPosition = targetTransform.position + new Vector3(0, 0, 5f);
            GameObject impact = Instantiate(impactEffectPrefab, impactPosition, Quaternion.identity);
            Destroy(impact, impactEffectDuration);
        }
   
        StartCoroutine(ReactivateGunEffectsAfterDelay(beamLifetime));
    }

    private IEnumerator ReactivateGunEffectsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reactivate effects only if the gun is still considered active
        if (IsActive && gunModelEffects != null)
        {
            gunModelEffects.SetActive(true);
        }
    }

    // Obsolete: You can remove this as the other Fire method is now used
    public void Fire(Vector3 hitPoint)
    {
        Debug.LogWarning("Fire(Vector3 hitPoint) is obsolete. Use Fire(Transform targetTransform) instead.");
    }
    
    private void OnDisable()
    {
        if (_currentBeamEmitter != null)
        {
            Destroy(_currentBeamEmitter.gameObject);
        }
    }

    public float GetDamage()
    {
        throw new NotImplementedException();
    }

    public void FireAtCharacter(CharacterReactionHandler targetCharacter)
    {
        Debug.Log("FireAtCharacter(CharacterReactionHandler targetCharacter) ++++++");
        Transform targetTransform = targetCharacter.deathEffectPosition;
        
        gunModel.transform.LookAt(targetTransform.position);
        Vibration.VibratePop();
        GameManager.Instance.audioManager.PlayGunSFX(GunSound);
        
        Fire(targetTransform);
        
        targetCharacter.GetComponent<IReactable>().ReactToHit();
    }
}