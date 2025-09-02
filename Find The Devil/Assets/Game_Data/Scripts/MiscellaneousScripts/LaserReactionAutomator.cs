using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LaserReactionAutomator : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private MonoBehaviour _weaponReference;
    private IWeapon _currentWeapon;
    
    [SerializeField] private CharacterReactionHandler _characterReactionHandler;

    [Header("Automator Type")]
    [SerializeField] private bool isScannerReactionAutomator;
    [SerializeField] private bool isLaserReactionAutomator;

    [Header("Scanner References & Settings")]
    [SerializeField] private GameObject scanner;
    [SerializeField] private Transform scannerInitialTransform;
    [SerializeField] private Transform scannerTransform1;
    [SerializeField] private Transform scannerTransform2;
    [SerializeField] private float initialAnimationDuration = 1.5f;
    [SerializeField] private float scannerMoveDuration = 2.0f;
    [SerializeField] private Ease scannerMoveEase = Ease.InOutQuad;


    [Header("Laser Gun Demo Settings")]  
    [SerializeField] private float laserActivationDelay = 0.35f;
    [SerializeField] private float reactionDuration = 3.0f;
    [SerializeField] private bool autoPlayOnStart = false;

     public ITweenMagic itweenMagicSC;
    // --- NEW: Gun Position & Animation Settings ---
    [SerializeField] private Transform gunShootPos;
    [SerializeField] private float gunAnimationDuration = 1.0f;
    [SerializeField] private Ease gunAnimationEase = Ease.OutQuad;
    private Vector3 _originalGunPosition;
    private Quaternion _originalGunRotation;
    // --- END NEW ---
    
    private Coroutine _currentDemoCoroutine;

    void Start()
    {
        if (_weaponReference != null)
        {
            _currentWeapon = _weaponReference.GetComponent<IWeapon>();
            if (_currentWeapon == null)
            {
                Debug.LogError("The assigned 'Weapon Reference' does not implement the IWeapon interface.", this);
                return;
            }
            // --- NEW: Store the original position and rotation of the gun ---
            _originalGunPosition = _weaponReference.transform.position;
            _originalGunRotation = _weaponReference.transform.rotation;
            // --- END NEW ---
        }

        StopAllDemos();
        
        if (autoPlayOnStart)
        {
            StartDemo();
        }
    }

    public void StartDemo()
    {
        StopAllDemos();

        if (scanner != null) scanner.SetActive(isScannerReactionAutomator);
        if (_weaponReference != null) _weaponReference.gameObject.SetActive(isLaserReactionAutomator);

        if (isScannerReactionAutomator)
        {
            if (scanner == null || scannerInitialTransform == null || scannerTransform1 == null || scannerTransform2 == null)
            {
                Debug.LogError("LaserReactionAutomator: Scanner references (scanner, initial, transform1, transform2) are missing for Scanner demo.", this);
                return;
            }
            GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Scanner_Activate,false);
            GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Scanner_Activate,true);

            _currentDemoCoroutine = StartCoroutine(ScannerReactionAutomatorSequence());
        }
        else if (isLaserReactionAutomator)
        {
            if (_currentWeapon == null || _characterReactionHandler == null)
            {
                Debug.LogError("LaserReactionAutomator: References to IWeapon or CharacterReactionHandler are missing for LaserGun demo.", this);
                return;
            }
            // --- UPDATED: Start the laser gun demo sequence ---
            _currentDemoCoroutine = StartCoroutine(RunLaserGunDemoSequence());
        }
        else
        {
            Debug.LogWarning("LaserReactionAutomator: No demo type (Scanner or Laser) is selected to start.");
        }
    }
    
    
    
    public void SetGunToPosition()
    {
        if (isScannerReactionAutomator)
        {
            return;
        }
        
        // First, check if the necessary references are assigned
        if (_weaponReference == null)
        {
            Debug.LogWarning("Weapon Reference is null. Cannot set gun position.");
            return;
        }
    
        if (itweenMagicSC != null)
        {
            // Enable the ITweenMagic component
            autoPlayOnStart = false;
            itweenMagicSC.enabled = true;
        }
        else
        {
            Debug.LogWarning("ITweenMagic script is not assigned. Cannot enable it.");
        }
    
        if (gunShootPos != null)
        {
            // Instantly set the position and rotation to the target
            _weaponReference.transform.position = gunShootPos.position;
            _weaponReference.transform.rotation = gunShootPos.rotation;
        }
        else
        {
            Debug.LogWarning("Gun Shoot Position is null. Cannot set gun position.");
        }
    }
    
    public void AnimateGunBackToOriginalPos(System.Action onAction = null)
    {
        // check
        StopAllDemos();
        
        // --- UPDATED: Use the new private variables ---
        if (_weaponReference == null)
        {
            Debug.LogError("Weapon Reference is null. Cannot animate gun back.", this);
            onAction?.Invoke();
            return;
        }
        // --- END UPDATED ---

        Transform gunTransform = _weaponReference.transform;
        gunTransform.DOKill();
    
        Sequence returnSequence = DOTween.Sequence();
    
        // --- UPDATED: Animate back to the stored original position ---
        returnSequence.Join(gunTransform.DOMove(_originalGunPosition, gunAnimationDuration).SetEase(gunAnimationEase));
        returnSequence.Join(gunTransform.DORotateQuaternion(_originalGunRotation, gunAnimationDuration).SetEase(gunAnimationEase));
        // --- END UPDATED ---

        returnSequence.OnComplete(() =>
        {
            onAction?.Invoke();
        });
    }
    
    // --- NEW: Function to animate the gun into position ---
    private IEnumerator AnimateGunToPosition()
    {
        yield return new WaitForSeconds(0.4f);
        if (_weaponReference == null || gunShootPos == null)
        {
            Debug.LogError("Gun reference or gunShootPos is null. Cannot animate gun.", this);
            yield break;
        }

        Transform gunTransform = _weaponReference.transform;
        
        // Use a sequence to combine both movement and rotation tweens
        Sequence gunAnimSequence = DOTween.Sequence();
        
        // Animate position to the gunShootPos
        gunAnimSequence.Join(gunTransform.DOMove(gunShootPos.position, gunAnimationDuration).SetEase(gunAnimationEase));
        
        // Animate rotation to the gunShootPos's rotation
        gunAnimSequence.Join(gunTransform.DORotateQuaternion(gunShootPos.rotation, gunAnimationDuration).SetEase(gunAnimationEase));

        yield return gunAnimSequence.WaitForCompletion();
    }
    // --- END NEW ---

    private IEnumerator RunLaserGunDemoSequence()
    {
        if (_currentWeapon == null)
        {
            Debug.LogError("IWeapon reference is null. Cannot run laser demo sequence.", this);
            yield break;
        }

        if (itweenMagicSC.enabled == false)
        {
            // --- UPDATED: Wait for the gun to animate into position before starting the demo ---
            yield return StartCoroutine(AnimateGunToPosition());
            // --- END UPDATED ---
        }

        _currentWeapon.Activate();

        while (true)
        {
            yield return new WaitForSeconds(laserActivationDelay);
            Debug.Log("RunDemoSequence - Laser Firing");
        
            _currentWeapon.FireAtCharacter(_characterReactionHandler);
            _characterReactionHandler.GetComponent<IReactable>().ReactToHit();

            yield return new WaitForSeconds(reactionDuration);
        }
    }

    private IEnumerator ScannerReactionAutomatorSequence()
    {
        if (scanner != null) scanner.SetActive(true);
        ScannerLogic scannerLogic = scanner.GetComponent<ScannerLogic>();

        if (scannerLogic == null)
        {
            Debug.LogError("Scanner object is missing ScannerLogic component!", scanner);
            StopAllDemos();
            yield break;
        }

        scannerLogic.Activate();
       
        scannerLogic.RevertBlackMaterialToOriginalColor();

        scanner.transform.position = scannerInitialTransform.position;
       
        yield return scanner.transform.DOMove(scannerTransform1.position, initialAnimationDuration).SetEase(scannerMoveEase).WaitForCompletion();

        while (true)
        {
            yield return scanner.transform.DOMove(scannerTransform2.position, scannerMoveDuration).SetEase(scannerMoveEase).WaitForCompletion();
            yield return scanner.transform.DOMove(scannerTransform1.position, scannerMoveDuration).SetEase(scannerMoveEase).WaitForCompletion();
        }
    }

    public void StopAllDemos()
    {
        if (_currentDemoCoroutine != null)
        {
            StopCoroutine(_currentDemoCoroutine);
            _currentDemoCoroutine = null;
        }

        if (scanner != null)
        {
           
            scanner.SetActive(false);
            scanner.transform.DOKill();
        }
        if (_currentWeapon != null)
        {
           _currentWeapon.Deactivate();
        }
        // --- NEW: Ensure the gun returns to its initial position/rotation on stop ---
        if (_weaponReference != null && scannerInitialTransform != null)
        {
            _weaponReference.transform.DOKill();
            _weaponReference.transform.position = scannerInitialTransform.position;
            _weaponReference.transform.rotation = scannerInitialTransform.rotation;
        }
        // --- END NEW ---
    }

    private void OnDisable()
    {
        StopAllDemos();
    }

    private void OnDestroy()
    {
        StopAllDemos();
    }
}