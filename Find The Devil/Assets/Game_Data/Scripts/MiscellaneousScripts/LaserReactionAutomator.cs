using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LaserReactionAutomator : MonoBehaviour
{
    [Header("Script References")]
    // --- CHANGED: Use a MonoBehaviour reference to hold the IWeapon script ---
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

    private Coroutine _currentDemoCoroutine;

    void Start()
    {
        // On start, get the IWeapon component from the assigned MonoBehaviour
        if (_weaponReference != null)
        {
            _currentWeapon = _weaponReference.GetComponent<IWeapon>();
            if (_currentWeapon == null)
            {
                Debug.LogError("The assigned 'Weapon Reference' does not implement the IWeapon interface.", this);
                return;
            }
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

        // --- UPDATED: Use the new references ---
        if (scanner != null) scanner.SetActive(isScannerReactionAutomator);
        if (_weaponReference != null) _weaponReference.gameObject.SetActive(isLaserReactionAutomator);

        if (isScannerReactionAutomator)
        {
            if (scanner == null || scannerInitialTransform == null || scannerTransform1 == null || scannerTransform2 == null)
            {
                Debug.LogError("LaserReactionAutomator: Scanner references (scanner, initial, transform1, transform2) are missing for Scanner demo.", this);
                return;
            }
            _currentDemoCoroutine = StartCoroutine(ScannerReactionAutomatorSequence());
        }
        else if (isLaserReactionAutomator)
        {
            if (_currentWeapon == null || _characterReactionHandler == null)
            {
                Debug.LogError("LaserReactionAutomator: References to IWeapon or CharacterReactionHandler are missing for LaserGun demo.", this);
                return;
            }
            _currentDemoCoroutine = StartCoroutine(RunLaserGunDemoSequence());
        }
        else
        {
            Debug.LogWarning("LaserReactionAutomator: No demo type (Scanner or Laser) is selected to start.");
        }
    }

    private IEnumerator RunLaserGunDemoSequence()
    {
        // This is a crucial check to prevent errors
        if (_currentWeapon == null)
        {
            Debug.LogError("IWeapon reference is null. Cannot run laser demo sequence.", this);
            yield break;
        }

        // Activate the weapon using the IWeapon interface
        _currentWeapon.Activate();

        while (true)
        {
            yield return new WaitForSeconds(laserActivationDelay);
            Debug.Log("RunDemoSequence - Laser Firing");
        
            // Call the FireAtCharacter method on the IWeapon interface
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
        // --- UPDATED: Deactivate weapon using the interface ---
        if (_currentWeapon != null)
        {
           _currentWeapon.Deactivate();
        }
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