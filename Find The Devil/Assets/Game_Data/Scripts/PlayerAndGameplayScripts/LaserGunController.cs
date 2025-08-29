using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using MasterFX;
using Unity.VisualScripting; 

public class LaserGunController : MonoBehaviour, IWeapon
{
    
    [Header("Gun References")]
    public GameObject gunModel; 
    public Transform laserOriginPoint;

    [Header("Laser Effect")] 
    public AudioManager.GunSounds GunSound;

    [Header("Laser Effect")]
    public GameObject laserPrefab;
    public float laserDisplayDuration = 0.1f; 
    public LayerMask tappableLayer; 
   
    [Header("Impact Effect (Optional)")]
    public GameObject impactEffectPrefab; 
    public GameObject missImpactEffectPrefab; 
    public float impactEffectDuration = 0.5f;
    public Transform demoTargetPos;
    
    [Header("Fire Rate")]
    [SerializeField] private float fireRateCooldown = 0.25f;
    private float _lastFireTime = -1f;

    private GameObject _currentLaserEffect;
    private Coroutine laserDisplayCoroutine; 
    
    public static event Action OnLaserFired;
    public bool IsActive { get; private set; } = false;

    public LaserShooter _laserShooter;

    public void Init()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        if (_currentLaserEffect != null)
        {
            MLaser currentMLaser = _currentLaserEffect.GetComponent<MLaser>();
            if (currentMLaser != null)
            {
                currentMLaser.StopLaser();
            }
            Destroy(_currentLaserEffect);
            _currentLaserEffect = null;
        }
    
        if (laserDisplayCoroutine != null)
        {
            StopCoroutine(laserDisplayCoroutine);
            laserDisplayCoroutine = null;
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
        if (laserPrefab == null)
        {
            Debug.LogWarning("Laser Prefab is not assigned. Laser visual will not appear.");
        }
    }

    public void HandleTapInput(Vector3 pos)
    {
        if (IsActive) 
        {
          
            if (Time.time < _lastFireTime + fireRateCooldown)
            {
                return;
            }
            _lastFireTime = Time.time;
            
            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit; 
        
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tappableLayer))
            {
               if (!hit.transform.gameObject.CompareTag("Devil"))
               {
                   Debug.Log("i am deactivating laser gun = " + hit.transform.gameObject.tag);
                    IsActive = false;
               }

               GameObject _parentRef;
                   
               // Changes Start
                laserPrefab.GetComponent<Hovl_Laser>().laserStartTransform = laserOriginPoint; // Removed Hovl_Laser
               // Changes End

               if (hit.transform.GetComponent<ParentRefdHandler>())
               {
                   _parentRef = hit.transform.GetComponent<ParentRefdHandler>().parentRef;
               }
               else
               {
                   _parentRef = hit.transform.gameObject;
               }
               // Changes Start
                laserPrefab.GetComponent<Hovl_Laser>().laserEndTransform = _parentRef.GetComponent<CharacterReactionHandler>().deathEffectPosition; // Removed Hovl_Laser
               // Changes End
               _parentRef.GetComponent<IReactable>().ReactToHit();
               
                OnLaserFired?.Invoke();
                gunModel.transform.LookAt(hit.point);
                Vibration.VibratePop();
                GameManager.Instance.audioManager.PlayGunSFX(GunSound);
                // The Fire method will now handle setting MLaser's start and end points
                Fire(_parentRef.GetComponent<CharacterReactionHandler>().deathEffectPosition.transform.position);
               
            }
            else if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
            {
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GameObject tempLaserEndPoint = new GameObject("TemporaryLaserEndPoint");
                    tempLaserEndPoint.transform.position = hit.point;

                    // Changes Start
                     laserPrefab.GetComponent<Hovl_Laser>().laserStartTransform = laserOriginPoint; // Removed Hovl_Laser
                     laserPrefab.GetComponent<Hovl_Laser>().laserEndTransform = tempLaserEndPoint.transform; // Removed Hovl_Laser
                    // Changes End
                    gunModel.transform.LookAt(hit.point);
                    Vibration.VibratePop();
                    GameManager.Instance.audioManager.PlayGunSFX(GunSound);
                    GameObject impact2 = Instantiate(missImpactEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(impact2, impactEffectDuration);
                    // The Fire method will now handle setting MLaser's start and end points
                    Fire(hit.point);
                    
                    Destroy(tempLaserEndPoint);
                }
            }
             
        }
    }


    public void Fire()
    {
        throw new NotImplementedException();
    }

    public void Fire(Vector3 hitPoint)
    { 
        if (laserPrefab == null) return;
        
        if (laserDisplayCoroutine != null)
        {
            StopCoroutine(laserDisplayCoroutine);
        }
        if (_currentLaserEffect != null)
        {
          
            Destroy(_currentLaserEffect);
        }
        
        _currentLaserEffect = Instantiate(laserPrefab, laserOriginPoint.position, laserOriginPoint.rotation); 
        
        laserDisplayCoroutine = StartCoroutine(DisableLaserAfterDelay(_currentLaserEffect, laserDisplayDuration));
        
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(impact, impactEffectDuration);
           
        }

    }

    private GameObject laserToDisable;
    private IEnumerator DisableLaserAfterDelay(GameObject laser, float delay) 
    {
        laserToDisable = laser;
        yield return new WaitForSeconds(delay);
        if (laserToDisable != null)
        {
           
            Destroy(laserToDisable); 
        }
    }

    private void OnDisable()
    {
        
        Destroy(laserToDisable);
    }

  
    public float GetDamage()
    {
        throw new System.NotImplementedException();
    }

    public void FireAtCharacter(CharacterReactionHandler targetCharacter)
    {
        Debug.Log("FireAtCharacter(CharacterReactionHandler targetCharacter) ++++++");
        Transform targetTransform = targetCharacter.deathEffectPosition;
        Vector3 hitPoint = targetTransform.position;
        
        gunModel.transform.LookAt(hitPoint);
        Vibration.VibratePop();
        GameManager.Instance.audioManager.PlayGunSFX(GunSound);

        if (laserPrefab != null)
        {
            if (laserDisplayCoroutine != null)
            {
                StopCoroutine(laserDisplayCoroutine);
            }
            if (_currentLaserEffect != null)
            {
               
                Destroy(_currentLaserEffect);
            }

            if (laserPrefab.GetComponent<Hovl_Laser>()) // Removed this if block
            {
                laserPrefab.GetComponent<Hovl_Laser>().laserStartTransform = laserOriginPoint;
                laserPrefab.GetComponent<Hovl_Laser>().laserEndTransform = targetTransform;
            }
            
            _currentLaserEffect = Instantiate(laserPrefab, laserOriginPoint.position, laserOriginPoint.rotation); // Instantiate at world position
           

            laserDisplayCoroutine = StartCoroutine(DisableLaserAfterDelay(_currentLaserEffect, laserDisplayDuration));
        }
        
        if (impactEffectPrefab != null) 
        {
            GameObject impact = Instantiate(impactEffectPrefab, targetTransform.position, Quaternion.identity);
            Destroy(impact, impactEffectDuration);
        }
        
        targetCharacter.GetComponent<IReactable>().ReactToHit();
       
    }
}
