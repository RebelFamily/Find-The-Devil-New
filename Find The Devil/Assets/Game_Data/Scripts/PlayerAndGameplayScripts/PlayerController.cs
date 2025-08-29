using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using DG.Tweening;
//using GameAnalyticsSDK;
using ITHappy;
using UnityEngine.EventSystems;

public enum ActiveTool { LaserGun, Scanner }

public class PlayerController : MonoBehaviour
{
    [Header("Tool References")] 
    [SerializeField] private SplineFollower splineFollower;
    [SerializeField] private Camera mainCamer; 
    [SerializeField] private GameObject toolhandler; 
    
    // --- CHANGED: Use IWeapon interface for the weapon reference ---
    private IWeapon _currentLaserGun;
    private ScannerLogic _currentScanner;
    
    // --- FIXED: Make RotationAnimator an Inspector reference ---
    [Header("Camera Rotation References")]
    [SerializeField] private RotationAnimator cameraRotationAnimator;
    public RotationAnimator _cameraRotationAnimator => cameraRotationAnimator;
    
    
    // --- CHANGED: Return IWeapon instead of LaserGunController ---
    public IWeapon CurrentLaserGun => _currentLaserGun;
    public ScannerLogic CurrentScanner => _currentScanner;

    [Header("Tool Positioning Transforms")]
    public Transform scannerUsePos;
    public Transform gunUsePos;
    public Transform scannerSwitchPos; 
    public Transform gunSwitchPos;
    [SerializeField] private Transform _toolEquipParent; 

    [Header("Initial Setup")]
    [Tooltip("Which tool should be active when the game starts.")]
    [SerializeField] private ActiveTool defaultActiveTool;
    [SerializeField] private GameObject defaultLaserGunPrefab; 
    [SerializeField] private GameObject defaultScannerPrefab;
    
    [Header("Tool Switch Animation")]
    [SerializeField] private float switchDuration = 0.3f; 
    [SerializeField] private Ease switchEaseType = Ease.OutBack; 
    
    [Header("Cutscene Camera Settings")]
    [SerializeField] private Transform mainGameCamera; 
    [SerializeField] private float cutsceneCameraAnimationDuration = 1.5f; 
    [SerializeField] private Ease cutsceneCameraEaseType = Ease.InOutQuad;

    [Header("Rescue Level Camera Color")] 
    [SerializeField] private Color rescueLevelCameraColor = Color.blue; 
    [SerializeField] private Color defaultLevelCameraColor = Color.blue; 
    [SerializeField] private Color rescueLevel12CameraColor = Color.blue; 
    
    private bool _isLaserGunActive;
    private bool _inCutscene = false;
    public bool _capturedSceneSkipped = false;
    private Vector3 _originalCameraLocalPosition;
    private Quaternion _originalCameraLocalRotation; 
    public Transform enemyLookAt;
    
    
    public static PlayerController Instance { get; private set; } 
    
    public static event Action OnWeaponSwitched;

