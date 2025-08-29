using System;
using UnityEngine;
using System.Collections;
using MasterFX;
using Unity.VisualScripting; 
using DG.Tweening;

public class ShootingGunController : MonoBehaviour, IWeapon
{
    
    [Header("Gun References")]
    public GameObject gunModel; 
    public Transform laserOriginPoint; 

    [Header("Laser Effect")]
    public ShootingBlast shootingBlast; // Reference to the new script
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

    private Coroutine _shootingCoroutine;
    
    public static event Action OnLaserFired;
    public bool IsActive { get; private set; } = false;

    public void Init()
    {
        IsActive = false;
        if (shootingBlast == null)
        {
            shootingBlast = GetComponent<ShootingBlast>();
        }
    }
    
    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
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
        if (shootingBlast == null)
        {
            Debug.LogWarning("ShootingBlast component is not assigned. Laser visual will not appear.");
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
               GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Gun_Shoot);
               
               // Use the new Fire method that accepts a target Transform
               Fire(_parentRef.GetComponent<CharacterReactionHandler>().deathEffectPosition.transform);
               
            }
            else if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GameObject tempLaserEndPoint = new GameObject("TemporaryLaserEndPoint");
                    tempLaserEndPoint.transform.position = hit.point;

                    gunModel.transform.LookAt(hit.point);
                    Vibration.VibratePop();
                    GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Gun_Shoot);
                    GameObject impact2 = Instantiate(missImpactEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(impact2, impactEffectDuration);
                    
                    Fire(tempLaserEndPoint.transform);
                    
                    Destroy(tempLaserEndPoint);
                }
            }
             
        }
    }


    public void Fire()
    {
        throw new NotImplementedException();
    }

    public void Fire(Vector3 points)
    {
        throw new NotImplementedException();
    }

    // New Fire method that uses a target transform
    public void Fire(Transform targetTransform)
    { 
        if (shootingBlast == null)
        {
            Debug.LogError("ShootingBlast component not found!");
            return;
        }

        // Stop any previous shooting coroutine to prevent overlap
        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
        }
        
        // Start the new shooting coroutine with the target
        _shootingCoroutine = StartCoroutine(ShootBulletAtTarget(targetTransform));

        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, targetTransform.position, Quaternion.identity);
            Destroy(impact, impactEffectDuration);
        }
    }

    private IEnumerator ShootBulletAtTarget(Transform targetTransform)
    {
        Vector3 targetPosition = targetTransform.position;
        gunModel.transform.LookAt(targetPosition);
        
        shootingBlast.BulletStartPoint.LookAt(targetPosition);
        
        // --- NEW: Store references to instantiated effects ---
        var muzzle = Instantiate(shootingBlast.Muzzle, shootingBlast.BulletStartPoint.position, shootingBlast.BulletStartPoint.rotation);
        var bullet = Instantiate(shootingBlast.Bullet, shootingBlast.BulletStartPoint.position, shootingBlast.BulletStartPoint.rotation);

        GameObject Trail = null;
        if (shootingBlast.DefaultTrails != null)
        {
            Trail = Instantiate(shootingBlast.DefaultTrails, shootingBlast.BulletStartPoint.position, shootingBlast.BulletStartPoint.rotation);
            shootingBlast.SetRampTexture(Trail.gameObject);
        }
        
        shootingBlast.SetRampTexture(bullet.gameObject);
        shootingBlast.SetRampTexture(muzzle.gameObject);
        
        bullet.transform.parent = transform;
        
        var time = Vector3.Distance(shootingBlast.BulletStartPoint.position, targetPosition) / shootingBlast.BulletSpeed;
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            bullet.transform.position += bullet.transform.forward * shootingBlast.BulletSpeed * Time.deltaTime;
            if (Trail != null)
            {
                Trail.transform.position = bullet.transform.position;
            }
            yield return null;
        }

        // --- NEW: Destroy bullet and trail after their visual "life" ---
        Destroy(bullet.gameObject);
        if (Trail != null)
        {
            Destroy(Trail);
        }
        
        // Instantiate hit effect
        var hitEffect = Instantiate(shootingBlast.HitEffect, bullet.transform.position, bullet.transform.rotation);
        shootingBlast.SetRampTexture(hitEffect.gameObject);

        // --- NEW: Clean up the muzzle and hit effects after they finish playing ---
        // A simple way is to destroy them after their respective durations
        float muzzleDuration = muzzle.main.duration;
        Destroy(muzzle.gameObject, muzzleDuration);
        
        float hitEffectDuration = hitEffect.main.duration;
        Destroy(hitEffect.gameObject, hitEffectDuration);
    }

    private void OnDisable()
    {
        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
        }
    }

    public float GetDamage()
    {
        throw new System.NotImplementedException();
    }

    public void FireAtCharacter(CharacterReactionHandler targetCharacter)
    {
        Debug.Log("FireAtCharacter(CharacterReactionHandler targetCharacter) ++++++");
        Transform targetTransform = targetCharacter.deathEffectPosition;
        
        gunModel.transform.LookAt(targetTransform.position);
        Vibration.VibratePop();
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Gun_Shoot);
        
        Fire(targetTransform);
        
        targetCharacter.GetComponent<IReactable>().ReactToHit();
    }
}