    public Transform toolHandler;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        InputManager.OnTap += OnTap;
    }

    public void Init()
    {
        defaultLaserGunPrefab = GameManager.Instance.shopManager.GetEquippedWeaponPrefab(); 
        defaultScannerPrefab = GameManager.Instance.shopManager.GetEquippedScannerPrefab(); 

        if (_currentLaserGun == null && defaultLaserGunPrefab != null)
        {
            EquipLaserGun(defaultLaserGunPrefab);
        }
        if (_currentScanner == null && defaultScannerPrefab != null)
        {
            EquipScanner(defaultScannerPrefab);
        }

        if (_currentLaserGun != null) _currentLaserGun.Init();
        if (_currentScanner != null) _currentScanner.Init();

        if (splineFollower != null)
        {

            splineFollower.spline = GameManager.Instance.levelManager._currentLevelObj.GetComponent<SplineData>().GetSplineData();
            splineFollower.Restart();
            splineFollower.followSpeed = 0;
            splineFollower.follow = true;
        }
        
        if (mainCamer != null && GameManager.Instance.levelManager != null)
        {
            if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue )
            {
        
                 if(GameManager.Instance.levelManager._currentLevelNumber != 11 || GameManager.Instance.levelManager._currentLevelNumber != 23)
                     mainCamer.backgroundColor = rescueLevelCameraColor;
                 else
                 {
                     mainCamer.backgroundColor = rescueLevel12CameraColor;
                 }
                RenderSettings.fog = false;
            }
            else
            {
                ResetCameraAngles(0.01f);
                mainCamer.backgroundColor = defaultLevelCameraColor; 
                RenderSettings.fog = true;
            }
        } 
     
    }

    public void ResetSpline()
    {
     
        splineFollower.Restart();
        
    }
    
    public void GoToLastSplinePoint()
    {
        if (splineFollower != null)
        {
            _capturedSceneSkipped = true;
            splineFollower.SetPercent(.995);
            splineFollower.followSpeed = 3;
            splineFollower.follow = true;
            
            Debug.Log("Player has been moved to the last spline point.");
        }
    }

   
    
    public void ResetCameraAngles(float time)
    {

        if (cameraRotationAnimator != null)
        {
            splineFollower.followSpeed = 0.01f;
            cameraRotationAnimator.ResetRotationToOriginalRotation(time);
        }
    }
    
    private void OnDestroy()
    {
        InputManager.OnTap -= OnTap;
        if (_currentLaserGun != null) Destroy((_currentLaserGun as MonoBehaviour).gameObject);
        if (_currentScanner != null) Destroy(_currentScanner.gameObject);
    }
    
    private void OnTap(Vector2 position)
    {
        if (_inCutscene) return; 

        bool isPointerOverUI = false;

        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        #else
                if (Input.touchCount > 0)
                {
                    isPointerOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                }
        #endif
        
                if (isPointerOverUI )
                {
                    return; 
                }
                
        if (_isLaserGunActive)
        {
            Shoot(position);
        }
        else 
        { 
            ActivateScanner();
        }
    }
    
    public void Shoot(Vector2 pos)
    {
        // --- CHANGED: Call HandleTapInput on the IWeapon reference ---
        if (_currentLaserGun != null && (_currentLaserGun as MonoBehaviour).gameObject.activeInHierarchy)
        {
            _currentLaserGun.HandleTapInput(pos);
        }
    }
     
    public void ActivateScanner()
    {
        if (_currentScanner != null && _currentScanner.gameObject.activeInHierarchy)
        { 
            //scanner.ScanArea(); // Keep this logic as per your ScannerLogic implementation
        }
    }
 
    public IEnumerator CheckForTutorialLevel(bool check)
    {
        GameManager.Instance.uiManager.HideAllPanels();
        
        // Game Analytics
       // AnalyticsManager.Instance.ProgressionEventSingleMode(GAProgressionStatus.Start, (GameManager.Instance.levelManager.GlobalLevelNumber+1).ToString());
        
       
        if (check)   
        {
            GameManager.Instance.uiManager.HidePanel(UIPanelType.TutorialPanel);
           // GameManager.Instance.uiManager.ShowPanel(UIPanelType.GameOverlayPanel);
            splineFollower.followSpeed = GameManager.Instance.levelManager.CurrentLevel.GetPlayerMovementSpeed();
            Debug.Log("SwitchBlocker(true)");
            //SetupTools(); 
            //yield return new WaitForSecondsRealtime(0.2f);
            
            yield return new WaitForSecondsRealtime(4.8f);
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.LevelObjectivesPanel);
            GameManager.Instance.uiManager.ShowTutorialPanel(UIPanelType.TutorialPanel);
            yield return new WaitForSecondsRealtime(1f);
            GameManager.Instance.uiManager.ActivateVIPGun();
   
        }
        else
        { 
            GameManager.Instance.uiManager.UpdateTutorialPanel(UIPanelType.TutorialPanel);

            Init(); 
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.LevelObjectivesPanel);
            GameManager.Instance.uiManager.ActivateVIPGun();
            splineFollower.followSpeed = GameManager.Instance.levelManager.CurrentLevel.GetPlayerMovementSpeed();
        }
    }
    
    public void SetupTools() 
    { 
        
        if (_currentLaserGun == null || _currentScanner == null || scannerUsePos == null || gunUsePos == null || scannerSwitchPos == null || gunSwitchPos == null)
        {
            Debug.LogError("PlayerController: One or more tool references or position transforms are null during SetupTools.");
            return;
        }
        
        if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
            GameManager.Instance.uiManager.ShowPanel(UIPanelType.GameOverlayPanel);
        
        // --- CHANGED: Use the IWeapon reference directly ---
        
        if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
            (_currentLaserGun as MonoBehaviour).GetComponent<ITweenMagic>().enabled = false;
            
            
        (_currentLaserGun as MonoBehaviour).gameObject.SetActive(true);
        _currentScanner.gameObject.SetActive(true);
        
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
        {
            
             _currentLaserGun.Activate();
            (_currentLaserGun as MonoBehaviour).transform.SetPositionAndRotation(gunUsePos.position, gunUsePos.rotation);
            (_currentLaserGun as MonoBehaviour).transform.localScale = gunUsePos.localScale;
           
            _currentScanner.transform.SetPositionAndRotation(scannerSwitchPos.position, scannerSwitchPos.rotation);
            _currentScanner.transform.localScale = scannerSwitchPos.localScale;
            _currentScanner.Deactivate(); 
            _currentScanner.gameObject.SetActive(false); 
            //GameManager.Instance.uiManager.HidePanel(UIPanelType.GameOverlayPanel);
            
            _isLaserGunActive = true;
        }
        else if (defaultActiveTool == ActiveTool.LaserGun) 
        { 
            (_currentLaserGun as MonoBehaviour).transform.SetPositionAndRotation(gunUsePos.position, gunUsePos.rotation);
            (_currentLaserGun as MonoBehaviour).transform.localScale = gunUsePos.localScale; 
            _currentLaserGun.Activate();
            
            _currentScanner.transform.SetPositionAndRotation(scannerSwitchPos.position, scannerSwitchPos.rotation); 
            _currentScanner.transform.localScale = scannerSwitchPos.localScale;
            _currentScanner.Deactivate();  
        
            _isLaserGunActive = true;
        }
        else  
        {
            _currentScanner.transform.SetPositionAndRotation(scannerUsePos.position, scannerUsePos.rotation);
            _currentScanner.transform.localScale = scannerUsePos.localScale;
            _currentScanner.Activate();
             
            (_currentLaserGun as MonoBehaviour).transform.SetPositionAndRotation(gunSwitchPos.position, gunSwitchPos.rotation);
            (_currentLaserGun as MonoBehaviour).transform.localScale = gunSwitchPos.localScale;
            _currentLaserGun.Deactivate();
        
            _isLaserGunActive = false;
        }
        
        // --- CHANGED: Use the IWeapon reference directly ---
        if(GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
            (_currentLaserGun as MonoBehaviour).gameObject.GetComponent<ITweenMagic>()?.PlayScale();
        
        _currentScanner.gameObject.GetComponent<ITweenMagic>()?.PlayScale();
    }
    
    public void GunScannerSwitch()
    {
        
        
        if (GameManager.Instance._waitForTryGun && !GameManager.Instance._isReachedPoint)
        {
            return;
        }
        
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() == LevelType.Rescue)
        {
            return; 
        }

        if (_currentLaserGun == null || _currentScanner == null || 
            scannerUsePos == null || gunUsePos == null || 
            scannerSwitchPos == null || gunSwitchPos == null)
        {
            Debug.LogError("PlayerController: One or more tool references or position transforms are null during GunScannerSwitch.");
            return;
        }
        Vibration.VibratePop();
        // --- CHANGED: Use the IWeapon reference directly ---
        (_currentLaserGun as MonoBehaviour).transform.DOKill(true);  
        _currentScanner.transform.DOKill(true);
 
        if (_isLaserGunActive) 
        {
            _currentScanner.gameObject.SetActive(true); 
            
            _currentScanner.transform.DOMove(scannerUsePos.position, switchDuration).SetEase(switchEaseType).OnComplete(() =>
            {
                _currentScanner.Activate();
            });
            _currentScanner.transform.DORotate(scannerUsePos.rotation.eulerAngles, switchDuration).SetEase(switchEaseType);
            _currentScanner.transform.DOScale(scannerUsePos.localScale, switchDuration).SetEase(switchEaseType);
            
            (_currentLaserGun as MonoBehaviour).transform.DOMove(gunSwitchPos.position, switchDuration).SetEase(switchEaseType);
            (_currentLaserGun as MonoBehaviour).transform.DORotate(gunSwitchPos.rotation.eulerAngles, switchDuration).SetEase(switchEaseType);
            (_currentLaserGun as MonoBehaviour).transform.DOScale(gunSwitchPos.localScale, switchDuration).SetEase(switchEaseType)
                     .OnComplete(() => {
                         _currentLaserGun.Deactivate();
                     }); 

            _isLaserGunActive = false;
        }
        else 
        {
            (_currentLaserGun as MonoBehaviour).gameObject.SetActive(true);

            (_currentLaserGun as MonoBehaviour).transform.DOMove(gunUsePos.position, switchDuration).SetEase(switchEaseType);
            (_currentLaserGun as MonoBehaviour).transform.DORotate(gunUsePos.rotation.eulerAngles, switchDuration).SetEase(switchEaseType);
            (_currentLaserGun as MonoBehaviour).transform.DOScale(gunUsePos.localScale, switchDuration).SetEase(switchEaseType);
            _currentLaserGun.Activate(); 
            
            _currentScanner.transform.DOMove(scannerSwitchPos.position, switchDuration).SetEase(switchEaseType);
            _currentScanner.transform.DORotate(scannerSwitchPos.rotation.eulerAngles, switchDuration).SetEase(switchEaseType);
            _currentScanner.transform.DOScale(scannerSwitchPos.localScale, switchDuration).SetEase(switchEaseType)
                     .OnComplete(() => {
                         _currentScanner.Deactivate();
                     });

            _isLaserGunActive = true;
        }
        OnWeaponSwitched?.Invoke(); 
    }
 
    public void ResetTools()
    {
        // --- CHANGED: Use the IWeapon reference directly ---
        if (_currentLaserGun != null) (_currentLaserGun as MonoBehaviour).gameObject.SetActive(false);
        if (_currentScanner != null) _currentScanner.gameObject.SetActive(false);
    }

    public IEnumerator StopMovementFor(float time)
    {
        splineFollower.follow = false;
        yield return new WaitForSecondsRealtime(time);
        splineFollower.follow = true;
    }
   
    public void SetNewMovementSpeed(float speed)
    {
        splineFollower.followSpeed = speed;
    }

    public IEnumerator SetNewMovementSpeed(float speed,float wait)
    {
        yield return new WaitForSecondsRealtime(wait-1);
        GameManager.Instance.playerController.GunScannerSwitch();
        yield return new WaitForSecondsRealtime(1f);
        splineFollower.followSpeed = speed;
    }
    
    public IEnumerator KingDevilCutScene(Transform targetLookAt)
    {
        _inCutscene = true;
        splineFollower.follow = false;
    
        ResetTools();
        GameManager.Instance.uiManager.HidePanel(UIPanelType.GameOverlayPanel);
    
        if (mainGameCamera != null)
        {
            _originalCameraLocalPosition = mainGameCamera.transform.localPosition;
        }

        //yield return new WaitForSecondsRealtime(1f);

        if (mainGameCamera == null)
        {
            Debug.LogError("PlayerController: Main Camera reference is not assigned for KingDevilCutScene. Cannot animate camera.");
        }
        else
        {
            // Debug.Log("targetLookAt.position = " + targetLookAt.position);
            // // Animate the camera's world position to the target's world position
            // yield return mainGameCamera.transform.DOMove(targetLookAt.position, cutsceneCameraAnimationDuration).SetEase(cutsceneCameraEaseType).WaitForCompletion();
            //
            Debug.Log("targetLookAt.position = " + targetLookAt.position);
        
            // Animate the camera's world position to the target's world position
            yield return  mainGameCamera.transform.DOMove(targetLookAt.position, cutsceneCameraAnimationDuration).SetEase(cutsceneCameraEaseType);
            yield return new WaitForSecondsRealtime(0.76f);
            // Add a camera shake effect
            yield return mainGameCamera.transform.DOShakePosition(cutsceneCameraAnimationDuration, 0.2f, 20, 90, false, true).WaitForCompletion();
            
        }
    
        GameManager.Instance.uiManager.PlayKingDevilDialogues();
    
        yield return new WaitForSecondsRealtime(10f);
    
        if (mainGameCamera != null)
        {
            // For the return animation, keep DOLocalMove to return to the original local position
            yield return mainGameCamera.transform.DOLocalMove(_originalCameraLocalPosition, cutsceneCameraAnimationDuration).SetEase(cutsceneCameraEaseType).WaitForCompletion();
           
        }
    
        
        _inCutscene = false;
        splineFollower.follow = true;
    }
    
    // --- NEW: A single, level-driven camera rotation method ---
    public void StartCameraRotationForLevel(int levelNumber)
    {
        //Reseted for the 15th level
        //ResetTools(); // Ensures tools are hidden during a cutscene
        if (cameraRotationAnimator != null)
        {
            cameraRotationAnimator.StartRotationForLevel(levelNumber);
        }
        
    }

    public void RotateToTarget(Vector3 targetRotation, float duration)
    {
        if (cameraRotationAnimator != null)
        {
            cameraRotationAnimator.RotateToTarget(targetRotation, duration,Ease.InSine,true);
        }
        
    }
    
    
    
    public void StartCityMetaRotation(int levelNumber)
    {
        ResetTools(); // Ensures tools are hidden during a cutscene
        if (cameraRotationAnimator != null)
        {
            cameraRotationAnimator.StartCityMetaRotationForLevel(levelNumber);
        }
        
    }
    
    // --- REMOVED: Redundant methods. Use the new one instead. ---
    // public void RescuedLevelCutScene()
    // {
    //     ResetTools();
    //     mainGameCamera.GetComponent<RotationAnimator>().StartRotationAnimation();
    // }
    // public void LadderLevelCutScene()
    // {
    //     mainGameCamera.GetComponent<RotationAnimator>().StartRotationLadderLevel(55,2f);
    // }
    
    public void EquipTool(IShopItem item)
    {
        if (item == null || item.GetPrefabToUnlock() == null)
        {
            Debug.LogError("PlayerController: Attempted to equip a null item or an item with no prefab.");
            return;
        }

        switch (item.GetItemType())
        {
            case ShopItemType.Weapon: 
                EquipLaserGun(item.GetPrefabToUnlock());
                break;
            case ShopItemType.Scanner: 
                EquipScanner(item.GetPrefabToUnlock());
                break;
            default:
                Debug.LogWarning($"PlayerController: Attempted to equip unsupported item type: {item.GetItemType()}");
                break;
        }
        
        SetupTools();
    }
    
    public void EquipLaserGun(GameObject gunPrefab)
    {
        if (_currentLaserGun != null)
        {
            Destroy((_currentLaserGun as MonoBehaviour).gameObject); 
        }

        GameObject newGunGO = Instantiate(gunPrefab, _toolEquipParent);
        _currentLaserGun = newGunGO.GetComponent<IWeapon>();

        if (_currentLaserGun == null)
        {
            Debug.LogError($"PlayerController: Equipped LaserGun prefab '{gunPrefab.name}' does not have a component that implements IWeapon!");
            return;
        }
        _currentLaserGun.Init(); 
    }
    
    public void EquipScanner(GameObject scannerPrefab)
    {
        if (_currentScanner != null)
        {
            Destroy(_currentScanner.gameObject); 
        }

        GameObject newScannerGO = Instantiate(scannerPrefab, _toolEquipParent);
        _currentScanner = newScannerGO.GetComponent<ScannerLogic>();

        if (_currentScanner == null)
        {
           Debug.LogError($"PlayerController: Equipped Scanner prefab '{scannerPrefab.name}' does not have a ScannerLogic component!");
            return;
        }
        _currentScanner.Init(); 
    }

    public float GetPlayerMovementSpeed()
    {
        return splineFollower.followSpeed;
    }

}